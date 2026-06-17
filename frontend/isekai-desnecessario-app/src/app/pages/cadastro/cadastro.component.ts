import { Component, OnInit, AfterViewInit, ViewChild, ElementRef, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { AuthService } from '../../services/auth.service';
import { LanguageService } from '../../services/language.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { Classe, Perfil, Usuario } from '../../models/models';

const API_BASE = environment.apiUrl.replace('/api', '');

@Component({
  selector: 'app-cadastro',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './cadastro.component.html',
  styleUrl: './cadastro.component.scss',
})
export class CadastroComponent implements OnInit, AfterViewInit {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  private auth    = inject(AuthService);
  private router  = inject(Router);
  readonly lang   = inject(LanguageService);

  @ViewChild('googleBtn') googleBtnRef?: ElementRef<HTMLDivElement>;

  usuario: Usuario | null = null;
  perfis:  Perfil[] = [];
  classes: Classe[] = [];

  novoNome   = '';
  erro       = '';
  salvando   = false;
  carregando = false;

  tela: 'inicio' | 'perfis' | 'cadastro' | 'convidado' = 'inicio';

  fotoFile:          File | null = null;
  fotoPreview:       string | null = null;
  classeSelecionada: number | null = null;
  generoSelecionado: string | null = null;

  readonly generos = ['Masculino', 'Feminino', 'Não-binário'];

  nomeClasse(c: Classe): string {
    if (this.generoSelecionado === 'Feminino') return c.nomeFeminino ?? c.nome;
    return c.nome;
  }

  ngOnInit() {
    this.api.getClasses().subscribe({ next: c => this.classes = c });

    // Se já está logado com Google, pula direto para os perfis
    this.usuario = this.auth.usuario();
    if (this.usuario) {
      this.carregarPerfisDoUsuario();
    }
  }

  ngAfterViewInit() {
    if (!environment.googleClientId) return;
    this.tentarIniciarGoogle();
  }

  private tentarIniciarGoogle() {
    if ((window as any)['google']?.accounts?.id) {
      this.auth.initGoogleSignIn(idToken => this.onGoogleToken(idToken));
      if (this.googleBtnRef) {
        this.auth.renderGoogleButton(this.googleBtnRef.nativeElement);
      }
    } else {
      setTimeout(() => this.tentarIniciarGoogle(), 150);
    }
  }

  onGoogleToken(idToken: string) {
    this.carregando = true;
    this.erro = '';
    this.auth.loginComGoogle(idToken).subscribe({
      next: res => {
        this.usuario = res.usuario;
        this.perfis  = res.perfis;
        this.carregando = false;
        if (this.perfis.length === 0) {
          this.tela = 'cadastro';
        } else {
          this.tela = 'perfis';
        }
      },
      error: () => {
        this.erro = 'Erro ao autenticar com Google. Tente novamente.';
        this.carregando = false;
      }
    });
  }

  carregarPerfisDoUsuario() {
    this.carregando = true;
    this.api.getMeusPerfis().subscribe({
      next: p => {
        this.perfis = p;
        this.carregando = false;
        this.tela = 'perfis';
      },
      error: () => {
        this.carregando = false;
        this.tela = 'perfis';
      }
    });
  }

  // ── Modo convidado (sem Google) ───────────────────────
  irModoConvidado() {
    this.api.getPerfis().subscribe({ next: p => this.perfis = p });
    this.tela = 'convidado';
  }

  // ── Criar novo perfil ─────────────────────────────────
  irCadastro() {
    this.novoNome = '';
    this.erro = '';
    this.classeSelecionada = null;
    this.generoSelecionado = null;
    this.removerFoto();
    this.tela = 'cadastro';
  }

  voltar() {
    if (this.usuario) {
      this.tela = this.perfis.length > 0 ? 'perfis' : 'inicio';
    } else {
      this.tela = 'inicio';
    }
  }

  logout() {
    this.auth.logout();
    this.profile.clearPerfil();
    this.usuario = null;
    this.perfis = [];
    this.tela = 'inicio';
    // Re-renderiza o botão Google
    setTimeout(() => this.tentarIniciarGoogle(), 200);
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
            error: () => this.entrar(p),
          });
        } else {
          this.entrar(p);
        }
      },
      error: () => { this.erro = 'Erro ao criar perfil.'; this.salvando = false; }
    });
  }

  private entrar(perfil: Perfil) {
    this.profile.setPerfilAtivo(perfil);
    this.router.navigate(['/']);
  }

  // ── Foto ──────────────────────────────────────────────
  onFotoSelecionada(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const okTipos = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
    if (!okTipos.includes(file.type)) { this.erro = 'Use uma imagem JPG, PNG, GIF ou WebP.'; return; }
    if (file.size > 5 * 1024 * 1024) { this.erro = 'Imagem muito grande. Máximo 5MB.'; return; }
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

  fotoUrl(perfil: Perfil): string | null {
    const url = perfil.fotoUrl;
    if (!url) return null;
    if (url.startsWith('data:')) return url;
    if (url.startsWith('/')) return `${API_BASE}${url}`;
    return null;
  }

  get podeCriarMais(): boolean {
    return this.perfis.length < 3;
  }

  get clientIdConfigurado(): boolean {
    return !!environment.googleClientId;
  }
}
