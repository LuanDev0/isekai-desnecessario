import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CommonModule, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { LanguageService, LangCode } from '../../services/language.service';
import { ProfileSelectorComponent } from '../../components/profile-selector/profile-selector.component';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { IconComponent } from '../../components/icon/icon.component';
import { BomHabito, DiarioAcao, MauHabito, Perfil } from '../../models/models';

const API_BASE = environment.apiUrl.replace('/api', '');

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, DatePipe, ProfileSelectorComponent, TranslatePipe, IconComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit, OnDestroy {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  private router  = inject(Router);
  readonly lang   = inject(LanguageService);

  onLangChange(event: Event) {
    const code = (event.target as HTMLSelectElement).value as LangCode;
    this.lang.setLanguage(code);
  }

  perfil: Perfil | null = null;
  bonsHabitos: BomHabito[] = [];
  mausHabitos: MauHabito[] = [];
  diario: DiarioAcao[] = [];
  fotoPreview: string | null = null;
  fotoErro = false;
  agora = new Date();
  private timer: ReturnType<typeof setInterval> | null = null;

  ngOnInit() {
    const savedId = this.profile.getSavedId();
    if (!savedId) {
      this.router.navigate(['/cadastro']);
      return;
    }
    // Sinal vazio (app recém aberto) → busca perfil e seta antes de carregar
    if (!this.profile.perfilAtivo()) {
      this.api.getPerfil(savedId).subscribe({
        next: p => { this.profile.setPerfilAtivo(p); this.carregarDados(); },
        error: () => { this.profile.clearPerfil(); this.router.navigate(['/cadastro']); }
      });
    } else {
      this.carregarDados();
    }
    this.timer = setInterval(() => this.agora = new Date(), 1000);
  }

  ngOnDestroy() {
    if (this.timer) clearInterval(this.timer);
  }

  carregarDados() {
    const id = this.profile.id;
    this.api.getPerfil(id).subscribe({ next: p => { this.perfil = p; this.profile.setPerfilAtivo(p); } });
    this.api.getBonsHabitos(id).subscribe({ next: h => this.bonsHabitos = this.ordenarHabitos(h) });
    this.api.getMausHabitos(id).subscribe({ next: h => this.mausHabitos = this.ordenarHabitos(h) });
    this.api.getDiario(id).subscribe({ next: d => this.diario = d });
  }

  // ── Disponibilidade ───────────────────────────────────────────────
  estaDisponivel(h: BomHabito | MauHabito): boolean {
    if (h.frequencia === 'Livre') return true;
    if (!h.ultimaExecucao) return true;

    const ultima = new Date(h.ultimaExecucao);
    const agora  = new Date();

    switch (h.frequencia) {
      case 'Diário':
        return ultima < new Date(agora.getFullYear(), agora.getMonth(), agora.getDate());
      case 'Semanal': {
        const domingo = new Date(agora);
        domingo.setHours(0, 0, 0, 0);
        domingo.setDate(agora.getDate() - agora.getDay());
        return ultima < domingo;
      }
      case 'Mensal':
        return ultima < new Date(agora.getFullYear(), agora.getMonth(), 1);
      default:
        return true;
    }
  }

  proximoReset(h: BomHabito | MauHabito): string {
    if (h.frequencia === 'Livre' || !h.ultimaExecucao) return '';
    const agora = new Date();
    let reset: Date;

    switch (h.frequencia) {
      case 'Diário':
        reset = new Date(agora.getFullYear(), agora.getMonth(), agora.getDate() + 1);
        break;
      case 'Semanal': {
        const diasAteDom = 7 - agora.getDay();
        reset = new Date(agora.getFullYear(), agora.getMonth(), agora.getDate() + diasAteDom);
        break;
      }
      case 'Mensal':
        reset = new Date(agora.getFullYear(), agora.getMonth() + 1, 1);
        break;
      default:
        return '';
    }

    const diff = reset.getTime() - agora.getTime();
    const h_ = Math.floor(diff / 3600000);
    const m  = Math.floor((diff % 3600000) / 60000);
    if (h_ >= 24) return this.lang.translate('dashboard.dias', { d: Math.floor(h_ / 24) });
    if (h_ > 0)   return this.lang.translate('dashboard.horasMinutos', { h: h_, m });
    return this.lang.translate('dashboard.minutos', { m });
  }

  private ordenarHabitos<T extends BomHabito | MauHabito>(lista: T[]): T[] {
    return [...lista].sort((a, b) => {
      const dispA = this.estaDisponivel(a) ? 0 : 1;
      const dispB = this.estaDisponivel(b) ? 0 : 1;
      if (dispA !== dispB) return dispA - dispB;
      return a.xp - b.xp;
    });
  }

  get fotoAtual(): string | null {
    if (this.fotoErro) return null;
    if (this.fotoPreview) return this.fotoPreview;
    const url = this.perfil?.fotoUrl;
    if (!url) return null;
    if (url.startsWith('data:')) return url;
    if (url.startsWith('/')) return `${API_BASE}${url}`;
    return null;
  }

  onFotoErro() { this.fotoErro = true; this.fotoPreview = null; }

  onFotoSelecionada(event: Event) {
    const input = event.target as HTMLInputElement;
    const arquivo = input.files?.[0];
    if (!arquivo) return;
    this.fotoErro = false;
    const reader = new FileReader();
    reader.onload = e => this.fotoPreview = e.target?.result as string;
    reader.readAsDataURL(arquivo);
    this.api.uploadFoto(this.perfil!.id, arquivo).subscribe({
      next: p => {
        this.perfil = p;
        this.fotoPreview = null;
        this.profile.setPerfilAtivo(p); // atualiza signal + fotoVersion no service
      }
    });
  }

  get xpPercent(): number {
    if (!this.perfil) return 0;
    return Math.min((this.perfil.xp / this.perfil.proximoNivelXp) * 100, 100);
  }

  // ── Desafio do dia (determinístico por data) ─────────
  get desafioRecusado(): boolean {
    const recusadoEm = this.perfil?.desafioRecusadoEm;
    if (!recusadoEm) return false;
    return new Date(recusadoEm).toDateString() === new Date().toDateString();
  }

  get desafioDoDia(): BomHabito | null {
    const disponiveis = this.bonsHabitos.filter(h => this.estaDisponivel(h));
    if (!disponiveis.length) return null;
    const hoje = new Date();
    const seed = hoje.getFullYear() * 10000 + (hoje.getMonth() + 1) * 100 + hoje.getDate();
    return disponiveis[seed % disponiveis.length];
  }

  get desafioConcluido(): boolean {
    const concluidoEm = this.perfil?.desafioConcluidoEm;
    if (!concluidoEm) return false;
    return new Date(concluidoEm).toDateString() === new Date().toDateString();
  }

  recusarDesafio() {
    this.api.recusarDesafio(this.perfil!.id).subscribe({
      next: p => { this.perfil = p; this.profile.setPerfilAtivo(p); }
    });
  }

  // ── Formatação hora do diário ────────────────────────
  horaAcao(data: string): string {
    const d = new Date(data);
    const agora = new Date();
    const diff = Math.floor((agora.getTime() - d.getTime()) / 60000);
    if (diff < 1)  return this.lang.translate('dashboard.agora');
    if (diff < 60) return this.lang.translate('dashboard.minAtras', { min: diff });
    if (diff < 1440) {
      const h = Math.floor(diff / 60);
      return this.lang.translate('dashboard.hAtras', { h });
    }
    const locale = this.lang.current() === 'en' ? 'en-US' : 'pt-BR';
    return d.toLocaleDateString(locale, { day: '2-digit', month: '2-digit' });
  }

  completar(habito: BomHabito) {
    if (!this.estaDisponivel(habito)) return;
    this.aplicarXpOtimista(habito.xp);
    habito.ultimaExecucao = new Date().toISOString();
    this.api.completarBomHabito(habito.id, this.perfil!.id).subscribe(() => this.carregarDados());
  }

  completarDesafio() {
    const d = this.desafioDoDia;
    if (!d || this.desafioConcluido) return;
    this.aplicarXpOtimista(d.xp);
    this.api.completarBomHabito(d.id, this.perfil!.id).subscribe(() => {
      this.api.concluirDesafio(this.perfil!.id).subscribe(p => {
        this.perfil = p;
        this.profile.setPerfilAtivo(p);
        this.carregarDados();
      });
    });
  }

  registrar(habito: MauHabito) {
    if (!this.estaDisponivel(habito)) return;
    this.aplicarXpOtimista(-habito.xp);
    habito.ultimaExecucao = new Date().toISOString();
    this.api.registrarMauHabito(habito.id, this.perfil!.id).subscribe(() => this.carregarDados());
  }

  private aplicarXpOtimista(delta: number) {
    if (!this.perfil) return;
    this.perfil.xp = Math.max(0, this.perfil.xp + delta);
    // sobe de nível otimisticamente se passou do limite
    while (this.perfil.xp >= this.perfil.proximoNivelXp) {
      this.perfil.xp -= this.perfil.proximoNivelXp;
      this.perfil.nivel++;
    }
    // desce de nível otimisticamente se ficou negativo
    while (this.perfil.xp < 0 && this.perfil.nivel > 1) {
      this.perfil.nivel--;
      this.perfil.xp += this.perfil.proximoNivelXp;
    }
  }
}
