import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { Perfil } from '../../models/models';

interface RankInfo {
  rank: string;
  label: string;
  minNivel: number;
  maxNivel: number;
  cor: string;
  icon: string;
}

@Component({
  selector: 'app-mundo',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mundo.component.html',
  styleUrl: './mundo.component.scss',
})
export class MundoComponent implements OnInit {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  private router  = inject(Router);

  perfil: Perfil | null = null;

  readonly ranks: RankInfo[] = [
    { rank: 'H',   label: 'H',   minNivel:  1,  maxNivel:  9,   cor: '#8b949e', icon: '🪨' },
    { rank: 'G',   label: 'G',   minNivel: 10,  maxNivel: 19,   cor: '#6e7681', icon: '🗡️' },
    { rank: 'F',   label: 'F',   minNivel: 20,  maxNivel: 29,   cor: '#3fb950', icon: '🌿' },
    { rank: 'E',   label: 'E',   minNivel: 30,  maxNivel: 39,   cor: '#58a6ff', icon: '💧' },
    { rank: 'D',   label: 'D',   minNivel: 40,  maxNivel: 49,   cor: '#79c0ff', icon: '❄️' },
    { rank: 'C',   label: 'C',   minNivel: 50,  maxNivel: 59,   cor: '#bc8cff', icon: '🔮' },
    { rank: 'B',   label: 'B',   minNivel: 60,  maxNivel: 69,   cor: '#d29922', icon: '⚡' },
    { rank: 'A',   label: 'A',   minNivel: 70,  maxNivel: 79,   cor: '#e3b341', icon: '🔥' },
    { rank: 'S',   label: 'S',   minNivel: 80,  maxNivel: 89,   cor: '#ff7b72', icon: '💎' },
    { rank: 'SS',  label: 'SS',  minNivel: 90,  maxNivel: 99,   cor: '#ffa657', icon: '👁️' },
    { rank: 'SSS', label: 'SSS', minNivel: 100, maxNivel: 9999, cor: '#f0c000', icon: '👑' },
  ];

  ngOnInit() {
    const savedId = this.profile.getSavedId();
    if (!savedId) { this.router.navigate(['/cadastro']); return; }
    if (this.profile.perfilAtivo()) {
      this.perfil = this.profile.perfilAtivo()!;
    } else {
      this.api.getPerfil(savedId).subscribe(p => {
        this.profile.setPerfilAtivo(p);
        this.perfil = p;
      });
    }
  }

  get nivel(): number { return this.perfil?.nivel ?? 1; }
  get xp(): number    { return this.perfil?.xp ?? 0; }

  get rankAtual(): RankInfo {
    return this.ranks.find(r => this.nivel >= r.minNivel && this.nivel <= r.maxNivel) ?? this.ranks[0];
  }

  get indiceAtual(): number {
    return this.ranks.indexOf(this.rankAtual);
  }

  status(r: RankInfo): 'passado' | 'atual' | 'futuro' {
    const idx = this.ranks.indexOf(r);
    if (idx < this.indiceAtual)  return 'passado';
    if (idx === this.indiceAtual) return 'atual';
    return 'futuro';
  }

  // Progresso dentro do rank atual (0-100%)
  get progressoRank(): number {
    const r = this.rankAtual;
    if (r.rank === 'SSS') return 100;
    const total = r.maxNivel - r.minNivel + 1;
    const feito = this.nivel - r.minNivel;
    return Math.round((feito / total) * 100);
  }

  // Próximo rank
  get proximoRank(): RankInfo | null {
    const idx = this.indiceAtual;
    return idx < this.ranks.length - 1 ? this.ranks[idx + 1] : null;
  }

  // Levels para chegar no próximo rank
  get niveisParaProximo(): number {
    const r = this.rankAtual;
    if (!this.proximoRank) return 0;
    return r.maxNivel - this.nivel + 1;
  }
}
