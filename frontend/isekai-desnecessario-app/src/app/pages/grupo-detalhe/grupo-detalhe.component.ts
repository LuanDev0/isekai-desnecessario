import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { LanguageService } from '../../services/language.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { IconComponent } from '../../components/icon/icon.component';
import { GrupoDetalhe, GrupoHabito, GrupoMissao, GrupoRecompensa, PlanoGrupo } from '../../models/models';
import { environment } from '../../../environments/environment';

const API_BASE = environment.apiUrl.replace('/api', '');

type Aba = 'feed' | 'ranking' | 'conteudo' | 'gerenciar';

@Component({
  selector: 'app-grupo-detalhe',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe, IconComponent],
  templateUrl: './grupo-detalhe.component.html',
  styleUrl: './grupo-detalhe.component.scss',
})
export class GrupoDetalheComponent implements OnInit {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  private route   = inject(ActivatedRoute);
  private router  = inject(Router);
  readonly lang   = inject(LanguageService);

  grupoId = 0;
  perfilId = 0;
  carregando = true;
  grupo: GrupoDetalhe | null = null;
  aba: Aba = 'feed';
  erro = '';

  // Gestão — convite
  emailConvite = '';
  conviteFeedback = '';

  // Gestão — formulários de conteúdo
  novoHabito = { habito: '', xp: 50, frequencia: 'Diário' };
  criandoHabito = false;
  editandoHabitoId: number | null = null;
  editHabito = { habito: '', xp: 50, frequencia: 'Diário' };

  novaMissao = { titulo: '', recompensaXp: 100, recompensaMoedas: 20, dataLimite: '' };
  criandoMissao = false;
  editandoMissaoId: number | null = null;
  editMissao = { titulo: '', recompensaXp: 100, recompensaMoedas: 20, dataLimite: '' };

  novaRecompensa = { nome: '', custo: 100 };
  criandoRecompensa = false;
  editandoRecompensaId: number | null = null;
  editRecompensa = { nome: '', custo: 100 };

  // Gestão — grupo
  renomeando = false;
  novoNome = '';
  planosUpgrade: { plano: PlanoGrupo; vagas: number }[] = [];

  readonly frequencias = ['Diário', 'Semanal', 'Mensal', 'Livre'];
  private readonly todosPlanos: { plano: PlanoGrupo; vagas: number }[] = [
    { plano: 'Starter', vagas: 5 }, { plano: 'Standard', vagas: 10 },
    { plano: 'Pro', vagas: 30 },    { plano: 'Max', vagas: 50 },
  ];

  ngOnInit() {
    const savedId = this.profile.getSavedId();
    if (!savedId) { this.router.navigate(['/cadastro']); return; }
    this.perfilId = savedId;
    this.grupoId = Number(this.route.snapshot.paramMap.get('id'));
    this.carregar();
  }

  carregar() {
    this.carregando = true;
    this.erro = '';
    this.api.getGrupoDetalhe(this.grupoId, this.perfilId).subscribe({
      next: g => {
        this.grupo = g;
        this.carregando = false;
        this.planosUpgrade = this.todosPlanos.filter(p => p.vagas > g.maxMembros);
        if (this.aba === 'gerenciar' && !g.organizador) this.aba = 'feed';
      },
      error: e => {
        this.carregando = false;
        this.erro = typeof e?.error === 'string' ? e.error : 'Erro ao carregar o grupo.';
      },
    });
  }

  voltar() { this.router.navigate(['/grupos']); }

  private erroMsg(e: any, padrao: string): string {
    return typeof e?.error === 'string' ? e.error : padrao;
  }

  // ── Conteúdo (membro) ─────────────────────────────────
  completarHabito(h: GrupoHabito) {
    if (!h.disponivel) return;
    this.api.completarHabitoGrupo(h.id, this.perfilId).subscribe({
      next: () => this.carregar(),
      error: e => alert(this.erroMsg(e, 'Erro ao completar hábito.')),
    });
  }

  completarMissao(m: GrupoMissao) {
    if (m.concluida || this.prazoEncerrado(m)) return;
    this.api.completarMissaoGrupo(m.id, this.perfilId).subscribe({
      next: () => this.carregar(),
      error: e => alert(this.erroMsg(e, 'Erro ao concluir missão.')),
    });
  }

