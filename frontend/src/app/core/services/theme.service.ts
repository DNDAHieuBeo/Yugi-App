import { Injectable, signal, effect, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { THEME_STORAGE_KEY, ThemeMode } from '../constants/theme.constants';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  readonly mode = signal<ThemeMode>(this.getInitialTheme());

  readonly isDark = () => this.mode() === 'dark';

  constructor() {
    effect(() => this.applyTheme(this.mode()));
  }

  toggle(): void {
    this.mode.update(m => (m === 'light' ? 'dark' : 'light'));
  }

  setTheme(mode: ThemeMode): void {
    this.mode.set(mode);
  }

  private getInitialTheme(): ThemeMode {
    if (!this.isBrowser) return 'dark';

    const stored = localStorage.getItem(THEME_STORAGE_KEY) as ThemeMode | null;
    if (stored === 'light' || stored === 'dark') return stored;

    return 'dark';
  }

  private applyTheme(mode: ThemeMode): void {
    if (!this.isBrowser) return;

    const html = document.documentElement;
    if (mode === 'dark') {
      html.classList.add('dark');
    } else {
      html.classList.remove('dark');
    }
    localStorage.setItem(THEME_STORAGE_KEY, mode);
  }
}
