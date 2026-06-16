export type ThemeMode = 'light' | 'dark';

export const THEME_STORAGE_KEY = 'cardvault-theme';

export const COLORS = {
  primary: {
    light: '#16a34a',
    dark:  '#22c55e',
  },
  bg: {
    light: '#ffffff',
    dark:  '#0f172a',
  },
  surface: {
    light: '#f9fafb',
    dark:  '#1e293b',
  },
} as const;
