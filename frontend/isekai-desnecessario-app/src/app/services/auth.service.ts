import { Injectable, inject, signal, computed, NgZone } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Usuario, Perfil, Role } from '../models/models';

const TOKEN_KEY   = 'isekai_jwt';
const USUARIO_KEY = 'isekai_usuario';

declare const google: any;

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private zone = inject(NgZone);

  private _usuario = signal<Usuario | null>(null);
  readonly usuario = this._usuario.asReadonly();

  // Papel de acesso da conta logada (default 'Usuario' quando deslogado).
  readonly role = computed<Role>(() => this._usuario()?.role ?? 'Usuario');
  readonly isAdmin     = computed(() => this.role() === 'Admin');
  readonly isModerador = computed(() => this.role() === 'Moderador');
  // Quem pode criar conteúdo próprio/privado: VIP, Moderador e Admin.
  readonly podeConteudoProprio = computed(() => this.role() !== 'Usuario');
  // Quem pode criar conteúdo no catálogo global: Admin e Moderador.
  readonly podeCatalogoGlobal  = computed(() => this.isAdmin() || this.isModerador());

  constructor() {
    const saved = localStorage.getItem(USUARIO_KEY);
    if (saved) this._usuario.set(JSON.parse(saved));
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isLogado(): boolean {
    return !!this.getToken();
  }

  private salvarSessao(res: { token: string; usuario: Usuario }) {
    localStorage.setItem(TOKEN_KEY, res.token);
    localStorage.setItem(USUARIO_KEY, JSON.stringify(res.usuario));
    this._usuario.set(res.usuario);
  }

  loginComGoogle(idToken: string) {
    return this.http
      .post<{ token: string; usuario: Usuario; perfis: Perfil[] }>(
        `${environment.apiUrl}/auth/google`, { idToken }
      )
      .pipe(tap(res => this.salvarSessao(res)));
  }

  registrar(nome: string, email: string, senha: string) {
    return this.http
      .post<{ token: string; usuario: Usuario; perfis: Perfil[] }>(
        `${environment.apiUrl}/auth/registrar`, { nome, email, senha }
      )
      .pipe(tap(res => this.salvarSessao(res)));
  }

  login(email: string, senha: string) {
    return this.http
      .post<{ token: string; usuario: Usuario; perfis: Perfil[] }>(
        `${environment.apiUrl}/auth/login`, { email, senha }
      )
      .pipe(tap(res => this.salvarSessao(res)));
  }

  logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USUARIO_KEY);
    this._usuario.set(null);
    try { google.accounts.id.disableAutoSelect(); } catch {}
  }

  private googleCallback: ((idToken: string) => void) | null = null;

  initGoogleSignIn(callback: (idToken: string) => void) {
    this.googleCallback = callback;
    if (!(window as any)['google']?.accounts?.id) return;
    google.accounts.id.initialize({
      client_id: environment.googleClientId,
      // Wraps callback in zone so Angular detects changes
      callback:  (response: any) => this.zone.run(() => callback(response.credential)),
      auto_select: false,
      cancel_on_tap_outside: true,
    });
  }

  abrirPopupGoogle(): boolean {
    if (!(window as any)['google']?.accounts?.id) return false;
    google.accounts.id.initialize({
      client_id: environment.googleClientId,
      callback:  (response: any) => this.zone.run(() => this.googleCallback?.(response.credential)),
      auto_select: false,
      cancel_on_tap_outside: true,
    });
    google.accounts.id.prompt();
    return true;
  }

  get googleDisponivel(): boolean {
    return !!(window as any)['google']?.accounts?.id;
  }
}
