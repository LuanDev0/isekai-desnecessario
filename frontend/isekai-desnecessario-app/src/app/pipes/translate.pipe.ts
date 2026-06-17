import { ChangeDetectorRef, Pipe, PipeTransform, effect, inject } from '@angular/core';
import { LanguageService } from '../services/language.service';

@Pipe({ name: 'translate', standalone: true, pure: false })
export class TranslatePipe implements PipeTransform {
  private lang = inject(LanguageService);
  private cdr  = inject(ChangeDetectorRef);

  constructor() {
    effect(() => {
      this.lang.current();
      this.cdr.markForCheck();
    });
  }

  transform(key: string, params?: Record<string, string | number>): string {
    return this.lang.translate(key, params);
  }
}
