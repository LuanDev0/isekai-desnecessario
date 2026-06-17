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

  loginComGoogle(idToken: string) {
    return this.http
      .post<{ token: string; usuario: Usuario; perfis: Perfil[] }>(
        `${environment.apiUrl}/auth/google`,
        { idToken }
      )
      .pipe(
        tap(res => {
          localStorage.setItem(TOKEN_KEY, res.token);
          localStorage.setItem(USUARIO_KEY, JSON.stringify(res.usuario));
          this._usuario.set(res.usuario);
        })
      );
  }

  logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USUARIO_KEY);
    this._usuario.set(null);
    try { google.accounts.id.disableAutoSelect(); } catch {}
  }

  initGoogleSignIn(callback: (idToken: string) => void) {
    google.accounts.id.initialize({
      client_id: environment.googleClientId,
      callback:  (response: any) => callback(response.credential),
    });
  }

  renderGoogleButton(element: HTMLElement) {
    google.accounts.id.renderButton(element, {
      theme:  'outline',
      size:   'large',
      width:  280,
      locale: 'pt-BR',
    });
  }
}
