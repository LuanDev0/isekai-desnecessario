import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { IconComponent } from '../../components/icon/icon.component';
import { PerfilRanking, PerfilRankingAtributo, Atributo } from '../../models/models';
import { environment } from '../../../environments/environment';

const API_BASE = environment.apiUrl.replace('/api', '');

type AbaAtiva = 'nivel' | 'atributos';

@Component({
  selector: 'app-ranking',
  standalone: true,
  imports: [CommonModule, TranslatePipe, IconComponent],
  templateUrl: './ranking.component.html',
  styleUrl: './ranking.component.scss',
})
export class RankingComponent implements OnInit {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  private router  = inject(Router);

  meuPerfilId: number | null = null;
  abaAtiva: AbaAtiva = 'nivel';
  subAba: number | null = null; // null = Total, number = atributoId

  // Aba Nível
  perfisNivel: PerfilRanking[] = [];
  carregandoNivel = true;

  // Aba Atributos
  atributos: Atributo[] = [];
  rankingAtributos: Map<string, PerfilRankingAtributo[]> = new Map();
  carregandoAtrib = false;

  ngOnInit() {
    const savedId = this.profile.getSavedId();
    if (!savedId) { this.router.navigate(['/cadastro']); return; }
    this.meuPerfilId = savedId;

    this.api.getRanking().subscribe({
      next: p => { this.perfisNivel = p; this.carregandoNivel = false; },
      error: () => { this.carregandoNivel = false; },
    });
    this.api.getAtributos().subscribe({ next: a => this.atributos = a });
  }

  selecionarAba(aba: AbaAtiva) {
    this.abaAtiva = aba;
    if (aba === 'atributos' && !this.rankingAtributos.has('total')) {
      this.carregarSubAba(null);
    }
  }

  selecionarSubAba(atributoId: number | null) {
    this.subAba = atributoId;
    const key = atributoId == null ? 'total' : String(atributoId);
    if (!this.rankingAtributos.has(key)) {
      this.carregarSubAba(atributoId);
    }
  }

  private carregarSubAba(atributoId: number | null) {
    const key = atributoId == null ? 'total' : String(atributoId);
    this.carregandoAtrib = true;
    this.api.getRankingAtributos(atributoId ?? undefined).subscribe({
      next: lista => {
        this.rankingAtributos.set(key, lista);
        this.carregandoAtrib = false;
      },
      error: () => { this.carregandoAtrib = false; },
    });
  }

  get listaAtribAtiva(): PerfilRankingAtributo[] {
    const key = this.subAba == null ? 'total' : String(this.subAba);
    return this.rankingAtributos.get(key) ?? [];
  }

  get subAbaCarregada(): boolean {
    const key = this.subAba == null ? 'total' : String(this.subAba);
    return this.rankingAtributos.has(key);
  }

  fotoUrl(fotoUrl: string | null): string | null {
    if (!fotoUrl) return null;
    if (fotoUrl.startsWith('data:')) return fotoUrl;
    if (fotoUrl.startsWith('/')) return `${API_BASE}${fotoUrl}`;
    return null;
  }

  rankColor(rank: string): string {
    const map: Record<string, string> = {
      SSS: '#ff1744', SS: '#f44336', S: '#e53935',
      A: '#ff6f00', B: '#bc8cff', C: '#58a6ff',
      D: '#3fb950', E: '#78909c', F: '#546e7a',
      G: '#455a64', H: '#37474f',
    };
    return map[rank] ?? '#546e7a';
  }

  posicaoIcon(i: number): string {
    if (i === 0) return '#1';
    if (i === 1) return '#2';
    if (i === 2) return '#3';
    return `${i + 1}`;
  }

  get minhaPosNivel(): number {
    return this.perfisNivel.findIndex(p => p.id === this.meuPerfilId);
  }

  get minhaPosAtrib(): number {
    return this.listaAtribAtiva.findIndex(p => p.id === this.meuPerfilId);
  }

  atributoNome(id: number | null): string {
    if (id == null) return '';
    return this.atributos.find(a => a.id === id)?.nome ?? '';
  }

  atributoEmoji(id: number | null): string {
    if (id == null) return '';
    return this.atributos.find(a => a.id === id)?.emoji ?? '';
  }

  atributoCor(id: number | null): string {
    if (id == null) return '#ffd700';
    return this.atributos.find(a => a.id === id)?.cor ?? '#8b949e';
  }
}
