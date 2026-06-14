import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { Perfil } from '../../models/models';

export type Periodo = 'diario' | 'semanal' | 'mensal' | 'anual';

export interface DiaStat {
  date:       string;       // 'YYYY-MM-DD'
  xpHoje:     number;
  nivel:      number;
  moedas:     number;
  xpPorHora:  number[];     // índice 0-23, valor = xpHoje acumulado até aquela hora
}

export interface Ponto {
  label: string;
  xp:    number;
}

// ── SVG layout ─────────────────────────────────────────
const VB_W  = 700;
const VB_H  = 150;
const PAD_L = 44;
const PAD_R = 10;
const PAD_T = 12;
const PAD_B = 24;
const CW    = VB_W - PAD_L - PAD_R;
const CH    = VB_H - PAD_T - PAD_B;

@Component({
  selector: 'app-grafico',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './grafico.component.html',
  styleUrl: './grafico.component.scss',
})
export class GraficoComponent implements OnInit, OnDestroy {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  private router  = inject(Router);

  perfil:   Perfil | null = null;
  history:  DiaStat[]    = [];
  periodo:  Periodo      = 'diario';
  private pollInterval: any;

  readonly periodos: { key: Periodo; label: string }[] = [
    { key: 'diario',  label: 'Diário'  },
    { key: 'semanal', label: 'Semanal' },
    { key: 'mensal',  label: 'Mensal'  },
    { key: 'anual',   label: 'Anual'   },
  ];

  readonly vbW = VB_W; readonly vbH = VB_H;
  readonly padL = PAD_L; readonly padT = PAD_T;
  readonly padB = PAD_B; readonly cw = CW; readonly ch = CH;

  ngOnInit() {
    const savedId = this.profile.getSavedId();
    if (!savedId) { this.router.navigate(['/cadastro']); return; }

    this.atualizar();

    // Atualiza a cada 15 segundos enquanto a página estiver aberta
    this.pollInterval = setInterval(() => this.atualizar(), 15_000);
  }

  ngOnDestroy() {
    clearInterval(this.pollInterval);
  }

  private atualizar() {
    const savedId = this.profile.getSavedId();
    if (!savedId) return;
    this.api.getPerfil(savedId).subscribe({ next: p => {
      this.perfil = p;
      this.profile.setPerfilAtivo(p);
      this.salvarECarregar(p);
    }});
  }

  private salvarECarregar(p: Perfil) {
    const hora = new Date().getHours();
    // Primeiro salva, depois busca (encadeado)
    this.api.upsertHistorico(p.id, p.xpHoje ?? 0, p.nivel, p.moedas, hora)
      .subscribe({
        next: () => this.carregarHistorico(p.id),
        error: ()  => this.carregarHistorico(p.id), // carrega mesmo se upsert falhar
      });
  }

  private carregarHistorico(perfilId: number) {
    this.api.getHistorico(perfilId).subscribe({
      next: hist => {
        this.history = hist.map(h => ({
          date:      h.date,
          xpHoje:    h.xpHoje,
          nivel:     h.nivel,
          moedas:    h.moedas,
          xpPorHora: h.xpPorHora ?? new Array(24).fill(0),
        }));
      }
    });
  }

  // ── Pontos por período ────────────────────────────────

  get pontos(): Ponto[] {
    switch (this.periodo) {
      case 'diario':  return this.pontosHorasDia();
      case 'semanal': return this.pontosDiasSemana();
      case 'mensal':  return this.pontosDiasMes();
      case 'anual':   return this.pontosMesesAno();
    }
  }

  /** 24 horas (0h–23h) do dia de hoje — mostra XP ganho em cada hora (delta) */
  private pontosHorasDia(): Ponto[] {
    const hoje  = new Date().toISOString().slice(0, 10);
    const dia   = this.history.find(d => d.date === hoje);
    const raw: number[] = dia?.xpPorHora ?? new Array(24).fill(0);

    // Converte snapshots cumulativos em delta (xp ganho naquela hora)
    // raw[h] = xpHoje no momento do snapshot; delta = diferença entre snapshots
    const delta = new Array(24).fill(0);
    let anterior = 0;
    for (let h = 0; h < 24; h++) {
      if (raw[h] > 0) {
        delta[h] = Math.max(0, raw[h] - anterior);
        anterior = raw[h];
      }
    }

    return Array.from({ length: 24 }, (_, h) => ({
      label: `${h}h`,
      xp:    delta[h],
    }));
  }

  /** Dias da semana atual: Dom, Seg, Ter, Qua, Qui, Sex, Sáb */
  private pontosDiasSemana(): Ponto[] {
    const hoje    = new Date();
    const domAtual = new Date(hoje);
    domAtual.setDate(hoje.getDate() - hoje.getDay()); // domingo da semana

    const nomes = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];

