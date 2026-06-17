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

  // ── Estado de tela ────────────────────────────────
  tela: 'inicio' | 'login' | 'registro' | 'perfis' | 'cadastro' = 'inicio';

  // ── Formulários ───────────────────────────────────
  loginEmail  = '';
  loginSenha  = '';
  regNome     = '';
  regEmail    = '';
  regSenha    = '';
  regConfirma = '';
  mostrarSenha        = false;
  mostrarSenhaConfirma = false;

  // ── Novo perfil ───────────────────────────────────
  novoNome           = '';
  classeSelecionada: number | null = null;
  generoSelecionado: string | null = null;
  fotoFile:    File | null = null;
  fotoPreview: string | null = null;

  erro      = '';
  salvando  = false;
  carregando = false;

  readonly generos = ['Masculino', 'Feminino', 'Não-binário'];

  nomeClasse(c: Classe): string {
    return this.generoSelecionado === 'Feminino' ? (c.nomeFeminino ?? c.nome) : c.nome;
  }

  ngOnInit() {
    this.api.getClasses().subscribe({ next: c => this.classes = c });
    this.usuario = this.auth.usuario();
    if (this.usuario) this.carregarPerfisDoUsuario();
  }

  ngAfterViewInit() {
    if (environment.googleClientId) this.tentarIniciarGoogle();
  }

  private tentarIniciarGoogle() {
    if ((window as any)['google']?.accounts?.id) {
      this.auth.initGoogleSignIn(idToken => this.onGoogleToken(idToken));
      if (this.googleBtnRef) this.auth.renderGoogleButton(this.googleBtnRef.nativeElement);
    } else {
      setTimeout(() => this.tentarIniciarGoogle(), 150);
    }
  }

  // ── Autenticação Google ───────────────────────────
  onGoogleToken(idToken: string) {
    this.carregando = true;
    this.erro = '';
    this.auth.loginComGoogle(idToken).subscribe({
      next: res => {
        this.usuario = res.usuario;
        this.perfis  = res.perfis;
        if (this.perfis.length === 0) {
          const savedId = this.profile.getSavedId();
          if (savedId) {
            this.api.vincularPerfil(savedId).subscribe({
              next: p => { this.perfis = [p]; this.carregando = false; this.tela = 'perfis'; },
              error: () => { this.carregando = false; this.tela = 'cadastro'; }
            });
            return;
          }
          this.carregando = false;
          this.tela = 'cadastro';
        } else {
          this.carregando = false;
          this.tela = 'perfis';
        }
      },
      error: () => { this.erro = 'Erro ao autenticar com Google.'; this.carregando = false; }
    });
  }

  // ── Login email/senha ─────────────────────────────
  fazerLogin() {
    if (!this.loginEmail || !this.loginSenha) { this.erro = 'Preencha e-mail e senha.'; return; }
    this.salvando = true;
    this.erro = '';
    this.auth.login(this.loginEmail, this.loginSenha).subscribe({
      next: res => this.aposAuth(res),
      error: err => {
        this.erro = err.error?.erro ?? 'E-mail ou senha incorretos.';
        this.salvando = false;
      }
    });
  }

  // ── Registro email/senha ──────────────────────────
  fazerRegistro() {
    if (!this.regNome.trim()) { this.erro = 'Digite seu nome.'; return; }
    if (!this.regEmail)       { this.erro = 'Digite seu e-mail.'; return; }
    if (this.regSenha.length < 8) { this.erro = 'A senha deve ter pelo menos 8 caracteres.'; return; }
    if (!this.regSenha.match(/[A-Z]/)) { this.erro = 'A senha deve conter pelo menos uma letra maiúscula.'; return; }
    if (!this.regSenha.match(/[0-9]/)) { this.erro = 'A senha deve conter pelo menos um número.'; return; }
    if (this.regSenha !== this.regConfirma) { this.erro = 'As senhas não coincidem.'; return; }
    this.salvando = true;
    this.erro = '';
    this.auth.registrar(this.regNome, this.regEmail, this.regSenha).subscribe({
      next: res => this.aposAuth(res),
      error: err => {
        this.erro = err.error?.erro ?? 'Erro ao criar conta.';
        this.salvando = false;
      }
    });
  }

  private aposAuth(res: { usuario: any; perfis: Perfil[] }) {
    this.usuario = res.usuario;
    this.perfis  = res.perfis;
    this.salvando = false;
    if (this.perfis.length === 0) {
      const savedId = this.profile.getSavedId();
      if (savedId) {
        this.api.vincularPerfil(savedId).subscribe({
          next: p => { this.perfis = [p]; this.tela = 'perfis'; },
          error: () => { this.tela = 'cadastro'; }
        });
        return;
      }
      this.tela = 'cadastro';
    } else {
      this.tela = 'perfis';
    }
  }

  // ── Carregar perfis do usuário já logado ──────────
  carregarPerfisDoUsuario() {
    this.carregando = true;
    this.api.getMeusPerfis().subscribe({
      next: p => { this.perfis = p; this.carregando = false; this.tela = 'perfis'; },
      error: () => { this.carregando = false; this.tela = 'inicio'; }
    });
  }

  // ── Seleção / criação de perfil ───────────────────
  selecionar(perfil: Perfil) {
    this.profile.setPerfilAtivo(perfil);
    this.router.navigate(['/']);
  }

  irCadastro() {
    this.novoNome = '';
    this.erro = '';
    this.classeSelecionada = null;
    this.generoSelecionado = null;
    this.removerFoto();
    this.tela = 'cadastro';
  }

  criar() {
    if (!this.novoNome.trim())   { this.erro = 'Digite seu nome de herói.'; return; }
    if (!this.classeSelecionada) { this.erro = 'Escolha uma classe para continuar.'; return; }
    if (this.salvando) return;
    this.erro = '';
    this.salvando = true;
    this.api.criarPerfil(this.novoNome.trim(), this.classeSelecionada, this.generoSelecionado).subscribe({
      next: p => {
        if (this.fotoFile) {
          this.api.uploadFoto(p.id, this.fotoFile).subscribe({
            next: a => this.entrar(a),
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

  voltar() {
    this.erro = '';
    this.tela = this.usuario ? 'perfis' : 'inicio';
  }

  logout() {
    this.auth.logout();
    this.profile.clearPerfil();
    this.usuario = null;
    this.perfis = [];
    this.tela = 'inicio';
    setTimeout(() => this.tentarIniciarGoogle(), 200);
  }

  // ── Foto ──────────────────────────────────────────
  onFotoSelecionada(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    if (!['image/jpeg','image/png','image/webp','image/gif'].includes(file.type)) { this.erro = 'Use JPG, PNG, GIF ou WebP.'; return; }
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

  get podeCriarMais(): boolean { return this.perfis.length < 3; }
  get senhaTemMaiuscula(): boolean { return /[A-Z]/.test(this.regSenha); }
  get senhaTemNumero(): boolean { return /[0-9]/.test(this.regSenha); }
  get senhaForca(): number {
    let f = 0;
    if (this.regSenha.length >= 8) f++;
    if (this.senhaTemMaiuscula) f++;
    if (this.senhaTemNumero) f++;
    if (/[^A-Za-z0-9]/.test(this.regSenha)) f++;
    return f;
  }
}
