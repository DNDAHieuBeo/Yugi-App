# CardVault

> A full-stack web application for Yu-Gi-Oh! players to manage their card collection, build competitive decks, and run in-game utilities during matches.

---

## Features

| Feature | Description |
|---|---|
| **Card Manager** | Browse all ~12,000 Yu-Gi-Oh! cards synced from YGOPRODeck, filter by type, attribute, race, and archetype |
| **My Collection** | Track how many copies of each card you own with quantity management |
| **Deck Builder** | Create and edit decks with drag-and-drop, auto-validated against official TCG rules and banlist |
| **Score Board** | Track Life Points for 2 players in real-time, starting at 8000 LP with full change history |
| **Duel Timer** | Countdown or stopwatch timer for timed matches |
| **Dice & Coin** | Roll 1d6 or 2d6, flip a coin - all with result history |

---

## Architecture

CardVault is a **monorepo** containing a separate backend and frontend application.

```
CardVault/
├── backend/          <- ASP.NET Core Web API (.NET 10) - Clean Architecture
│   ├── YugiDeck.API/
│   ├── YugiDeck.Core/
│   └── YugiDeck.Infrastructure/
└── frontend/         <- Angular 21 SPA with SSR
    └── src/
```

### Backend - Clean Architecture

```
YugiDeck.Core           <- Domain layer: Entities, DTOs, Interfaces (no external deps)
    ^
YugiDeck.Infrastructure <- Data layer: EF Core, SQLite, ASP.NET Identity, HttpClient
    ^
YugiDeck.API            <- Presentation layer: Controllers, JWT Auth, AutoMapper, Swagger
```

### Frontend - Angular 21

```
src/app/
├── core/              <- Services, Guards, JWT Interceptor
├── features/          <- Auth, Cards, Collection, Decks, Duel (lazy-loaded)
└── shared/            <- Reusable components (CardThumbnail, LPDisplay)
```

---

## Tech Stack

### Backend

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core Web API (.NET 10) |
| Database | SQLite (dev) / PostgreSQL (prod) via EF Core |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Object Mapping | AutoMapper |
| API Docs | Swagger / OpenAPI |
| External Data | YGOPRODeck API (https://db.ygoprodeck.com/api/v7/) |

### Frontend

| Layer | Technology |
|---|---|
| Framework | Angular 21 (Standalone Components + SSR) |
| Language | TypeScript 5.9 |
| State | Angular Signals |
| Styling | TailwindCSS 3.x |
| Drag & Drop | @angular/cdk/drag-drop |
| Testing | Vitest |

---

## Data Model

```
AspNetUsers (Identity)
    |
    |--< UserCard >-- Card          (collection: how many copies owned)
    |--< Deck    >-- DeckCard >-- Card  (deck builder)
    |--< Duel    >-- LPLog          (duel sessions + LP history)
```

### Key Entities

| Entity | Description |
|---|---|
| `Card` | ~12,000 cards synced from YGOPRODeck. Read-only for users. |
| `UserCard` | Links a user to a card with a quantity counter. |
| `Deck` | A named deck belonging to a user, split into main / extra / side sections. |
| `DeckCard` | A single card entry inside a deck section. |
| `Duel` | A duel session between two players, tracking LP and status. |
| `LPLog` | Immutable log of every LP change (delta + new value + timestamp). |

---

## API Overview

Base URL (development): `https://localhost:7001/api`

All endpoints except `/auth` require `Authorization: Bearer <token>`.

| Method | Endpoint | Description |
|---|---|---|
| POST | `/auth/register` | Create a new account |
| POST | `/auth/login` | Login and receive JWT + refresh token |
| POST | `/auth/refresh` | Renew access token |
| GET | `/cards` | Search and filter all cards (paginated) |
| POST | `/cards/sync` | Trigger full sync from YGOPRODeck |
| GET | `/collection` | Get your card collection |
| POST | `/collection` | Add a card to your collection |
| PATCH | `/collection/{cardId}/quantity` | Update card quantity |
| GET | `/decks` | List all your decks |
| GET | `/decks/{id}` | Get a deck with all cards by section |
| PUT | `/decks/{id}` | Save / overwrite a full deck |
| GET | `/decks/{id}/validate` | Validate deck against TCG rules + banlist |
| POST | `/duels` | Start a new duel session |
| POST | `/duels/{id}/lp` | Apply LP damage or heal |
| GET | `/duels/{id}/history` | Get full LP change log |

---

## Getting Started

### Prerequisites

- .NET 10 SDK (https://dotnet.microsoft.com/download)
- Node.js 20+ and npm 11+
- Angular CLI (`npm install -g @angular/cli`)
- EF Core CLI (`dotnet tool install --global dotnet-ef`)

---

### Backend

```bash
# Navigate to backend
cd backend

# Restore packages
dotnet restore YugiDeck.slnx

# Apply database migrations
dotnet ef database update --project YugiDeck.Infrastructure --startup-project YugiDeck.API

# Run the API
dotnet run --project YugiDeck.API
```

API will be available at:
- HTTP:  http://localhost:5252
- HTTPS: https://localhost:7047
- Swagger UI: https://localhost:7047/swagger

#### appsettings.json (backend/YugiDeck.API/appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=yugideck.db"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-minimum-32-characters-long",
    "Issuer": "CardVaultAPI",
    "Audience": "CardVaultClient",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "YgoApi": {
    "BaseUrl": "https://db.ygoprodeck.com/api/v7/",
    "RateLimitPerSecond": 20
  }
}
```

---

### Frontend

```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Start dev server
npm start
```

App will be available at `http://localhost:4200`.

#### environment.ts (frontend/src/environments/environment.ts)

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7001/api'
};
```

---

## Development Workflow

### Backend - EF Core Migrations

```bash
# Create a new migration (run from backend/)
dotnet ef migrations add <MigrationName> \
  --project YugiDeck.Infrastructure \
  --startup-project YugiDeck.API

