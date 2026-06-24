const API_BASE = environment.apiUrl.replace('/api', '');

import { Component, OnInit, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { LanguageService } from '../../services/language.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { Atributo, Perfil, BomHabito, MauHabito, Missao, ItemInventario } from '../../models/models';

export interface AtributoDisplay {
  id:      number;
  nome:    string;
  emoji:   string;
  valor:   number;
  cor:     string;
  desc:    string;
  habitos: number;
}

@Component({
  selector: 'app-status',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './status.component.html',
  styleUrl: './status.component.scss',
})
export class StatusComponent implements OnInit {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  readonly lang   = inject(LanguageService);
  private router  = inject(Router);

  perfil:      Perfil | null  = null;
  bonsHabitos: BomHabito[]    = [];
  mausHabitos: MauHabito[]    = [];
  missoes:     Missao[]       = [];
  inventario:  ItemInventario[] = [];
  atributos:   Atributo[]     = [];

  // Snapshot anterior para o "fantasma" do radar
  snapshotAnterior: { atributoId: number; pontos: number }[] = [];
  pontosAtributos:  { atributoId: number; total: number }[]  = [];

  carregando = true;
  private loaded = 0;
  private total  = 8;

  ngOnInit() {
    const id = this.profile.getSavedId();
    if (!id) { this.router.navigate(['/cadastro']); return; }
    this.carregar();
  }

  carregar() {
    this.carregando = true;
    this.loaded = 0;
    const id = this.profile.id;

    this.api.getPerfil(id).subscribe(p => { this.perfil = p; this.tick(); });
    this.api.getBonsHabitos(id).subscribe(h => { this.bonsHabitos = h; this.tick(); });
    this.api.getMausHabitos(id).subscribe(h => { this.mausHabitos = h; this.tick(); });
    this.api.getMissoes(id).subscribe(m => { this.missoes = m; this.tick(); });
    this.api.getInventario(id).subscribe(i => { this.inventario = i; this.tick(); });
    this.api.getAtributos().subscribe(a => { this.atributos = a; this.tick(); });
    this.api.getSnapshotAnterior(id).subscribe(s => {
      this.snapshotAnterior = s;
      this.tick();
    });
    this.api.getPontosAtributos(id).subscribe(p => {
      this.pontosAtributos = p;
      this.tick();
    });
  }

  private tick() {
    this.loaded++;
    if (this.loaded >= this.total) {
      this.carregando = false;
      // Salva snapshot após carregar (idempotente — só salva uma vez por dia)
      const dados = this.atributosDisplay.map(a => ({ atributoId: a.id, pontos: a.valor }));
      if (dados.length > 0) {
        this.api.salvarSnapshot(this.profile.id, dados).subscribe();
      }
    }
  }

  // ── Progresso XP ─────────────────────────────────────
  get xpPercent(): number {
    if (!this.perfil || this.perfil.proximoNivelXp <= 0) return 0;
    return Math.min(100, Math.max(0, Math.round((this.perfil.xp / this.perfil.proximoNivelXp) * 100)));
  }

  get xpParaProximo(): number {
    if (!this.perfil) return 0;
    return Math.max(0, this.perfil.proximoNivelXp - this.perfil.xp);
  }

  // ── Atributos RPG ─────────────────────────────────────
  get atributosDisplay(): AtributoDisplay[] {
    return this.atributos.map(attr => {
      const bonsVinculados    = this.bonsHabitos.filter(h => h.atributoId === attr.id);
      const mausVinculados    = this.mausHabitos.filter(h => h.atributoId === attr.id);
      const missoesVinculadas = this.missoes.filter(m => m.atributoId === attr.id);

      const valor = this.pontosAtributos.find(p => p.atributoId === attr.id)?.total ?? 0;

      return {
        id:      attr.id,
        nome:    attr.nome,
        emoji:   attr.emoji,
        valor,
        cor:     attr.cor,
        desc:    attr.descricao,
        habitos: bonsVinculados.length + mausVinculados.length + missoesVinculadas.length,
      };
    });
  }

  // ── Maior / menor atributo (para coroa e caveira) ────
  get atributoMaior(): AtributoDisplay | null {
    const lista = this.atributosDisplay;
    if (!lista.length) return null;
    return lista.reduce((a, b) => b.valor > a.valor ? b : a);
  }

  get atributoMenor(): AtributoDisplay | null {
    const lista = this.atributosDisplay;
    if (!lista.length) return null;
    return lista.reduce((a, b) => b.valor < a.valor ? b : a);
  }

  // ── Max dinâmico dos atributos ────────────────────────
  get maxAtributo(): number {
    const valores = this.atributosDisplay.map(a => a.valor);
    const maior   = Math.max(0, ...valores);
    return maior > 100 ? maior : 100;
  }

  // ── Mini cards laterais: 3 esquerda, 3 direita ───────
  get cardsEsquerda(): AtributoDisplay[] {
    return this.atributosDisplay.slice(0, 3);
  }

  get cardsDireita(): AtributoDisplay[] {
    return this.atributosDisplay.slice(3, 6);
  }

  badgeCard(a: AtributoDisplay): string {
    if (a.id === this.atributoMaior?.id && a.valor > 0) return '👑';
    if (a.id === this.atributoMenor?.id) return '💀';
    return '';
  }

  // Tamanho do valor escalado: 0.8rem (zero) → 1.5rem (máximo)
  mcValorSize(valor: number): string {
    const ratio = this.maxAtributo > 0 ? valor / this.maxAtributo : 0;
    const size  = 0.8 + ratio * 0.7;
    return `${size.toFixed(2)}rem`;
  }

  // Opacidade do card: mais fraco = mais apagado
  mcOpacity(valor: number): string {
    const ratio = this.maxAtributo > 0 ? valor / this.maxAtributo : 0;
    const op = 0.5 + ratio * 0.5;
    return op.toFixed(2);
  }

  // ── Toggle de visualização ────────────────────────────
  vistaRadar = false;

  // ── Radar SVG ────────────────────────────────────────
  readonly CX = 150;
  readonly CY = 155;
  readonly R  = 110;

  radarPonto(index: number, valor: number): { x: number; y: number } {
    const angle = (index * 60 - 90) * Math.PI / 180;
    const ratio = this.maxAtributo > 0 ? Math.min(valor / this.maxAtributo, 1) : 0;
    return {
      x: this.CX + ratio * this.R * Math.cos(angle),
      y: this.CY + ratio * this.R * Math.sin(angle),
    };
  }

  get radarPoligono(): string {
    return this.atributosDisplay
      .map((a, i) => { const p = this.radarPonto(i, a.valor); return `${p.x},${p.y}`; })
      .join(' ');
  }

  // Polígono fantasma (snapshot anterior)
  get radarFantasma(): string {
    if (!this.snapshotAnterior.length) return '';
    return this.atributosDisplay.map((a, i) => {
      const snap = this.snapshotAnterior.find(s => s.atributoId === a.id);
      const pontos = snap ? snap.pontos : 0;
      const p = this.radarPonto(i, pontos);
      return `${p.x},${p.y}`;
    }).join(' ');
  }

  get temFantasma(): boolean {
    return this.snapshotAnterior.length > 0;
  }

  hexGrid(ratio: number): string {
    return Array.from({ length: 6 }, (_, i) => {
      const angle = (i * 60 - 90) * Math.PI / 180;
      const x = this.CX + ratio * this.R * Math.cos(angle);
      const y = this.CY + ratio * this.R * Math.sin(angle);
      return `${x},${y}`;
    }).join(' ');
  }

  eixoFim(index: number): { x: number; y: number } {
    const angle = (index * 60 - 90) * Math.PI / 180;
    return { x: this.CX + this.R * Math.cos(angle), y: this.CY + this.R * Math.sin(angle) };
  }

  labelPos(index: number): { x: number; y: number } {
    const angle = (index * 60 - 90) * Math.PI / 180;
    const dist  = this.R + 24;
    return { x: this.CX + dist * Math.cos(angle), y: this.CY + dist * Math.sin(angle) };
  }

  // ── Rank cor ──────────────────────────────────────────
  get fotoUrl(): string | null {
    const url = this.perfil?.fotoUrl;
    if (!url) return null;
    if (url.startsWith('data:')) return url;
    if (url.startsWith('/')) return `${API_BASE}${url}`;
    return null;
  }

  get rankCor(): string {
    const cores: Record<string, string> = {
      'SSS': '#ff1744', 'SS': '#f44336', 'S': '#e53935',
      'A': '#ff6f00', 'B': '#bc8cff', 'C': '#58a6ff',
      'D': '#3fb950', 'E': '#78909c', 'F': '#546e7a',
      'G': '#455a64', 'H': '#37474f',
    };
    return cores[this.perfil?.rank ?? 'E'] ?? '#78909c';
  }

  get rankLabel(): string {
    return `Rank ${this.perfil?.rank ?? 'E'}`;
  }

  get totalPontosAtributos(): number {
    return this.atributosDisplay.reduce((acc, a) => acc + a.valor, 0);
  }

  get totalHabitosVinculados(): number {
    return this.atributosDisplay.reduce((acc, a) => acc + a.habitos, 0);
  }

  // ── Resumo ────────────────────────────────────────────
  get melhorStreakBom(): number {
    return this.bonsHabitos.reduce((a, h) => Math.max(a, h.streak), 0);
  }

  get missoesConcluidas(): number {
    return this.missoes.filter(m => m.concluida).length;
  }
}