  resgatarRecompensa(r: GrupoRecompensa) {
    this.api.resgatarRecompensaGrupo(r.id, this.perfilId).subscribe({
      next: () => this.carregar(),
      error: e => alert(this.erroMsg(e, 'Erro ao resgatar recompensa.')),
    });
  }

  prazoEncerrado(m: GrupoMissao): boolean {
    return !!m.dataLimite && new Date(m.dataLimite).getTime() < Date.now();
  }

  podeResgatar(r: GrupoRecompensa): boolean {
    return (this.grupo?.membro?.moedasGrupo ?? 0) >= r.custo;
  }

  // ── Gestão: hábitos ───────────────────────────────────
  criarHabito() {
    if (!this.novoHabito.habito.trim()) return;
    this.api.criarHabitoGrupo(this.grupoId, this.novoHabito).subscribe({
      next: () => {
        this.novoHabito = { habito: '', xp: 50, frequencia: 'Diário' };
        this.criandoHabito = false;
        this.carregar();
      },
      error: e => alert(this.erroMsg(e, 'Erro ao criar hábito.')),
    });
  }

  iniciarEdicaoHabito(h: GrupoHabito) {
    this.editandoHabitoId = h.id;
    this.editHabito = { habito: h.habito, xp: h.xp, frequencia: h.frequencia };
  }

  salvarHabito() {
    if (this.editandoHabitoId == null) return;
    this.api.editarHabitoGrupo(this.editandoHabitoId, this.editHabito).subscribe({
      next: () => { this.editandoHabitoId = null; this.carregar(); },
      error: e => alert(this.erroMsg(e, 'Erro ao salvar hábito.')),
    });
  }

  excluirHabito(h: GrupoHabito) {
    this.api.excluirHabitoGrupo(h.id).subscribe({ next: () => this.carregar() });
  }

  // ── Gestão: missões ───────────────────────────────────
  criarMissao() {
    if (!this.novaMissao.titulo.trim()) return;
    this.api.criarMissaoGrupo(this.grupoId, {
      ...this.novaMissao,
      dataLimite: this.novaMissao.dataLimite || null,
    }).subscribe({
      next: () => {
        this.novaMissao = { titulo: '', recompensaXp: 100, recompensaMoedas: 20, dataLimite: '' };
        this.criandoMissao = false;
        this.carregar();
      },
      error: e => alert(this.erroMsg(e, 'Erro ao criar missão.')),
    });
  }

  iniciarEdicaoMissao(m: GrupoMissao) {
    this.editandoMissaoId = m.id;
    this.editMissao = {
      titulo: m.titulo, recompensaXp: m.recompensaXp, recompensaMoedas: m.recompensaMoedas,
      dataLimite: m.dataLimite ? m.dataLimite.substring(0, 10) : '',
    };
  }

  salvarMissao() {
    if (this.editandoMissaoId == null) return;
    this.api.editarMissaoGrupo(this.editandoMissaoId, {
      ...this.editMissao,
      dataLimite: this.editMissao.dataLimite || null,
    }).subscribe({
      next: () => { this.editandoMissaoId = null; this.carregar(); },
      error: e => alert(this.erroMsg(e, 'Erro ao salvar missão.')),
    });
  }

  excluirMissao(m: GrupoMissao) {
    this.api.excluirMissaoGrupo(m.id).subscribe({ next: () => this.carregar() });
  }

  // ── Gestão: recompensas ───────────────────────────────
  criarRecompensa() {
    if (!this.novaRecompensa.nome.trim()) return;
    this.api.criarRecompensaGrupo(this.grupoId, this.novaRecompensa).subscribe({
      next: () => {
        this.novaRecompensa = { nome: '', custo: 100 };
        this.criandoRecompensa = false;
        this.carregar();
      },
      error: e => alert(this.erroMsg(e, 'Erro ao criar recompensa.')),
    });
  }

  iniciarEdicaoRecompensa(r: GrupoRecompensa) {
    this.editandoRecompensaId = r.id;
    this.editRecompensa = { nome: r.nome, custo: r.custo };
  }

  salvarRecompensa() {
    if (this.editandoRecompensaId == null) return;
    this.api.editarRecompensaGrupo(this.editandoRecompensaId, this.editRecompensa).subscribe({
      next: () => { this.editandoRecompensaId = null; this.carregar(); },
      error: e => alert(this.erroMsg(e, 'Erro ao salvar recompensa.')),
    });
  }

