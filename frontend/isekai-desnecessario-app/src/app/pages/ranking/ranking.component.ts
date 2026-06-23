import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { PerfilRanking } from '../../models/models';
import { environment } from '../../../environments/environment';

const API_BASE = environment.apiUrl.replace('/api', '');

@Component({
  selector: 'app-ranking',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './ranking.component.html',
  styleUrl: './ranking.component.scss',
})
export class RankingComponent implements OnInit {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  private router  = inject(Router);

  perfis:    PerfilRanking[] = [];
  carregando = true;
  meuPerfilId: number | null = null;

  ngOnInit() {
    const savedId = this.profile.getSavedId();
    if (!savedId) { this.router.navigate(['/cadastro']); return; }
    this.meuPerfilId = savedId;
    this.api.getRanking().subscribe({
      next: p => { this.perfis = p; this.carregando = false; },
      error: () => { this.carregando = false; },
    });
  }

  fotoUrl(perfil: PerfilRanking): string | null {
    const url = perfil.fotoUrl;
    if (!url) return null;
    if (url.startsWith('data:')) return url;
    if (url.startsWith('/')) return `${API_BASE}${url}`;
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
    if (i === 0) return '🥇';
    if (i === 1) return '🥈';
    if (i === 2) return '🥉';
    return `${i + 1}`;
  }

  get minhaPos(): number {
    return this.perfis.findIndex(p => p.id === this.meuPerfilId);
  }
}
