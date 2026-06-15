import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { Atributo, BomHabito, JornadaSemana, Missao } from '../../models/models';

@Component({
  selector: 'app-missoes',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './missoes.component.html',
  styleUrl: './missoes.component.scss',
})
export class MissoesComponent implements OnInit {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  private router  = inject(Router);

  principais:  Missao[] = [];
  secundarias: Missao[] = [];
  desafios:    Missao[] = [];
  atributos:   Atributo[] = [];
  bonsHabitos: BomHabito[] = [];
  missoes:     Missao[] = [];
  jornada:     JornadaSemana[] = [];

  abertaPrincipais  = false;
  abertaSecundarias = false;
  abertaDesafios    = false;

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
    this.api.getMissoes(id).subscribe(missoes => {
      this.missoes    = missoes;
      this.principais  = missoes.filter(m => m.tipo?.nome === 'Principal');
      this.secundarias = missoes.filter(m => m.tipo?.nome === 'Secundária');
      this.desafios    = missoes.filter(m => m.tipo?.nome === 'Desafio');
    });
    this.api.getAtributos().subscribe(a => this.atributos = a);
    this.api.getBonsHabitos(id).subscribe(h => this.bonsHabitos = h);
    this.api.getJornada(id).subscribe(j => this.jornada = j);
  }

  completar(missao: Missao) {
    if (missao.concluida || this.expirada(missao)) return;
    this.api.completarMissao(missao.id, this.profile.id).subscribe(() => this.carregar());
  }

  expirada(missao: Missao): boolean {
    const hoje = new Date(new Date().toDateString());
    if (missao.dataLimite && new Date(missao.dataLimite) < hoje) return true;
    if (missao.missaoPrincipalId) {
      const principal = this.missoes.find(m => m.id === missao.missaoPrincipalId);
      if (principal && (principal.concluida || this.expirada(principal))) return true;
    }
    return false;
  }

  labelDataLimite(missao: Missao): string {
    if (!missao.dataLimite) return '';
    const d = new Date(missao.dataLimite);
    return d.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  // ── Missão Sugerida (Ideia 4) ────────────────────────
  get atributoMaisFraco(): Atributo | null {
    if (!this.atributos.length) return null;
    let menor = this.atributos[0];
    let menorValor = this.calcularAtributo(menor.id);
    for (const a of this.atributos) {
      const v = this.calcularAtributo(a.id);
      if (v < menorValor) { menorValor = v; menor = a; }
    }
    return menor;
  }

  calcularAtributo(atributoId: number): number {
    const xpBons    = this.bonsHabitos.filter(h => h.atributoId === atributoId)
                        .reduce((acc, h) => acc + h.xp * h.streak, 0);
    const xpMissoes = this.missoes.filter(m => m.atributoId === atributoId && m.concluida)
                        .reduce((acc, m) => acc + m.recompensaXp, 0);
    return Math.floor((xpBons + xpMissoes) / 10);
  }

  get sugestaoMissao(): string {
    const a = this.atributoMaisFraco;
    if (!a) return '';

    const sugestoes: Record<string, string[]> = {
      'Inteligência': [
        'Resolver 10 exercícios de lógica ou matemática',
        'Assistir uma aula ou vídeo educativo completo',
        'Aprender algo novo e anotar 3 pontos principais',
        'Fazer um quiz ou teste de conhecimento geral',
        'Estudar por 1 hora sem distrações',
      ],
      'Sabedoria': [
        'Ler por 30 minutos seguidos',
        'Ouvir um podcast educativo até o fim',
        'Escrever um resumo do que aprendeu hoje',
        'Ler um artigo longo e refletir sobre ele',
        'Terminar um capítulo de um livro',
      ],
      'Físico': [
        'Fazer 30 minutos de exercício físico',
        'Dar uma caminhada de pelo menos 20 minutos',
        'Fazer uma série de alongamentos completa',
        'Completar um treino de força ou cardio',
        'Subir escadas ao invés de usar elevador o dia todo',
      ],
      'Disciplina': [
        'Cumprir todas as tarefas planejadas para hoje',
        'Acordar no horário certo e seguir a rotina',
        'Organizar seu espaço de trabalho ou quarto',
        'Não usar redes sociais por 3 horas seguidas',
        'Ir dormir no horário planejado',
      ],
      'Foco': [
        'Fazer 3 sessões de Pomodoro (25min cada)',
        'Trabalhar ou estudar por 1h sem pegar o celular',
        'Completar uma tarefa difícil sem interrupções',
        'Desligar notificações e focar por 2 horas',
        'Planejar o dia e executar sem desvios',
      ],
      'Vitalidade': [
        'Beber pelo menos 2 litros de água hoje',
        'Dormir 8 horas e registrar como acordou',
        'Fazer uma refeição saudável e equilibrada',
        'Tirar 10 minutos para respirar e relaxar',
        'Evitar açúcar e ultraprocessados por um dia',
      ],
    };

    const lista = sugestoes[a.nome] ?? [`Dedicar 1 hora para trabalhar em ${a.nome}`];
    const hoje = new Date();
    const seed = hoje.getFullYear() * 10000 + (hoje.getMonth() + 1) * 100 + hoje.getDate();
    return lista[seed % lista.length];
  }

  // ── Jornada SVG (Ideia 6) ─────────────────────────────
  readonly SEMANAS = 12;

  get jornadaCompleta(): { semana: string; total: number; xp: number; label: string }[] {
    const resultado = [];
    for (let i = this.SEMANAS - 1; i >= 0; i--) {
      const d = new Date();
      const diff = (d.getDay() + 6) % 7; // dias desde segunda
      d.setDate(d.getDate() - diff - i * 7);
      d.setHours(0, 0, 0, 0);
      const chave = d.toISOString().slice(0, 10);
      const dados = this.jornada.find(j => j.semana === chave);
      resultado.push({
        semana: chave,
        total:  dados?.total ?? 0,
        xp:     dados?.xp ?? 0,
        label:  this.labelSemana(d),
      });
    }
    return resultado;
  }

  private labelSemana(d: Date): string {
    return d.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' });
  }

  get maxJornada(): number {
    return Math.max(1, ...this.jornadaCompleta.map(s => s.total));
  }

  jornadaR(total: number): number {
    const ratio = total / this.maxJornada;
    return 5;
  }

  jornadaCor(total: number): string {
    if (total === 0) return '#30363d';
    if (total <= 2)  return '#1f6feb';
    if (total <= 4)  return '#58a6ff';
    return '#79c0ff';
  }

  jornadaX(i: number): number {
    const W = 320;
    const pad = 20;
    return pad + (i / (this.SEMANAS - 1)) * (W - pad * 2);
  }

  tooltipJornada: { semana: string; total: number; xp: number } | null = null;

  mostrarTooltip(s: { semana: string; total: number; xp: number }) {
    this.tooltipJornada = s;
  }

  esconderTooltip() {
    this.tooltipJornada = null;
  }
}