  excluirRecompensa(r: GrupoRecompensa) {
    this.api.excluirRecompensaGrupo(r.id).subscribe({ next: () => this.carregar() });
  }

  // ── Gestão: convites e membros ────────────────────────
  convidar() {
    const email = this.emailConvite.trim();
    if (!email) return;
    this.conviteFeedback = '';
    this.api.convidarParaGrupo(this.grupoId, email).subscribe({
      next: () => {
        this.emailConvite = '';
        this.conviteFeedback = this.lang.translate('grupos.conviteEnviado');
        this.carregar();
      },
      error: e => this.conviteFeedback = this.erroMsg(e, 'Erro ao enviar convite.'),
    });
  }

  removerMembro(membroId: number) {
    if (!confirm(this.lang.translate('grupos.confirmaRemover'))) return;
    this.api.removerMembroGrupo(this.grupoId, membroId).subscribe({
      next: () => this.carregar(),
      error: e => alert(this.erroMsg(e, 'Erro ao remover membro.')),
    });
  }

  sair() {
    if (!confirm(this.lang.translate('grupos.confirmaSair'))) return;
    this.api.sairDoGrupo(this.grupoId, this.perfilId).subscribe({
      next: () => this.voltar(),
      error: e => alert(this.erroMsg(e, 'Erro ao sair do grupo.')),
    });
  }

  // ── Gestão: grupo ─────────────────────────────────────
  renomear() {
    const nome = this.novoNome.trim();
    if (!nome) return;
    this.api.renomearGrupo(this.grupoId, nome).subscribe({
      next: () => { this.renomeando = false; this.carregar(); },
      error: e => alert(this.erroMsg(e, 'Erro ao renomear grupo.')),
    });
  }

  upgrade(plano: PlanoGrupo) {
    this.api.upgradeGrupo(this.grupoId, plano).subscribe({
      next: () => this.carregar(),
      error: e => alert(this.erroMsg(e, 'Erro ao fazer upgrade.')),
    });
  }

  cancelar() {
    if (!confirm(this.lang.translate('grupos.confirmaCancelar'))) return;
    this.api.cancelarGrupo(this.grupoId).subscribe({ next: () => this.carregar() });
  }

  reativar() {
    this.api.reativarGrupo(this.grupoId).subscribe({ next: () => this.carregar() });
  }

  excluir() {
    if (!confirm(this.lang.translate('grupos.confirmaExcluir'))) return;
    this.api.excluirGrupo(this.grupoId).subscribe({ next: () => this.voltar() });
  }

  // ── Helpers de exibição ───────────────────────────────
  fotoUrl(fotoUrl: string | null | undefined): string | null {
    if (!fotoUrl) return null;
    if (fotoUrl.startsWith('data:')) return fotoUrl;
    if (fotoUrl.startsWith('/')) return `${API_BASE}${fotoUrl}`;
    return null;
  }

  feedEmoji(tipo: string): string {
    switch (tipo) {
      case 'entrou':     return '👋';
      case 'saiu':       return '🚪';
      case 'removido':   return '🚪';
      case 'habito':     return '✅';
      case 'missao':     return '🏆';
      case 'recompensa': return '🎁';
      case 'conteudo':   return '✨';
      default:           return '📣';
    }
  }

  // Tempo relativo simples ("agora", "5min", "3h", data) — mesmo padrão das notificações.
  quando(data: string): string {
    const d = new Date(data);
    const diff = Math.floor((Date.now() - d.getTime()) / 60000);
    if (diff < 1)    return this.lang.translate('dashboard.agora');
    if (diff < 60)   return this.lang.translate('dashboard.minAtras', { min: diff });
    if (diff < 1440) return this.lang.translate('dashboard.hAtras', { h: Math.floor(diff / 60) });
    const locale = this.lang.current() === 'en' ? 'en-US' : 'pt-BR';
    return d.toLocaleDateString(locale, { day: '2-digit', month: '2-digit' });
  }

  dataCurta(data: string): string {
    const locale = this.lang.current() === 'en' ? 'en-US' : 'pt-BR';
    return new Date(data).toLocaleDateString(locale, { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  posicao(i: number): string {
    if (i === 0) return '🥇';
    if (i === 1) return '🥈';
    if (i === 2) return '🥉';
    return `${i + 1}º`;
  }
}
