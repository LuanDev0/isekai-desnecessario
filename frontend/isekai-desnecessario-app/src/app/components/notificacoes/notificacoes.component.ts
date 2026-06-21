import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { LanguageService } from '../../services/language.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { Notificacao } from '../../models/models';

@Component({
  selector: 'app-notificacoes',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './notificacoes.component.html',
  styleUrl: './notificacoes.component.scss',
})
export class NotificacoesComponent implements OnInit {
  private api  = inject(ApiService);
  private auth = inject(AuthService);
  readonly lang = inject(LanguageService);

  notificacoes: Notificacao[] = [];
  aberto = false;

  ngOnInit() {
    this.carregar();
  }

  get naoLidas(): number {
    return this.notificacoes.filter(n => !n.lida).length;
  }

  carregar() {
    if (!this.auth.isLogado()) return;
    this.api.getNotificacoes().subscribe({
      next: n => this.notificacoes = n,
      error: () => {},
    });
  }

  toggle() {
    this.aberto = !this.aberto;
    if (this.aberto) this.carregar();
  }

  fechar() {
    this.aberto = false;
  }

  marcarLida(n: Notificacao) {
    if (n.lida) return;
    n.lida = true; // otimista
    this.api.marcarNotificacaoLida(n.id).subscribe({ error: () => n.lida = false });
  }

  marcarTodas() {
    if (this.naoLidas === 0) return;
    this.notificacoes.forEach(n => n.lida = true); // otimista
    this.api.marcarTodasNotificacoesLidas().subscribe({ error: () => this.carregar() });
  }

  emojiTipo(tipo: string): string {
    switch (tipo) {
      case 'pendente':   return '📥';
      case 'rejeitado':  return '🚫';
      case 'modificado': return '✏️';
      default:           return '🔔';
    }
  }

  // Tempo relativo simples ("agora", "5min", "3h", data)
  quando(data: string): string {
    const d = new Date(data);
    const diff = Math.floor((Date.now() - d.getTime()) / 60000);
    if (diff < 1)    return this.lang.translate('dashboard.agora');
    if (diff < 60)   return this.lang.translate('dashboard.minAtras', { min: diff });
    if (diff < 1440) return this.lang.translate('dashboard.hAtras', { h: Math.floor(diff / 60) });
    const locale = this.lang.current() === 'en' ? 'en-US' : 'pt-BR';
    return d.toLocaleDateString(locale, { day: '2-digit', month: '2-digit' });
  }
}
