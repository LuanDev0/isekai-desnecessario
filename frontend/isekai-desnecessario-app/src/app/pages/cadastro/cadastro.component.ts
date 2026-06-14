import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { Classe, Perfil } from '../../models/models';

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

  perfis:  Perfil[] = [];
  classes: Classe[] = [];
  novoNome   = '';
  erro       = '';
  salvando   = false;
  tela: 'inicio' | 'entrar' | 'cadastro' = 'inicio';

  fotoFile:    File | null = null;
  fotoPreview: string | null = null;
  classeSelecionada: number | null = null;
  generoSelecionado: string | null = null;

  readonly generos = ['Masculino', 'Feminino', 'Não-binário'];

  nomeClasse(c: Classe): string {
    if (this.generoSelecionado === 'Feminino') return c.nomeFeminino ?? c.nome;
    return c.nome;
  }

  ngOnInit() {
    this.carregar();
    this.api.getClasses().subscribe({ next: c => this.classes = c });
  }

  carregar() {
    this.api.getPerfis().subscribe({ next: p => this.perfis = p });
  }

  irEntrar() {
    this.carregar();
    this.tela = 'entrar';
  }

  irCadastro() {
    this.novoNome = '';
    this.erro = '';
    this.classeSelecionada = null;
    this.generoSelecionado = null;
    this.removerFoto();
    this.tela = 'cadastro';
  }

  voltar() {
    this.tela = 'inicio';
  }

  onFotoSelecionada(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const okTipos = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
    if (!okTipos.includes(file.type)) {
      this.erro = 'Use uma imagem JPG, PNG, GIF ou WebP.';
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.erro = 'Imagem muito grande. Máximo 5MB.';
      return;
    }

    this.erro = '';
    if (this.fotoPreview) URL.revokeObjectURL(this.fotoPreview);
    this.fotoFile = file;
    this.fotoPreview = URL.createObjectURL(file);
  }

  removerFoto() {
    if (this.fotoPreview) URL.revokeObjectURL(this.fotoPreview);
    this.fotoFile = null;
    this.fotoPreview = null;
  }

  private entrar(perfil: Perfil) {
    this.profile.setPerfilAtivo(perfil);
    this.router.navigate(['/']);
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
    if (!this.novoNome.trim()) { this.erro = 'Digite seu nome de herói.'; return; }
    if (!this.classeSelecionada) { this.erro = 'Escolha uma classe para continuar.'; return; }
    if (this.salvando) return;
    this.erro = '';
    this.salvando = true;
    this.api.criarPerfil(this.novoNome.trim(), this.classeSelecionada, this.generoSelecionado).subscribe({
      next: p => {
        if (this.fotoFile) {
          this.api.uploadFoto(p.id, this.fotoFile).subscribe({
            next: atualizado => this.entrar(atualizado),
            error: () => this.entrar(p), // se a foto falhar, entra mesmo assim
          });
        } else {
          this.entrar(p);
        }
      },
      error: () => { this.erro = 'Erro ao criar perfil.'; this.salvando = false; }
    });
  }
}
