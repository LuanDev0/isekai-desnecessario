import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { IconComponent } from '../../components/icon/icon.component';
import { GrupoConvitePendente, GrupoResumo, PlanoGrupo } from '../../models/models';

// Planos disponíveis (tamanho de cada um vem do backend — espelhado aqui para a UI).
const PLANOS: { plano: PlanoGrupo; vagas: number }[] = [
  { plano: 'Starter',  vagas: 5 },
  { plano: 'Standard', vagas: 10 },
  { plano: 'Pro',      vagas: 30 },
  { plano: 'Max',      vagas: 50 },
];

@Component({
  selector: 'app-grupos',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe, IconComponent],
  templateUrl: './grupos.component.html',
  styleUrl: './grupos.component.scss',
})
export class GruposComponent implements OnInit {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  private router  = inject(Router);

  readonly planos = PLANOS;

  perfilId = 0;
  carregando = true;

  grupos: GrupoResumo[] = [];
  convites: GrupoConvitePendente[] = [];

  criando = false;
  novoNome = '';
  novoPlano: PlanoGrupo = 'Starter';
  erro = '';

  ngOnInit() {
    const savedId = this.profile.getSavedId();
    if (!savedId) { this.router.navigate(['/cadastro']); return; }
    this.perfilId = savedId;
    this.carregar();
  }

  carregar() {
    this.carregando = true;
    this.api.getGrupos(this.perfilId).subscribe({
      next: g => { this.grupos = g; this.carregando = false; },
      error: () => { this.carregando = false; },
    });
    this.api.getConvitesGrupo().subscribe({ next: c => this.convites = c });
  }

  criar() {
    const nome = this.novoNome.trim();
    if (!nome) return;
    this.erro = '';
    this.api.criarGrupo(nome, this.novoPlano, this.perfilId).subscribe({
      next: g => {
        this.criando = false;
        this.novoNome = '';
        this.novoPlano = 'Starter';
        this.router.navigate(['/grupos', g.id]);
      },
      error: e => this.erro = typeof e?.error === 'string' ? e.error : 'Erro ao criar grupo.',
    });
  }

  abrir(g: GrupoResumo) {
    this.router.navigate(['/grupos', g.id]);
  }

  aceitar(c: GrupoConvitePendente) {
    this.api.aceitarConviteGrupo(c.id, this.perfilId).subscribe({
      next: () => this.carregar(),
      error: e => { alert(typeof e?.error === 'string' ? e.error : 'Erro ao aceitar convite.'); this.carregar(); },
    });
  }

  recusar(c: GrupoConvitePendente) {
    this.api.recusarConviteGrupo(c.id).subscribe({ next: () => this.carregar() });
  }
}
