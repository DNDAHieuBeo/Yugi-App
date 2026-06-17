# YugiDeck — Frontend Overview

## Summary

The **YugiDeck** frontend is a Single Page Application built with Angular 21, providing the user interface for managing card collections, building competitive decks, and running in-game utilities during Yu-Gi-Oh! matches.

---

## Tech Stack

| Component | Technology | Version |
|---|---|---|
| Framework | Angular (with SSR) | ^21.2.0 |
| Language | TypeScript | ~5.9.2 |
| State Management | Angular Signals (built-in) | 21 |
| Styling | TailwindCSS | ^3.4.19 |
| HTTP Client | Angular HttpClient | 21 |
| Drag & Drop | @angular/cdk | ^21.2.14 |
| SSR Server | @angular/ssr + Express 5 | 21 |
| Testing | Vitest | ^4.0.8 |
| Formatter | Prettier | ^3.4.19 |

---

## Current File Structure

```
frontend/
├── src/
│   ├── app/
│   │   ├── app.ts               <- Root component
│   │   ├── app.html
│   │   ├── app.routes.ts        <- Main routing
│   │   ├── app.config.ts        <- App-level providers (CSR)
│   │   ├── app.config.server.ts <- App-level providers (SSR)
│   │   └── app.routes.server.ts <- SSR routes
│   ├── main.ts                  <- Bootstrap (CSR)
│   ├── main.server.ts           <- Bootstrap (SSR)
│   ├── server.ts                <- Express server for SSR
│   ├── index.html
│   └── styles.scss              <- Global styles + Tailwind imports
├── public/
│   
├── angular.json
├── tailwind.config.js
├── tsconfig.json
└── package.json
```

> Note: The `core/`, `features/`, and `shared/` folders are not yet created — the structure below is the target architecture.

---

## Target Architecture

```
src/app/
├── core/
│   ├── services/
│   │   ├── auth.service.ts
│   │   ├── card.service.ts
│   │   ├── collection.service.ts
│   │   ├── deck.service.ts
│   │   └── duel.service.ts
│   ├── guards/
│   │   └── auth.guard.ts
│   └── interceptors/
│       └── jwt.interceptor.ts
├── features/
│   ├── auth/           <- Login, Register pages
│   ├── cards/          <- Card Browser
│   ├── collection/     <- My Collection
│   ├── decks/          <- Deck List + Deck Builder
│   └── duel/           <- Score Board + Timer + Dice & Coin
└── shared/
    └── components/
        ├── card-thumbnail/
        └── lp-display/
```

---

## Planned Features

| Feature | Description |
|---|---|
| Card Browser | Browse ~12,000 cards synced from YGOPRODeck, filter by type / attribute / race / archetype |
| My Collection | Manage personal card collection, track quantity of each card owned |
| Deck Builder | Create and edit decks with drag-and-drop (CDK), auto-validated against TCG rules |
| Score Board | Track LP for 2 players in real-time (starting at 8000), with full change history |
| Duel Timer | Countdown or stopwatch timer for timed matches |
| Dice & Coin | Roll 1d6 or 2d6, flip a coin — all with result history |

---

## Routing Plan

```typescript
// app.routes.ts
{ path: '',           redirectTo: 'cards', pathMatch: 'full' }
{ path: 'login',      component: LoginComponent }
{ path: 'register',   component: RegisterComponent }

// Protected routes (canActivate: authGuard):
{ path: 'cards',      loadComponent: ... }   // Card Browser
{ path: 'collection', loadComponent: ... }   // My Collection
{ path: 'decks',      loadComponent: ... }   // Deck List
{ path: 'decks/:id',  loadComponent: ... }   // Deck Builder
{ path: 'duel',       loadComponent: ... }   // Score Board
```

All feature components are **lazy-loaded** to minimize the initial bundle size.

---

## Authentication Strategy

- **Access token** stored in memory only (not localStorage — more secure against XSS)
- **Refresh token** stored in `localStorage` (auto-renewed when access token expires)
- `jwt.interceptor.ts` automatically attaches `Authorization: Bearer {token}` to every request
- `auth.guard.ts` protects all routes that require a logged-in user

---

## State Management

Uses **Angular Signals** (built-in since Angular 16) for reactive local state — no NgRx or RxJS BehaviorSubject needed for simple cases.

```typescript
// Example: DuelService
player1LP = signal(8000);
player2LP = signal(8000);

updateLP(player: 1 | 2, delta: number) {
  if (player === 1) this.player1LP.update(v => Math.max(0, v + delta));
  else              this.player2LP.update(v => Math.max(0, v + delta));
}
```

---

## Backend Connection

```typescript
// environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5252/api'
};
```

---

## Development Progress

### Completed
- [x] Angular 21 project initialized with SSR
- [x] TailwindCSS configured
- [x] Angular CDK installed
- [x] Prettier configured
- [x] Vitest for unit testing
- [x] Root app files (app.ts, app.routes.ts, app.config.ts)

### Pending
- [ ] Core services, guards, interceptors
- [ ] Auth feature (login / register pages)
- [ ] Card browser with filtering and pagination
- [ ] Deck builder with drag-and-drop
- [ ] Score board
- [ ] Duel timer + dice & coin roller
- [ ] Shared components (CardThumbnail, LPDisplay)
- [ ] Full routing configuration

---

## Common Commands

```bash
# Install dependencies
npm install

# Start dev server (http://localhost:4200)
npm start

# Production build
npm run build

# Run SSR server
npm run serve:ssr:frontend

# Run unit tests
npm test

# Dev build with file watching
npm run watch
```

---

*Last updated: June 2026*
