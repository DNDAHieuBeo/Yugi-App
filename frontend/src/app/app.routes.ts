import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'cards', pathMatch: 'full' },

  // Guest-only routes (redirect to /cards if already logged in)
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register/register.component').then(m => m.RegisterComponent),
  },
  {
    path: 'forgot-password',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent),
  },

  // Protected routes
  {
    path: 'cards',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/cards/card-browser/card-browser.component').then(m => m.CardBrowserComponent),
  },
  {
    path: 'collection',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/collection/my-collection/my-collection.component').then(m => m.MyCollectionComponent),
  },
  {
    path: 'decks',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/decks/deck-list/deck-list.component').then(m => m.DeckListComponent),
  },
  {
    path: 'decks/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/decks/deck-builder/deck-builder.component').then(m => m.DeckBuilderComponent),
  },
  {
    path: 'duel',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/duel/score-board/score-board.component').then(m => m.ScoreBoardComponent),
  },

  { path: '**', redirectTo: 'cards' },
];
