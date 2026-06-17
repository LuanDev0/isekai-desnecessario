import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { LanguageService } from '../../services/language.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { Perfil, Recompensa, Atributo, BomHabito, Missao } from '../../models/models';

@Component({
  selector: 'app-loja',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './loja.component.html',
  styleUrl: './loja.component.scss',
})
export class LojaComponent implements OnInit {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  readonly lang   = inject(LanguageService);
  private router  = inject(Router);

  perfil:      Perfil | null  = null;
  recompensas: Recompensa[]   = [];
  atributos:   Atributo[]     = [];
  bonsHabitos: BomHabito[]    = [];
  missoes:     Missao[]       = [];
  resgatando:  number | null  = null;
  feedbacks:   Record<number, 'ok' | 'erro' | 'sem-moedas' | 'atributo'> = {};

  // Lootbox
  lootboxDisponivel = false;
  lootboxXpHoje     = 0;
  lootboxJaAbriu    = false;
  lootboxAbrindo    = false;
  lootboxResultado: { recompensa: Recompensa; chance: number } | null = null;
  lootboxAnimando   = false;

  ngOnInit() {
    const savedId = this.profile.getSavedId();
    if (!savedId) { this.router.navigate(['/cadastro']); return; }
    if (!this.profile.perfilAtivo()) {
      this.api.getPerfil(savedId).subscribe({ next: p => { this.profile.setPerfilAtivo(p); this.carregar(); } });
    } else {
      this.carregar();
    }
  }

  carregar() {
    const id = this.profile.id;
    this.api.getPerfil(id).subscribe({ next: p => { this.perfil = p; this.profile.setPerfilAtivo(p); } });
    this.api.getRecompensas(id).subscribe({ next: r => this.recompensas = r.filter(x => x.ativa) });
    this.api.getAtributos().subscribe({ next: a => this.atributos = a });
    this.api.getBonsHabitos(id).subscribe({ next: h => this.bonsHabitos = h });
    this.api.getMissoes(id).subscribe({ next: m => this.missoes = m });
    this.api.getLootboxStatus(id).subscribe({ next: s => {
      this.lootboxDisponivel = s.disponivel;
      this.lootboxXpHoje     = s.xpHoje;
      this.lootboxJaAbriu    = s.jaAbriuHoje;
    }});
  }

  pontosAtributo(atributoId: number): number {
    const bons   = this.bonsHabitos.filter(h => h.atributoId === atributoId);
    const miss   = this.missoes.filter(m => m.atributoId === atributoId && m.concluida);
    return Math.floor((bons.reduce((a, h) => a + h.xp * h.streak, 0) + miss.reduce((a, m) => a + m.recompensaXp, 0)) / 10);
  }

  nomeAtributo(id: number | null | undefined): string {
    if (!id) return '';
    const a = this.atributos.find(x => x.id === id);
    return a ? `${a.emoji} ${a.nome}` : '';
  }

  podeComprar(r: Recompensa): boolean {
    return (this.perfil?.moedas ?? 0) >= r.preco;
  }

  requisitoCumprido(r: Recompensa): boolean {
    if (!r.atributoId || !r.pontosNecessarios) return true;
    return this.pontosAtributo(r.atributoId) >= r.pontosNecessarios;
  }

  resgatar(r: Recompensa) {
    if (!this.podeComprar(r) || !this.requisitoCumprido(r) || this.resgatando) return;
    this.resgatando = r.id;
    this.api.resgatarRecompensa(r.id, this.profile.id).subscribe({
      next: p => {
        this.perfil = p;
        this.profile.setPerfilAtivo(p);
        this.resgatando = null;
        this.feedbacks[r.id] = 'ok';
        setTimeout(() => delete this.feedbacks[r.id], 2500);
      },
      error: e => {
        this.resgatando = null;
        this.feedbacks[r.id] = e.status === 400 && e.error?.includes?.('Atributo') ? 'atributo' : e.status === 400 ? 'sem-moedas' : 'erro';
        setTimeout(() => delete this.feedbacks[r.id], 2500);
      }
    });
  }

  abrirLootbox() {
    if (!this.lootboxDisponivel || this.lootboxAbrindo) return;
    this.lootboxAbrindo  = true;
    this.lootboxResultado = null;
    this.lootboxAnimando  = true;

    // Animação de "abrindo" por 1.8s antes de mostrar o resultado
    setTimeout(() => {
      this.api.abrirLootbox(this.profile.id).subscribe({
        next: resultado => {
          this.lootboxAbrindo   = false;
          this.lootboxResultado = resultado;
          this.lootboxDisponivel = false;
          this.lootboxJaAbriu   = true;
          setTimeout(() => this.lootboxAnimando = false, 200);
        },
        error: () => {
          this.lootboxAbrindo  = false;
          this.lootboxAnimando = false;
        }
      });
    }, 1800);
  }

  fecharResultado() {
    this.lootboxResultado = null;
  }
}