# Apply to database
dotnet ef database update \
  --project YugiDeck.Infrastructure \
  --startup-project YugiDeck.API

# Roll back last migration
dotnet ef migrations remove \
  --project YugiDeck.Infrastructure \
  --startup-project YugiDeck.API
```

### Frontend - Useful Commands

```bash
npm run build               # Production build
npm run watch               # Dev build with file watching
npm test                    # Run unit tests (Vitest)
npm run serve:ssr:frontend  # Run SSR production server
```

---

## Business Rules

### Deck Validation (enforced server-side)

| Rule | Constraint |
|---|---|
| Main deck size | 40 - 60 cards |
| Extra deck size | Max 15 cards (Fusion / Synchro / XYZ / Link only) |
| Side deck size | Max 15 cards |
| Copies per card | Max 3 across main + side |
| Banned cards (TCG) | 0 copies allowed |
| Limited cards (TCG) | Max 1 copy |
| Semi-Limited (TCG) | Max 2 copies |

### Life Points

| Rule | Value |
|---|---|
| Starting LP | 8000 per player |
| Minimum LP | 0 (cannot go negative) |
| Win condition | Opponent LP reaches 0 |

---

## Development Progress

### Backend

- [x] Solution with 3 projects (API, Core, Infrastructure)
- [x] All domain entities defined
- [x] AppDbContext with ASP.NET Identity
- [x] EF Core migration applied (SQLite DB created)
- [x] JWT Auth + Identity wired in Program.cs
- [x] AuthController - register / login / refresh / revoke
- [x] CardsController + YGOPRODeck sync service
- [x] CollectionController
- [x] DecksController + validation logic
- [x] DuelsController + LP tracking
- [x] Global exception handler middleware
- [x] Swagger with JWT Authorization button

### Frontend

- [x] Angular 21 project with SSR
- [x] TailwindCSS configured
- [x] Angular CDK installed
- [ ] Core services, guards, interceptors
- [ ] Auth feature (login / register)
- [ ] Card browser
- [ ] Deck builder with drag-and-drop
- [ ] Score board
- [ ] Duel timer + dice & coin roller

---

## License

This project is for personal and educational use.
Yu-Gi-Oh! is a trademark of Kazuki Takahashi / Konami.
Card data is provided by YGOPRODeck under their free API terms.

---

Built with love for the Yu-Gi-Oh! community