import { Injectable, signal } from '@angular/core';
import { pt } from '../translations/pt';
import { en } from '../translations/en';

export type LangCode = 'pt' | 'en';

export interface LangOption {
  code: LangCode;
  label: string;
  flag: string;
}

const TRANSLATIONS: Record<LangCode, typeof pt> = { pt, en };

@Injectable({ providedIn: 'root' })
export class LanguageService {
  readonly languages: LangOption[] = [
    { code: 'pt', label: 'Português', flag: '🇧🇷' },
    { code: 'en', label: 'English',   flag: '🇺🇸' },
  ];

  readonly current = signal<LangCode>(
    (localStorage.getItem('lang') as LangCode) ?? 'pt'
  );

  setLanguage(code: LangCode) {
    this.current.set(code);
    localStorage.setItem('lang', code);
  }

  translate(key: string, params?: Record<string, string | number>): string {
    const parts = key.split('.');
    let value: unknown = TRANSLATIONS[this.current()];
    for (const part of parts) {
      value = (value as Record<string, unknown>)?.[part];
    }
    if (typeof value !== 'string') return key;
    if (!params) return value;
    return value.replace(/\{\{(\w+)\}\}/g, (_, k) => String(params[k] ?? ''));
  }
}
