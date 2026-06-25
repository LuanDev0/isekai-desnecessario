import { Component, Input } from '@angular/core';
import { NgSwitch, NgSwitchCase, NgSwitchDefault } from '@angular/common';

@Component({
  selector: 'app-icon',
  standalone: true,
  imports: [NgSwitch, NgSwitchCase, NgSwitchDefault],
  template: `
    <svg [attr.width]="size" [attr.height]="size" viewBox="0 0 48 48" fill="none"
         [attr.aria-label]="name" role="img" style="display:inline-block;vertical-align:middle;">
      <ng-container [ngSwitch]="name">

        <!-- ══ PACK 1 ══════════════════════════════════════════════ -->

        <ng-container *ngSwitchCase="'espada'">
          <path d="M23 10 L25 10 L26 34 L24 36 L22 34 Z" fill="#9E9E9E" stroke="#757575" stroke-width="1.2" stroke-linejoin="round"/>
          <line x1="24" y1="11" x2="24" y2="33" stroke="#E0E0E0" stroke-width="0.8" stroke-linecap="round"/>
          <line x1="16" y1="34" x2="32" y2="34" stroke="#757575" stroke-width="2.2" stroke-linecap="round"/>
          <line x1="24" y1="36" x2="24" y2="41" stroke="#6D4C41" stroke-width="3" stroke-linecap="round"/>
          <circle cx="24" cy="42.5" r="2" fill="#5D4037"/>
        </ng-container>

        <ng-container *ngSwitchCase="'trofeu'">
          <path d="M24 12 L22 18 L16 18 L21 22 L19 28 L24 24 L29 28 L27 22 L32 18 L26 18 Z" fill="#C8A84B22" stroke="#8C6D1F" stroke-width="1.6" stroke-linejoin="round"/>
          <path d="M18 34 C14 30 12 24 14 18" stroke="#8C6D1F" stroke-width="1.8" stroke-linecap="round" fill="none"/>
          <path d="M30 34 C34 30 36 24 34 18" stroke="#8C6D1F" stroke-width="1.8" stroke-linecap="round" fill="none"/>
          <path d="M18 34 C18 37 22 38 24 38 C26 38 30 37 30 34" stroke="#8C6D1F" stroke-width="1.6" fill="#C8A84B18"/>
          <line x1="20" y1="38" x2="28" y2="38" stroke="#8C6D1F" stroke-width="2" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'coroa'">
          <path d="M8 34 L12 18 L20 26 L24 14 L28 26 L36 18 L40 34 Z" fill="#C8A84B22" stroke="#8C6D1F" stroke-width="2" stroke-linejoin="round"/>
          <line x1="8" y1="36" x2="40" y2="36" stroke="#8C6D1F" stroke-width="2.5" stroke-linecap="round"/>
          <circle cx="24" cy="14" r="2.5" fill="#C8A84B" stroke="#8C6D1F" stroke-width="1"/>
          <circle cx="12" cy="18" r="2" fill="#C8A84B" stroke="#8C6D1F" stroke-width="1"/>
          <circle cx="36" cy="18" r="2" fill="#C8A84B" stroke="#8C6D1F" stroke-width="1"/>
        </ng-container>

        <ng-container *ngSwitchCase="'check'">
          <circle cx="24" cy="24" r="16" fill="#4CAF5018" stroke="#388E3C" stroke-width="2"/>
          <polyline points="15,24 22,31 33,17" stroke="#388E3C" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
        </ng-container>

        <ng-container *ngSwitchCase="'gema'">
          <path d="M24 10 L36 20 L30 38 L18 38 L12 20 Z" fill="#5C4A7222" stroke="#5C4A72" stroke-width="2" stroke-linejoin="round"/>
          <line x1="12" y1="20" x2="36" y2="20" stroke="#5C4A7255" stroke-width="1.2"/>
          <path d="M18 14 Q24 10 30 14" stroke="#B8A8D0" stroke-width="1.2" stroke-linecap="round" fill="none"/>
        </ng-container>

        <!-- ══ PACK 2 — RANKS ════════════════════════════════════════ -->

        <ng-container *ngSwitchCase="'rank-e'">
          <circle cx="24" cy="24" r="16" fill="#5A8A5A22" stroke="#5A8A5A" stroke-width="2"/>
          <text x="24" y="31" text-anchor="middle" font-size="18" fill="#5A8A5A" font-family="sans-serif" font-weight="700">E</text>
        </ng-container>

        <ng-container *ngSwitchCase="'rank-d'">
          <circle cx="24" cy="24" r="16" fill="#7A9A5A22" stroke="#7A9A5A" stroke-width="2"/>
          <text x="24" y="31" text-anchor="middle" font-size="18" fill="#7A9A5A" font-family="sans-serif" font-weight="700">D</text>
        </ng-container>

        <ng-container *ngSwitchCase="'rank-c'">
          <circle cx="24" cy="24" r="16" fill="#4A7A9A22" stroke="#4A7A9A" stroke-width="2"/>
          <text x="24" y="31" text-anchor="middle" font-size="18" fill="#4A7A9A" font-family="sans-serif" font-weight="700">C</text>
        </ng-container>

        <ng-container *ngSwitchCase="'rank-b'">
          <circle cx="24" cy="24" r="16" fill="#5A4A9A22" stroke="#5A4A9A" stroke-width="2"/>
          <text x="24" y="31" text-anchor="middle" font-size="18" fill="#5A4A9A" font-family="sans-serif" font-weight="700">B</text>
        </ng-container>

        <ng-container *ngSwitchCase="'rank-a'">
          <circle cx="24" cy="24" r="16" fill="#9A4A5A22" stroke="#9A4A5A" stroke-width="2"/>
          <text x="24" y="31" text-anchor="middle" font-size="18" fill="#9A4A5A" font-family="sans-serif" font-weight="700">A</text>
        </ng-container>

        <ng-container *ngSwitchCase="'rank-s'">
          <circle cx="24" cy="24" r="16" fill="#8C6D1F22" stroke="#8C6D1F" stroke-width="2"/>
          <text x="24" y="31" text-anchor="middle" font-size="18" fill="#8C6D1F" font-family="sans-serif" font-weight="700">S</text>
        </ng-container>

        <ng-container *ngSwitchCase="'rank-ss'">
          <circle cx="24" cy="24" r="16" fill="#C8A84B18" stroke="#8C6D1F" stroke-width="2"/>
          <polygon points="16,8 18,14 24,14 19,18 21,24 16,20 11,24 13,18 8,14 14,14" fill="#C8A84B22" stroke="#8C6D1F" stroke-width="1.4" stroke-linejoin="round"/>
          <polygon points="32,8 34,14 40,14 35,18 37,24 32,20 27,24 29,18 24,14 30,14" fill="#C8A84B22" stroke="#8C6D1F" stroke-width="1.4" stroke-linejoin="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'rank-sss'">
          <path d="M24 8 L25.5 15 L32 13 L27.5 19 L34 22 L27 23.5 L30 30 L24 26 L18 30 L21 23.5 L14 22 L20.5 19 L16 13 L22.5 15 Z" fill="#C8A84B22" stroke="#8C6D1F" stroke-width="1.6" stroke-linejoin="round"/>
          <path d="M24 18 L25 21 L28 21 L25.5 23 L26.5 26 L24 24.5 L21.5 26 L22.5 23 L20 21 L23 21 Z" fill="#C8A84B" stroke="#8C6D1F" stroke-width="1"/>
          <text x="24" y="43" text-anchor="middle" font-size="9" fill="#8C6D1F" font-family="sans-serif" font-weight="700">SSS</text>
        </ng-container>

        <!-- ══ PACK 3 — UI ════════════════════════════════════════════ -->

        <ng-container *ngSwitchCase="'sino'">
          <path d="M24 8 C24 8 16 12 14 22 L12 32 L36 32 L34 22 C32 12 24 8 24 8 Z" fill="#C8A84B22" stroke="#8C6D1F" stroke-width="2" stroke-linejoin="round"/>
          <line x1="12" y1="32" x2="36" y2="32" stroke="#8C6D1F" stroke-width="2" stroke-linecap="round"/>
          <path d="M20 32 Q20 37 24 37 Q28 37 28 32" stroke="#8C6D1F" stroke-width="1.8" fill="none"/>
          <circle cx="24" cy="8" r="2.5" fill="#C8A84B44" stroke="#8C6D1F" stroke-width="1.5"/>
        </ng-container>

        <ng-container *ngSwitchCase="'loja'">
          <rect x="10" y="30" width="28" height="10" rx="2" fill="#C8A84B22" stroke="#8C6D1F" stroke-width="1.8"/>
          <path d="M8 30 L16 16 L24 20 L32 16 L40 30 Z" fill="#C8A84B22" stroke="#8C6D1F" stroke-width="1.8" stroke-linejoin="round"/>
          <path d="M8 30 L11 34 M13 30 L14 34 M18 30 L18 34 M23 30 L23 34 M28 30 L28 34 M33 30 L33 34 M38 30 L37 34 M40 30 L41 34" stroke="#8C6D1F" stroke-width="1.2" stroke-linecap="round"/>
          <line x1="16" y1="16" x2="32" y2="16" stroke="#8C6D1F" stroke-width="2" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'moedas'">
          <ellipse cx="22" cy="30" rx="10" ry="4" fill="#C8A84B22" stroke="#8C6D1F" stroke-width="1.8"/>
          <rect x="12" y="22" width="20" height="8" fill="#C8A84B18"/>
          <line x1="12" y1="22" x2="12" y2="30" stroke="#8C6D1F" stroke-width="1.8"/>
          <line x1="32" y1="22" x2="32" y2="30" stroke="#8C6D1F" stroke-width="1.8"/>
          <ellipse cx="22" cy="22" rx="10" ry="4" fill="#C8A84B33" stroke="#8C6D1F" stroke-width="1.8"/>
          <ellipse cx="24" cy="18" rx="10" ry="4" fill="#C8A84B22" stroke="#8C6D1F" stroke-width="1.8"/>
          <rect x="14" y="10" width="20" height="8" fill="#C8A84B18"/>
          <line x1="14" y1="10" x2="14" y2="18" stroke="#8C6D1F" stroke-width="1.8"/>
          <line x1="34" y1="10" x2="34" y2="18" stroke="#8C6D1F" stroke-width="1.8"/>
          <ellipse cx="24" cy="10" rx="10" ry="4" fill="#C8A84B44" stroke="#8C6D1F" stroke-width="1.8"/>
          <path d="M16 8 Q20 7 26 8" stroke="#FAD97A" stroke-width="1" stroke-linecap="round" fill="none"/>
        </ng-container>

        <ng-container *ngSwitchCase="'entrada'">
          <path d="M8 28 L8 40 L40 40 L40 28" stroke="#4A7A9A" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="#4A7A9A18"/>
          <path d="M8 28 L14 20 L34 20 L40 28 L28 28 Q28 34 24 34 Q20 34 20 28 Z" fill="#4A7A9A18" stroke="#4A7A9A" stroke-width="2" stroke-linejoin="round"/>
          <line x1="24" y1="8" x2="24" y2="17" stroke="#4A7A9A" stroke-width="2" stroke-linecap="round"/>
          <polyline points="19,13 24,18 29,13" stroke="#4A7A9A" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
        </ng-container>

        <ng-container *ngSwitchCase="'rejeitado'">
          <circle cx="24" cy="24" r="16" fill="#C0392B18" stroke="#C0392B" stroke-width="2.2"/>
          <line x1="16" y1="16" x2="32" y2="32" stroke="#C0392B" stroke-width="2.5" stroke-linecap="round"/>
          <line x1="32" y1="16" x2="16" y2="32" stroke="#C0392B" stroke-width="2.5" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'editado'">
          <path d="M30 10 L38 18 L18 38 L10 38 L10 30 Z" fill="#4A7A9A18" stroke="#4A7A9A" stroke-width="2" stroke-linejoin="round"/>
          <line x1="32" y1="12" x2="36" y2="16" stroke="#4A7A9A" stroke-width="1.5"/>
          <path d="M10 30 L14 34 L10 38 Z" fill="#4A7A9A44" stroke="#4A7A9A" stroke-width="1.2" stroke-linejoin="round"/>
          <line x1="8" y1="42" x2="24" y2="42" stroke="#4A7A9A88" stroke-width="1.5" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'mochila'">
          <rect x="12" y="18" width="24" height="24" rx="5" fill="#8B5E3C18" stroke="#8B5E3C" stroke-width="2"/>
          <path d="M18 18 L18 12 Q18 8 24 8 Q30 8 30 12 L30 18" stroke="#8B5E3C" stroke-width="2" fill="none" stroke-linecap="round"/>
          <rect x="17" y="28" width="14" height="9" rx="3" fill="none" stroke="#8B5E3C" stroke-width="1.5"/>
          <line x1="21" y1="32" x2="27" y2="32" stroke="#8B5E3C88" stroke-width="1.2" stroke-linecap="round"/>
          <line x1="12" y1="22" x2="8" y2="34" stroke="#8B5E3C" stroke-width="1.8" stroke-linecap="round"/>
          <line x1="36" y1="22" x2="40" y2="34" stroke="#8B5E3C" stroke-width="1.8" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'timer'">
          <circle cx="24" cy="28" r="15" fill="#5C4A7218" stroke="#5C4A72" stroke-width="2"/>
          <line x1="24" y1="28" x2="24" y2="18" stroke="#5C4A72" stroke-width="2.2" stroke-linecap="round"/>
          <line x1="24" y1="28" x2="31" y2="32" stroke="#5C4A72" stroke-width="1.8" stroke-linecap="round"/>
          <rect x="20" y="10" width="8" height="4" rx="2" fill="#5C4A7222" stroke="#5C4A72" stroke-width="1.5"/>
          <path d="M38 14 Q44 8 38 4" stroke="#5C4A72" stroke-width="1.8" stroke-linecap="round" fill="none"/>
          <path d="M44 6 L38 4 L40 10" stroke="#5C4A72" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
          <line x1="24" y1="14" x2="24" y2="16" stroke="#5C4A7288" stroke-width="1.2" stroke-linecap="round"/>
          <line x1="34" y1="18" x2="33" y2="20" stroke="#5C4A7288" stroke-width="1.2" stroke-linecap="round"/>
          <line x1="14" y1="18" x2="15" y2="20" stroke="#5C4A7288" stroke-width="1.2" stroke-linecap="round"/>
          <line x1="38" y1="28" x2="36" y2="28" stroke="#5C4A7288" stroke-width="1.2" stroke-linecap="round"/>
          <line x1="10" y1="28" x2="12" y2="28" stroke="#5C4A7288" stroke-width="1.2" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'camera'">
          <rect x="6" y="18" width="36" height="26" rx="4" fill="#6B6B8A18" stroke="#6B6B8A" stroke-width="2"/>
          <path d="M16 18 L18 12 L30 12 L32 18" stroke="#6B6B8A" stroke-width="1.8" stroke-linejoin="round" fill="#6B6B8A12"/>
          <circle cx="24" cy="31" r="8" fill="none" stroke="#6B6B8A" stroke-width="2"/>
          <circle cx="24" cy="31" r="4.5" fill="#6B6B8A22" stroke="#6B6B8A" stroke-width="1.2"/>
          <circle cx="36" cy="22" r="2.5" fill="#6B6B8A33" stroke="#6B6B8A" stroke-width="1.2"/>
        </ng-container>

        <ng-container *ngSwitchCase="'lampada'">
          <path d="M18 28 Q14 22 14 18 Q14 10 24 10 Q34 10 34 18 Q34 22 30 28 Z" fill="#C8A84B22" stroke="#8C6D1F" stroke-width="2" stroke-linejoin="round"/>
          <rect x="19" y="28" width="10" height="3" rx="1" fill="#8C6D1F22" stroke="#8C6D1F" stroke-width="1.5"/>
          <rect x="20" y="31" width="8" height="3" rx="1" fill="#8C6D1F22" stroke="#8C6D1F" stroke-width="1.5"/>
          <rect x="19" y="34" width="10" height="3" rx="1" fill="#8C6D1F22" stroke="#8C6D1F" stroke-width="1.5"/>
          <path d="M20 20 Q22 17 24 20 Q26 23 28 20" stroke="#FAD97A" stroke-width="1.5" stroke-linecap="round" fill="none"/>
          <line x1="20" y1="20" x2="20" y2="26" stroke="#8C6D1F66" stroke-width="1" stroke-linecap="round"/>
          <line x1="28" y1="20" x2="28" y2="26" stroke="#8C6D1F66" stroke-width="1" stroke-linecap="round"/>
          <line x1="24" y1="5" x2="24" y2="7" stroke="#C8A84B" stroke-width="1.4" stroke-linecap="round"/>
          <line x1="35" y1="10" x2="33" y2="12" stroke="#C8A84B" stroke-width="1.2" stroke-linecap="round"/>
          <line x1="13" y1="10" x2="15" y2="12" stroke="#C8A84B" stroke-width="1.2" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'pergaminho'">
          <path d="M12 8 L34 8 L40 14 L40 42 L12 42 Z" fill="#8B5E3C18" stroke="#8B5E3C" stroke-width="1.8" stroke-linejoin="round"/>
          <path d="M34 8 L34 14 L40 14" stroke="#8B5E3C" stroke-width="1.5" fill="#8B5E3C22"/>
          <line x1="18" y1="22" x2="34" y2="22" stroke="#8B5E3C55" stroke-width="1.3" stroke-linecap="round"/>
          <line x1="18" y1="27" x2="34" y2="27" stroke="#8B5E3C55" stroke-width="1.3" stroke-linecap="round"/>
          <line x1="18" y1="32" x2="28" y2="32" stroke="#8B5E3C55" stroke-width="1.3" stroke-linecap="round"/>
          <path d="M16 16 Q12 12 14 8 Q18 10 16 16 Z" fill="#8B5E3C33" stroke="#8B5E3C" stroke-width="1.2" stroke-linejoin="round"/>
          <line x1="14" y1="8" x2="16" y2="16" stroke="#8B5E3C88" stroke-width="0.8"/>
        </ng-container>

        <ng-container *ngSwitchCase="'mapa'">
          <rect x="7" y="9" width="34" height="30" rx="3" fill="#4A7A9A10" stroke="#4A7A9A" stroke-width="1.8"/>
          <line x1="7" y1="17" x2="41" y2="17" stroke="#4A7A9A22" stroke-width="0.8"/>
          <line x1="7" y1="25" x2="41" y2="25" stroke="#4A7A9A22" stroke-width="0.8"/>
          <line x1="7" y1="33" x2="41" y2="33" stroke="#4A7A9A22" stroke-width="0.8"/>
          <line x1="16" y1="9" x2="16" y2="39" stroke="#4A7A9A22" stroke-width="0.8"/>
          <line x1="25" y1="9" x2="25" y2="39" stroke="#4A7A9A22" stroke-width="0.8"/>
          <line x1="34" y1="9" x2="34" y2="39" stroke="#4A7A9A22" stroke-width="0.8"/>
          <path d="M11 33 L11 21 L34 21" stroke="#8C6D1F" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
          <circle cx="11" cy="33" r="2.5" fill="#4A7A9A" stroke="#4A7A9A" stroke-width="1"/>
          <circle cx="34" cy="20" r="3" fill="#C8A84B33" stroke="#8C6D1F" stroke-width="1.5"/>
          <line x1="34" y1="23" x2="34" y2="26" stroke="#8C6D1F" stroke-width="1.5" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'cadeado'">
          <path d="M16 22 L16 16 Q16 8 24 8 Q32 8 32 16 L32 22" stroke="#6B6B8A" stroke-width="2.2" fill="none" stroke-linecap="round"/>
          <rect x="11" y="22" width="26" height="20" rx="4" fill="#6B6B8A18" stroke="#6B6B8A" stroke-width="2"/>
          <circle cx="24" cy="30" r="3.5" fill="none" stroke="#6B6B8A" stroke-width="1.8"/>
          <line x1="24" y1="33" x2="24" y2="37" stroke="#6B6B8A" stroke-width="1.8" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'aviso'">
          <path d="M24 8 L42 40 L6 40 Z" fill="#D4880018" stroke="#D48800" stroke-width="2.2" stroke-linejoin="round"/>
          <line x1="24" y1="20" x2="24" y2="31" stroke="#D48800" stroke-width="2.5" stroke-linecap="round"/>
          <circle cx="24" cy="35.5" r="2" fill="#D48800"/>
        </ng-container>

        <ng-container *ngSwitchCase="'lixeira'">
          <rect x="11" y="12" width="26" height="4" rx="2" fill="#C0392B18" stroke="#C0392B" stroke-width="1.8"/>
          <rect x="20" y="8" width="8" height="5" rx="2" fill="none" stroke="#C0392B" stroke-width="1.8"/>
          <path d="M14 16 L16 40 L32 40 L34 16 Z" fill="#C0392B18" stroke="#C0392B" stroke-width="1.8" stroke-linejoin="round"/>
          <line x1="20" y1="20" x2="20" y2="36" stroke="#C0392B66" stroke-width="1.3" stroke-linecap="round"/>
          <line x1="24" y1="20" x2="24" y2="36" stroke="#C0392B66" stroke-width="1.3" stroke-linecap="round"/>
          <line x1="28" y1="20" x2="28" y2="36" stroke="#C0392B66" stroke-width="1.3" stroke-linecap="round"/>
        </ng-container>

        <!-- ══ PACK 4 — EXTRAS ═══════════════════════════════════════ -->

        <ng-container *ngSwitchCase="'perfil'">
          <circle cx="24" cy="24" r="18" fill="#6B6B8A18" stroke="#6B6B8A" stroke-width="2"/>
          <circle cx="24" cy="19" r="6" fill="#6B6B8A22" stroke="#6B6B8A" stroke-width="1.8"/>
          <path d="M12 36 Q12 28 24 28 Q36 28 36 36" fill="#6B6B8A22" stroke="#6B6B8A" stroke-width="1.8" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'raio'">
          <path d="M29 6 L15 26 L23 26 L19 42 L33 22 L25 22 Z" fill="#C8A84B22" stroke="#C8A84B" stroke-width="2" stroke-linejoin="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'alvo'">
          <circle cx="24" cy="24" r="18" fill="none" stroke="#C0392B" stroke-width="1.8"/>
          <circle cx="24" cy="24" r="12" fill="#C0392B18" stroke="#C0392B" stroke-width="1.8"/>
          <circle cx="24" cy="24" r="6" fill="#C0392B33" stroke="#C0392B" stroke-width="1.8"/>
          <circle cx="24" cy="24" r="2.5" fill="#C0392B"/>
        </ng-container>

        <ng-container *ngSwitchCase="'frasco'">
          <rect x="19" y="6" width="10" height="4" rx="2" fill="#4A7A9A22" stroke="#4A7A9A" stroke-width="1.5"/>
          <path d="M20 10 L14 28 Q10 42 24 42 Q38 42 34 28 L28 10 Z" fill="#4A7A9A18" stroke="#4A7A9A" stroke-width="2" stroke-linejoin="round"/>
          <path d="M14 32 Q12 42 24 42 Q36 42 34 32 L28 22 L20 22 Z" fill="#4A7A9A33"/>
          <circle cx="19" cy="36" r="2" fill="#4A7A9A55"/>
          <circle cx="26" cy="33" r="1.5" fill="#4A7A9A55"/>
        </ng-container>

        <ng-container *ngSwitchCase="'estrela'">
          <path d="M24 8 L27.1 18 L38 18 L29.5 24.2 L32.6 34.2 L24 28 L15.4 34.2 L18.5 24.2 L10 18 L20.9 18 Z" fill="#C8A84B22" stroke="#C8A84B" stroke-width="2" stroke-linejoin="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'carrinho'">
          <path d="M6 8 L12 12 L16 28 L38 28 L42 16 L14 16" fill="none" stroke="#6B6B8A" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
          <path d="M14 16 L16 28" fill="none" stroke="#6B6B8A" stroke-width="2" stroke-linecap="round"/>
          <circle cx="20" cy="34" r="3.5" fill="none" stroke="#6B6B8A" stroke-width="2"/>
          <circle cx="34" cy="34" r="3.5" fill="none" stroke="#6B6B8A" stroke-width="2"/>
          <line x1="22" y1="20" x2="21" y2="28" stroke="#6B6B8A44" stroke-width="1" stroke-linecap="round"/>
          <line x1="29" y1="18" x2="27" y2="28" stroke="#6B6B8A44" stroke-width="1" stroke-linecap="round"/>
          <line x1="36" y1="18" x2="33" y2="28" stroke="#6B6B8A44" stroke-width="1" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'caixa'">
          <rect x="10" y="22" width="28" height="18" rx="2" fill="#8B5E3C18" stroke="#8B5E3C" stroke-width="2"/>
          <path d="M10 22 L13 13 L35 13 L38 22 Z" fill="#8B5E3C22" stroke="#8B5E3C" stroke-width="2" stroke-linejoin="round"/>
          <line x1="24" y1="13" x2="24" y2="22" stroke="#8B5E3C" stroke-width="1.5" stroke-linecap="round"/>
          <line x1="24" y1="22" x2="24" y2="40" stroke="#C8A84B" stroke-width="2" stroke-linecap="round"/>
          <line x1="10" y1="30" x2="38" y2="30" stroke="#8B5E3C44" stroke-width="1"/>
        </ng-container>

        <ng-container *ngSwitchCase="'brilho'">
          <path d="M24 6 L25.8 18 L38 18 L28 25 L31.8 37 L24 30 L16.2 37 L20 25 L10 18 L22.2 18 Z" fill="#C8A84B22" stroke="#C8A84B" stroke-width="1.8" stroke-linejoin="round"/>
          <path d="M40 8 L40.8 11.5 L44 12 L40.8 12.5 L40 16 L39.2 12.5 L36 12 L39.2 11.5 Z" fill="#C8A84B55" stroke="#C8A84B" stroke-width="1" stroke-linejoin="round"/>
          <path d="M8 30 L8.8 33 L12 33.5 L8.8 34 L8 37 L7.2 34 L4 33.5 L7.2 33 Z" fill="#C8A84B55" stroke="#C8A84B" stroke-width="1" stroke-linejoin="round"/>
        </ng-container>

        <!-- ══ PACK 5 — ATRIBUTOS ════════════════════════════════════ -->

        <ng-container *ngSwitchCase="'inteligencia'">
          <circle cx="24" cy="24" r="4" fill="#58a6ff44" stroke="#58a6ff" stroke-width="2"/>
          <circle cx="12" cy="14" r="3" fill="#58a6ff22" stroke="#58a6ff" stroke-width="1.8"/>
          <circle cx="36" cy="14" r="3" fill="#58a6ff22" stroke="#58a6ff" stroke-width="1.8"/>
          <circle cx="10" cy="32" r="3" fill="#58a6ff22" stroke="#58a6ff" stroke-width="1.8"/>
          <circle cx="38" cy="32" r="3" fill="#58a6ff22" stroke="#58a6ff" stroke-width="1.8"/>
          <circle cx="24" cy="40" r="3" fill="#58a6ff22" stroke="#58a6ff" stroke-width="1.8"/>
          <line x1="24" y1="20" x2="12" y2="17" stroke="#58a6ff" stroke-width="1.2"/>
          <line x1="24" y1="20" x2="36" y2="17" stroke="#58a6ff" stroke-width="1.2"/>
          <line x1="24" y1="28" x2="10" y2="29" stroke="#58a6ff" stroke-width="1.2"/>
          <line x1="24" y1="28" x2="38" y2="29" stroke="#58a6ff" stroke-width="1.2"/>
          <line x1="24" y1="28" x2="24" y2="37" stroke="#58a6ff" stroke-width="1.2"/>
        </ng-container>

        <ng-container *ngSwitchCase="'sabedoria'">
          <path d="M24 14 Q18 10 8 12 L8 36 Q18 34 24 38 Q30 34 40 36 L40 12 Q30 10 24 14Z" fill="#bc8cff18" stroke="#bc8cff" stroke-width="1.8" stroke-linejoin="round"/>
          <line x1="24" y1="14" x2="24" y2="38" stroke="#bc8cff" stroke-width="1.8" stroke-linecap="round"/>
          <line x1="12" y1="18" x2="21" y2="17" stroke="#bc8cff88" stroke-width="1.2" stroke-linecap="round"/>
          <line x1="12" y1="23" x2="21" y2="22" stroke="#bc8cff88" stroke-width="1.2" stroke-linecap="round"/>
          <line x1="12" y1="28" x2="21" y2="27" stroke="#bc8cff88" stroke-width="1.2" stroke-linecap="round"/>
          <line x1="27" y1="17" x2="36" y2="18" stroke="#bc8cff88" stroke-width="1.2" stroke-linecap="round"/>
          <line x1="27" y1="22" x2="36" y2="23" stroke="#bc8cff88" stroke-width="1.2" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'fisico'">
          <line x1="10" y1="22" x2="38" y2="22" stroke="#3fb950" stroke-width="2" stroke-linecap="round"/>
          <rect x="6" y="16" width="6" height="12" rx="3" fill="#3fb95022" stroke="#3fb950" stroke-width="1.8"/>
          <rect x="36" y="16" width="6" height="12" rx="3" fill="#3fb95022" stroke="#3fb950" stroke-width="1.8"/>
          <circle cx="24" cy="10" r="3.5" fill="#3fb95022" stroke="#3fb950" stroke-width="1.8"/>
          <path d="M24 13 L24 22" stroke="#3fb950" stroke-width="2" stroke-linecap="round"/>
          <path d="M24 22 L16 34 M24 22 L32 34" stroke="#3fb950" stroke-width="1.8" stroke-linecap="round"/>
          <path d="M24 16 L18 22 M24 16 L30 22" stroke="#3fb950" stroke-width="1.8" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'disciplina'">
          <circle cx="24" cy="24" r="14" fill="#f7816618" stroke="#f78166" stroke-width="2"/>
          <circle cx="24" cy="24" r="2" fill="#f78166"/>
          <line x1="24" y1="24" x2="24" y2="13" stroke="#f78166" stroke-width="2" stroke-linecap="round"/>
          <line x1="24" y1="24" x2="31" y2="28" stroke="#f78166" stroke-width="1.5" stroke-linecap="round"/>
          <line x1="24" y1="10" x2="24" y2="12" stroke="#f78166" stroke-width="1.5" stroke-linecap="round"/>
          <line x1="24" y1="36" x2="24" y2="38" stroke="#f78166" stroke-width="1.5" stroke-linecap="round"/>
          <line x1="10" y1="24" x2="12" y2="24" stroke="#f78166" stroke-width="1.5" stroke-linecap="round"/>
          <line x1="36" y1="24" x2="38" y2="24" stroke="#f78166" stroke-width="1.5" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'foco'">
          <circle cx="24" cy="24" r="14" fill="#ffd70018" stroke="#ffd700" stroke-width="2"/>
          <circle cx="24" cy="24" r="8" fill="#ffd70022" stroke="#ffd700" stroke-width="1.8"/>
          <circle cx="24" cy="24" r="3" fill="#ffd700"/>
          <line x1="24" y1="8" x2="24" y2="12" stroke="#ffd700" stroke-width="1.5" stroke-linecap="round"/>
          <line x1="24" y1="36" x2="24" y2="40" stroke="#ffd700" stroke-width="1.5" stroke-linecap="round"/>
          <line x1="8" y1="24" x2="12" y2="24" stroke="#ffd700" stroke-width="1.5" stroke-linecap="round"/>
          <line x1="36" y1="24" x2="40" y2="24" stroke="#ffd700" stroke-width="1.5" stroke-linecap="round"/>
        </ng-container>

        <ng-container *ngSwitchCase="'vitalidade'">
          <path d="M24 36 C24 36 10 27 10 18 C10 13 14 10 18 10 C21 10 23 12 24 13 C25 12 27 10 30 10 C34 10 38 13 38 18 C38 27 24 36 24 36Z" fill="#f8514922" stroke="#f85149" stroke-width="2" stroke-linejoin="round"/>
          <path d="M16 22 L20 18 L23 24 L26 16 L29 22 L32 22" stroke="#f85149" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
        </ng-container>

        <!-- fallback -->
        <ng-container *ngSwitchDefault>
          <circle cx="24" cy="24" r="16" fill="none" stroke="currentColor" stroke-width="2" stroke-dasharray="4 2"/>
          <text x="24" y="28" text-anchor="middle" font-size="10" fill="currentColor" font-family="sans-serif">?</text>
        </ng-container>

      </ng-container>
    </svg>
  `,
})
export class IconComponent {
  @Input() name!: string;
  @Input() size: number = 24;
}
