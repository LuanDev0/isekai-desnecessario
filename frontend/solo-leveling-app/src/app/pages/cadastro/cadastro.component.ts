import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { Perfil } from '../../models/models';

const API_BASE = 'http://localhost:5008';

@Component({
  selector: 'app-cadastro',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cadastro.component.html',
  styleUrl: './cadastro.component.scss',
})
export class CadastroComponent implements OnInit {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  private router  = inject(Router);

  perfis: Perfil[] = [];
  novoNome = '';
  criando  = false;
  erro     = '';

  ngOnInit() {
    this.carregar();
  }

  carregar() {
    this.api.getPerfis().subscribe({ next: p => this.perfis = p });
  }

  fotoUrl(perfil: Perfil): string | null {
    if (perfil.fotoUrl?.startsWith('/')) return `${API_BASE}${perfil.fotoUrl}`;
    return null;
  }

  selecionar(perfil: Perfil) {
    this.profile.setPerfilAtivo(perfil);
    this.router.navigate(['/']);
  }

  criar() {
    if (!this.novoNome.trim()) return;
    this.api.criarPerfil(this.novoNome.trim()).subscribe({
      next: p => {
        this.profile.setPerfilAtivo(p);
        this.router.navigate(['/']);
      },
      error: () => this.erro = 'Erro ao criar perfil.'
    });
  }
}
