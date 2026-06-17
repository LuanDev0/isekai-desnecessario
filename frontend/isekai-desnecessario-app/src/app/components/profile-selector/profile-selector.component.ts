import { Component, OnInit, OnDestroy, inject, HostListener } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { Perfil } from '../../models/models';

const API_BASE = environment.apiUrl.replace('/api', '');

@Component({
  selector: 'app-profile-selector',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-selector.component.html',
  styleUrl: './profile-selector.component.scss',
})
export class ProfileSelectorComponent implements OnInit, OnDestroy {
  private api     = inject(ApiService);
  public  profile = inject(ProfileService);
  private router  = inject(Router);
  private sub?: Subscription;

  perfis:  Perfil[]  = [];
  aberto = false;

  ngOnInit() {
    this.carregarPerfis();
    // Recarrega o combo toda vez que a foto do perfil ativo mudar
    this.sub = this.profile.fotoAtualizada$.subscribe(() => this.carregarPerfis());
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
  }

  carregarPerfis() {
    this.api.getMeusPerfis().subscribe({ next: p => this.perfis = p });
  }

  get atual(): Perfil | null {
    return this.profile.perfilAtivo();
  }

  fotoUrl(perfil: Perfil): string | null {
    const url = perfil.fotoUrl;
    if (!url) return null;
    if (url.startsWith('data:')) return url;
    if (url.startsWith('/')) return `${API_BASE}${url}`;
    return null;
  }

  inicial(nome: string) {
    return nome?.[0]?.toUpperCase() ?? '?';
  }

  trocar(perfil: Perfil) {
    this.profile.setPerfilAtivo(perfil);
    this.aberto = false;
    window.location.reload(); // recarrega dados do novo perfil
  }

  irParaCadastro() {
    this.aberto = false;
    this.router.navigate(['/cadastro']);
  }

  irParaLogin() {
    this.aberto = false;
    this.profile.clearPerfil();
    this.router.navigate(['/cadastro']);
  }

  @HostListener('document:click', ['$event'])
  fecharAoClicarFora(event: MouseEvent) {
    const el = event.target as HTMLElement;
    if (!el.closest('.profile-selector')) this.aberto = false;
  }
}
