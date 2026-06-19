import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // Guest-only routes
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'forgot-password',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },

  // Protected routes inside App Shell
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./shared/components/app-shell/app-shell.component').then(m => m.AppShellComponent),
    children: [
      { path: '', redirectTo: 'cards', pathMatch: 'full' },
      {
        path: 'cards',
        loadComponent: () => import('./features/cards/card-browser/card-browser.component').then(m => m.CardBrowserComponent)
      },
      {
        path: 'collection',
        loadComponent: () => import('./features/collection/my-collection/my-collection.component').then(m => m.MyCollectionComponent)
      },
      {
        path: 'decks',
        loadComponent: () => import('./features/decks/deck-list/deck-list.component').then(m => m.DeckListComponent)
      },
      {
        path: 'decks/:id',
        loadComponent: () => import('./features/decks/deck-builder/deck-builder.component').then(m => m.DeckBuilderComponent)
      },
      {
        path: 'duel',
        loadComponent: () => import('./features/duel/score-board/score-board.component').then(m => m.ScoreBoardComponent)
      }
    ]
  },

  { path: '**', redirectTo: 'cards' }
];
