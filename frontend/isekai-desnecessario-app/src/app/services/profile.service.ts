import { Injectable, signal } from '@angular/core';
import { Subject } from 'rxjs';
import { Perfil } from '../models/models';

const STORAGE_KEY = 'solo_perfil_id';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private _perfilAtivo = signal<Perfil | null>(null);

  /** Emite quando a foto do perfil ativo é trocada — selector escuta para recarregar a lista */
  readonly fotoAtualizada$ = new Subject<void>();

  readonly perfilAtivo = this._perfilAtivo.asReadonly();

  getSavedId(): number | null {
    const v = localStorage.getItem(STORAGE_KEY);
    return v ? Number(v) : null;
  }

  setPerfilAtivo(perfil: Perfil) {
    const anterior = this._perfilAtivo();
    const fotoMudou = anterior?.id === perfil.id && anterior?.fotoUrl !== perfil.fotoUrl;
    this._perfilAtivo.set(perfil);
    localStorage.setItem(STORAGE_KEY, String(perfil.id));
    if (fotoMudou) this.fotoAtualizada$.next();
  }

  clearPerfil() {
    this._perfilAtivo.set(null);
    localStorage.removeItem(STORAGE_KEY);
  }

  get id(): number {
    return this._perfilAtivo()?.id ?? this.getSavedId() ?? 0;
  }
}
