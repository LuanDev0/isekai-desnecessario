import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Usuario, Perfil } from '../models/models';

const TOKEN_KEY   = 'isekai_jwt';
const USUARIO_KEY = 'isekai_usuario';

declare const google: any;

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);

  private _usuario = signal<Usuario | null>(null);
  readonly usuario = this._usuario.asReadonly();

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
      callback:  (response: any) => callback(response.credential),
      auto_select: false,
      cancel_on_tap_outside: true,
    });
  }

  abrirPopupGoogle() {
    if (!(window as any)['google']?.accounts?.id) return false;
    if (this.googleCallback) {
      google.accounts.id.initialize({
        client_id: environment.googleClientId,
        callback:  (response: any) => this.googleCallback!(response.credential),
        auto_select: false,
        cancel_on_tap_outside: true,
      });
    }
    google.accounts.id.prompt();
    return true;
  }

  get googleDisponivel(): boolean {
    return !!(window as any)['google']?.accounts?.id;
  }
}