    return Array.from({ length: 7 }, (_, i) => {
      const d    = new Date(domAtual);
      d.setDate(domAtual.getDate() + i);
      const key  = d.toISOString().slice(0, 10);
      const stat = this.history.find(h => h.date === key);
      return { label: nomes[i], xp: stat?.xpHoje ?? 0 };
    });
  }

  /** Dias 1..N do mês atual */
  private pontosDiasMes(): Ponto[] {
    const agora  = new Date();
    const ano    = agora.getFullYear();
    const mes    = agora.getMonth(); // 0-based
    const total  = new Date(ano, mes + 1, 0).getDate(); // dias no mês

    return Array.from({ length: total }, (_, i) => {
      const dia  = i + 1;
      const key  = `${ano}-${String(mes + 1).padStart(2, '0')}-${String(dia).padStart(2, '0')}`;
      const stat = this.history.find(h => h.date === key);
      return { label: String(dia), xp: stat?.xpHoje ?? 0 };
    });
  }

  /** Meses Jan–Dez do ano atual */
  private pontosMesesAno(): Ponto[] {
    const ano    = new Date().getFullYear();
    const nomes  = ['Jan','Fev','Mar','Abr','Mai','Jun','Jul','Ago','Set','Out','Nov','Dez'];

    return Array.from({ length: 12 }, (_, m) => {
      const prefixo = `${ano}-${String(m + 1).padStart(2, '0')}`;
      const xp = this.history
        .filter(h => h.date.startsWith(prefixo))
        .reduce((s, h) => s + (h.xpHoje ?? 0), 0);
      return { label: nomes[m], xp };
    });
  }

  // ── Cálculo SVG ───────────────────────────────────────

  get chartData() {
    const pts = this.pontos;
    const n   = pts.length;
    if (n === 0) return null;

    const maxXp = Math.max(...pts.map(p => p.xp), 1);
    const ticks = this.niceYTicks(maxXp, 4);
    const yMax  = ticks[ticks.length - 1];

    const coords = pts.map((p, i) => {
      const x = PAD_L + (n === 1 ? CW / 2 : (i / (n - 1)) * CW);
      const y = PAD_T + CH - (p.xp / yMax) * CH;
      return { x, y, xp: p.xp, label: p.label };
    });

    const polyline = coords.map(c => `${c.x},${c.y}`).join(' ');
    const first    = coords[0];
    const last     = coords[coords.length - 1];
    const area     = `M${first.x},${PAD_T + CH} ` +
                     coords.map(c => `L${c.x},${c.y}`).join(' ') +
                     ` L${last.x},${PAD_T + CH} Z`;

    const yTickLines = ticks.map(t => ({
      y:     PAD_T + CH - (t / yMax) * CH,
      label: this.fmtXp(t),
    }));

    // Labels X: espaçadas para não sobrepor (máx ~8 visíveis)
    const maxLbls = 8;
    const step    = n <= maxLbls ? 1 : Math.ceil(n / maxLbls);
    const xLabels = coords.filter((_, i) => i % step === 0 || i === n - 1);

    const showDots = false;

    return { coords, polyline, area, yTickLines, xLabels, showDots };
  }

  // ── Helpers ───────────────────────────────────────────

  private niceYTicks(max: number, count: number): number[] {
    if (max === 0) return [0, 100, 200, 300, 400];
    const raw  = max / count;
    const mag  = Math.pow(10, Math.floor(Math.log10(raw)));
    const nice = [1, 2, 2.5, 5, 10].map(f => f * mag).find(f => f >= raw) ?? mag * 10;
    const ticks: number[] = [];
    for (let i = 1; i <= count + 2; i++) {
      ticks.push(nice * i);
      if (nice * i >= max * 1.05) break;
    }
    return ticks;
  }

  private fmtXp(n: number): string {
    if (n >= 1000) return `${+(n / 1000).toFixed(1)}k`;
    return String(n);
  }

  // ── Stats ─────────────────────────────────────────────

  get xpPercent(): number {
    if (!this.perfil) return 0;
    return Math.min(100, Math.round((this.perfil.xp / this.perfil.proximoNivelXp) * 100));
  }

  get rankColor(): string {
    const r   = this.perfil?.rank ?? 'F';
    const map: Record<string, string> = {
      SSS: '#ff1744', SS: '#f44336', S: '#e53935',
      A:   '#ff6f00', B: '#bc8cff', C: '#58a6ff',
      D:   '#3fb950', E: '#78909c', F: '#546e7a',
      G:   '#455a64', H: '#37474f',
    };
    return map[r] ?? '#546e7a';
  }

  get totalXpPeriodo(): number {
    // Diário: xpHoje é cumulativo — não somar as 24 horas (seria múltiplo)
    if (this.periodo === 'diario') return this.perfil?.xpHoje ?? 0;
    return this.pontos.reduce((s, p) => s + p.xp, 0);
  }

  get mediaXp(): number {
    if (this.periodo === 'diario') {
      // Média por hora com dado registrado
      const hoje = new Date().toISOString().slice(0, 10);
      const dia  = this.history.find(d => d.date === hoje);
      const horas = (dia?.xpPorHora ?? []).filter(v => v > 0);
      if (horas.length === 0) return 0;
      return Math.round((this.perfil?.xpHoje ?? 0) / horas.length);
    }
    const pts = this.pontos.filter(p => p.xp > 0);
    if (pts.length === 0) return 0;
    return Math.round(pts.reduce((s, p) => s + p.xp, 0) / pts.length);
  }

  get melhorPonto(): Ponto | null {
    if (this.periodo === 'diario') {
      // Melhor hora = hora com maior XP ganho (delta)
      const pts   = this.pontos; // já calculado como delta
      const m     = pts.reduce((a, b) => b.xp > a.xp ? b : a, { label: '', xp: 0 });
      return m.xp > 0 ? m : null;
    }
    const pts = this.pontos;
    if (pts.length === 0) return null;
    const m = pts.reduce((a, b) => b.xp > a.xp ? b : a);
    return m.xp > 0 ? m : null;
  }

  get labelPeriodo(): string {
    const map: Record<Periodo, string> = { diario: 'hora', semanal: 'dia', mensal: 'dia', anual: 'mês' };
    return map[this.periodo];
  }
}
