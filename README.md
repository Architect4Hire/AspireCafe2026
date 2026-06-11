# ☕ AspireCafe — Cafe POS POC

A point-of-sale proof-of-concept showcasing **.NET 10 + Aspire** microservices behind a **modern Angular 20** POS UI.

## Architecture at a glance

```
┌─────────────────────────────────────────────────────────────────┐
│                  AspireCafe.AppHost (Aspire 9)                  │
│  Orchestrates SQL Server container + 3 APIs + Angular POS       │
└─────────────────────────────────────────────────────────────────┘
                                  │
        ┌─────────────────────────┼─────────────────────────┐
        ▼                         ▼                         ▼
┌────────────────┐       ┌────────────────┐       ┌────────────────┐
│  Menu.API      │       │  Orders.API    │       │ Payments.API   │
│  (MenuDb)      │       │  (OrdersDb)    │       │  (PaymentsDb)  │
└────────────────┘       └────────────────┘       └────────────────┘
        ▲                         ▲                         ▲
        └─────────────────────────┼─────────────────────────┘
                                  │
                       ┌──────────────────────┐
                       │  Angular 20 POS UI   │
                       └──────────────────────┘
```

Each microservice uses the **onion-style layered architecture** you specified:

```
Controller  →  Facade  →  Business  →  Data  →  EF Core DbContext
                                                         │
                                                    SQL Server
```

Inside each service's `Managers/` folder you'll find:
- `DataContext/` — EF Core DbContext + entity config + seeding
- `Domain/` — internal domain models
- `ViewModels/` — incoming models (request DTOs)
- `ServiceModels/` — outgoing models (response DTOs)
- `Extensions/` — hand-rolled mapping between the three model types
- `Data/`, `Business/`, `Facades/` — the layered logic

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | **10.0.100** or newer (pinned via `global.json`) |
| Node.js | 22 LTS |
| Docker | Required — Aspire spins up SQL Server in a container |
| Visual Studio 2026 or VS Code | Either works |

## Run it

```bash
# From the repo root:
dotnet run --project src/AspireCafe.AppHost
```

Aspire will:
1. Start a SQL Server container with persistent volume `aspirecafe-sql-data`
2. Create the three databases (`MenuDb`, `OrdersDb`, `PaymentsDb`)
3. Run the three APIs and seed the Menu DB with 10 starter items
4. Run `npm install` + `npm start` for the Angular app on port 4200

Open the Aspire dashboard URL printed to the console (defaults to `https://localhost:17100`), then click `pos-web` to launch the POS UI.

### Override the dev SQL password (optional)

A dev-only fallback SQL password lives in `appsettings.Development.json` for first-run convenience. To override per developer, set a user secret:

```bash
dotnet user-secrets set "Parameters:sql-password" "<your-strong-password>" \
  --project src/AspireCafe.AppHost
```

User secrets take precedence over `appsettings.Development.json`. Aspire 9.4 will also prompt in the dashboard before starting SQL if neither is set.

## Key user flows

1. **Menu Review & Ordering** — `/menu` — Browse, filter by category, add items to the order, set table number.
2. **Payment Processing with Auto Tip** — `/payment` — Pick 15/18/20/25% or enter a custom tip. Submit the order, then process payment. Receipt with auth code.
3. **Order Routing by Table** — `/orders` — Kitchen view of active orders, sorted oldest first. Advance through Submitted → Preparing → Ready → Delivered.

## API endpoints

| Service | Endpoint | Purpose |
|---------|----------|---------|
| Menu | `GET /api/Menu` | List all menu items |
| Menu | `GET /api/Menu/category/{category}` | Filter by category |
| Orders | `POST /api/Orders` | Submit order (routes to table) |
| Orders | `GET /api/Orders/active` | Active kitchen queue |
| Orders | `PATCH /api/Orders/{id}/status` | Advance status |
| Payments | `GET /api/Payments/tip-suggestions?subtotal=X` | Suggested tip amounts |
| Payments | `POST /api/Payments` | Process payment |

## Project layout

```
AspireCafe/
├── AspireCafe.sln
├── global.json
├── src/
│   ├── AspireCafe.AppHost/          ← Aspire orchestrator
│   ├── AspireCafe.ServiceDefaults/  ← Shared OTel + health + discovery
│   ├── AspireCafe.Menu.API/         ← Menu microservice (onion)
│   ├── AspireCafe.Orders.API/       ← Orders microservice (onion)
│   ├── AspireCafe.Payments.API/     ← Payments microservice (onion)
│   └── AspireCafe.POS/              ← Angular 20 POS UI
└── docs/
    ├── SCRUB_PROMPTS.md             ← All SCRUB prompts to recreate the POC
    ├── NEXT_STEPS.md                ← Roadmap and TODOs
    └── AspireCafe_Conference_Session.pptx
```

See `docs/SCRUB_PROMPTS.md` for the full prompt library used to build this POC.
