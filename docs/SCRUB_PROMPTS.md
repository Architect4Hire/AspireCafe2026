# AspireCafe — SCRUB Prompt Library

> A complete, ordered set of SCRUB-style prompts to recreate the AspireCafe POC from a blank repo, end-to-end, in a conference session.

## How to use this library

Each prompt follows the **SCRUB framework** from the *SCRUB & Prompt Engineering Deep Dive* deck:

- **[S] Scope** — name the exact deliverable and methods. If it takes more than three sentences, split it.
- **[C] Constraints** — framework, architecture, naming, DI, testing. Match the codebase.
- **[R] Restrictions** — `Do NOT…` rules. Tiered: **CRITICAL / IMPORTANT / PREFERRED**.
- **[U] Usage** — who calls this, what the surrounding system is, compliance and audit context.
- **[B] Behavior** — what must NOT change (dominant element in Edit Mode).

Run the prompts in order. After each one, **verify against the original `[R]` restrictions** before continuing — this is the Verify step in the chain.

### Recommended tool mode per prompt

- **Agent Mode** (Copilot Agent or Claude Code) for prompts marked 🤖 — multi-file scaffolding.
- **Chat** for prompts marked 💬 — single-file or analytical work.
- **Edit Mode** for prompts marked ✏️ — narrow, surgical edits where `[B]` dominates.

---

## Phase 0 — Pre-load context (CLAUDE.md / copilot-instructions.md)

Before writing the first SCRUB prompt, drop this file at the repo root as `CLAUDE.md` (or `.github/copilot-instructions.md`). It pre-loads `C`, `U`, and most `R` so every later prompt can stay short.

```markdown
# AspireCafe Conventions (pre-loaded into every prompt)

## System (U)
AspireCafe is a cafe point-of-sale POC. It demonstrates .NET 10 + Aspire microservices
behind an Angular 20 frontend. Each domain is its own microservice with its own SQL
Server database. This is a POC — no real PII, no real payment processor.

## Architecture (C)
- .NET 10, ASP.NET Core 10, C# 14, nullable + implicit usings on
- Aspire 9.4 for orchestration; SQL Server in a container with persistent volume
- Onion architecture per service: Controller → Facade → Business → Data → DbContext
- Each service has a `Managers/` folder containing: DataContext/, Domain/, ViewModels/,
  ServiceModels/, Extensions/, Data/, Business/, Facades/
- Hand-rolled mapping extensions (no AutoMapper)
- EF Core 9, code-first, EnsureCreatedAsync for the POC (migrations later)
- Angular 20 standalone components, signals, no NgModules
- Dark cafe theme: caramel accent (#d4a574) on espresso bg (#1a1410)

## Naming (C)
- Interfaces prefixed `I` (e.g. `IMenuFacade`)
- DbContext suffix on EF contexts (`MenuDbContext`)
- ViewModel = incoming, ServiceModel = outgoing, Domain = internal
- Extension methods grouped by entity (e.g. `MenuItemMappingExtensions`)

## Restrictions — apply to ALL prompts (R)
CRITICAL — Do NOT:
1. Use AutoMapper, Mapster, or any reflection-based mapper. Hand-rolled extensions only.
2. Put EF Core types or DbContext into the Controller, Facade, or Business layer.
3. Return Domain entities from controllers — always map to ServiceModels.
4. Accept Domain entities into controllers — always take ViewModels.
5. Hard-code connection strings — use Aspire's `AddSqlServerDbContext("DbName")`.
6. Reference one microservice's project from another. They are independent.

IMPORTANT — Do NOT:
7. Use synchronous EF calls. Everything async with CancellationToken.
8. Use `decimal` math without explicit rounding to 2 dp at boundaries.
9. Skip XML doc-comments on public Facade or Controller methods.

PREFERRED — Do NOT:
10. Introduce magic numbers — promote to `const` or appsettings.
11. Mix concerns in a single class — split if the file exceeds ~120 lines.
```

> With this loaded, every prompt below stays focused on `S`, `R`-deltas, and `B`. This is the **"Custom Instructions = Shorter, Safer Prompts"** pattern from slide 30.

---

# PART A — Solution Scaffold & Aspire AppHost

## Prompt A1 — Create the solution and `global.json` 💬

```text
[S] Create an empty .NET solution named `AspireCafe.sln` and a `global.json` at
    the repo root pinning the SDK to 10.0.100 with rollForward latestFeature.

[C] .NET 10 LTS. No projects yet — just the .sln and global.json.

[R] Do NOT include any projects in the solution yet. We add them in later prompts.
    Do NOT add a Directory.Build.props yet.

[U] First step of a from-scratch POC build. Will be followed by per-project prompts.

[B] N/A — greenfield.
```

## Prompt A2 — Create the Aspire AppHost project 🤖

```text
[S] Create `src/AspireCafe.AppHost/AspireCafe.AppHost.csproj`, `Program.cs`,
    `Properties/launchSettings.json`, and `appsettings.Development.json`
    that orchestrate: one SQL Server container with persistent volume, three
    databases (MenuDb, OrdersDb, PaymentsDb), three not-yet-existing API
    projects referenced as Projects.AspireCafe_Menu_API, Projects.AspireCafe_Orders_API,
    Projects.AspireCafe_Payments_API, and one Angular npm app at
    `../AspireCafe.POS` on port 4200.

[C] - Use `<Sdk Name="Aspire.AppHost.Sdk" Version="9.4.0" />` as an inline SDK
      element inside the .csproj (DO NOT use `<IsAspireHost>true</IsAspireHost>`
      — that property was deprecated in Aspire 9.2 and triggers NETSDK1206).
    - Target `net10.0`
    - PackageReferences: Aspire.Hosting.AppHost 9.4.0, Aspire.Hosting.SqlServer
      9.4.0, Aspire.Hosting.NodeJs 9.4.0
    - SQL password via `builder.AddParameter("sql-password", secret: true)`
    - SQL Server: `.WithDataVolume("aspirecafe-sql-data").WithLifetime(ContainerLifetime.Persistent)`
    - Each API: `.WithReference(db).WaitFor(db).WithExternalHttpEndpoints()`
    - Angular: `.AddNpmApp("pos-web", "../AspireCafe.POS", "start")
      .WithHttpEndpoint(port: 4200, env: "PORT")`
      with environment variables `API_MENU`, `API_ORDERS`, `API_PAYMENTS` pointing
      to each API's https endpoint, then `.PublishAsDockerFile()`
    - `Properties/launchSettings.json` MUST exist with two profiles (http, https).
      Each profile sets `applicationUrl` (the dashboard URL), plus environment
      variables `DOTNET_DASHBOARD_OTLP_ENDPOINT_URL`,
      `DOTNET_DASHBOARD_OTLP_HTTP_ENDPOINT_URL`, and
      `DOTNET_RESOURCE_SERVICE_ENDPOINT_URL`. Use distinct ports across both
      profiles. Without this file, `dotnet run` returns immediately with no
      dashboard, no resources, no errors — a silent no-op.
    - `appsettings.Development.json` may include a `Parameters:sql-password`
      fallback so first-time `dotnet run` succeeds without requiring a manual
      `dotnet user-secrets set` (Aspire 9.4 will otherwise prompt in the
      dashboard before starting SQL).

[R] CRITICAL — Do NOT:
    1. Hard-code the SQL password — use AddParameter with secret:true.
       (A dev-only fallback in appsettings.Development.json is acceptable
       but the parameter declaration in code stays `secret: true`.)
    2. Bake API URLs into Angular — pass them via environment variables.
    3. Use `<IsAspireHost>true</IsAspireHost>` in the csproj. That property
       was tied to the old Aspire workload (deprecated in 9.2). Use the
       `<Sdk Name="Aspire.AppHost.Sdk" Version="9.4.0" />` element instead.
    4. Skip `Properties/launchSettings.json`. The AppHost is an ASP.NET Core
       Exe — without launch profiles it has no URL to bind to and `dotnet run`
       silently does nothing (no dashboard, no DCP, no resources started).
    IMPORTANT — Do NOT:
    5. Add ProjectReferences yet to the three APIs in the csproj — leave them as
       TODO comments; we create the projects in a later prompt and then come back.
    PREFERRED — Do NOT:
    6. Add OpenTelemetry collector, Redis, RabbitMQ, or other resources we don't
       need yet.

[U] This is the orchestrator a developer will run with `dotnet run`. The Aspire
    dashboard becomes the front door to logs, traces, and resource health.

[B] N/A — new file.
```

## Prompt A3 — Create the ServiceDefaults project 🤖

```text
[S] Create `src/AspireCafe.ServiceDefaults/AspireCafe.ServiceDefaults.csproj`
    and `Extensions.cs` exposing two extension methods on `IHostApplicationBuilder`:
    `AddServiceDefaults()` and on `WebApplication`: `MapDefaultEndpoints()`.
    Inside `AddServiceDefaults`, wire up: OpenTelemetry (metrics + tracing +
    logging), default health checks (liveness tag "live"), service discovery,
    and standard HTTP client resilience.

[C] - `IsAspireSharedProject=true`, net10.0
    - PackageReferences: Microsoft.Extensions.ServiceDiscovery 9.4.0,
      Microsoft.Extensions.Http.Resilience 9.0.0,
      OpenTelemetry.Exporter.OpenTelemetryProtocol 1.10.0,
      OpenTelemetry.Extensions.Hosting 1.10.0,
      OpenTelemetry.Instrumentation.AspNetCore 1.10.0,
      OpenTelemetry.Instrumentation.Http 1.10.0,
      OpenTelemetry.Instrumentation.Runtime 1.10.0
    - FrameworkReference Microsoft.AspNetCore.App
    - Namespace: `Microsoft.Extensions.Hosting`

[R] Do NOT:
    1. Add an OTLP exporter unless OTEL_EXPORTER_OTLP_ENDPOINT is set in config.
    2. Expose health endpoints in non-Development environments.

[U] Imported by every API in the solution so they share telemetry, health, and
    HTTP-client policies. Keeps APIs free of boilerplate.

[B] N/A.
```

---

# PART B — Menu microservice (the template)

> Once Menu is complete, Orders and Payments follow the same pattern (with their
> own restrictions). This is the **layered SCRUB** pattern from slide 23 — build
> the interface, then the implementation, then tests.

## Prompt B1 — Menu API csproj + folder skeleton 🤖

```text
[S] Create `src/AspireCafe.Menu.API/AspireCafe.Menu.API.csproj` as a
    Microsoft.NET.Sdk.Web project, plus the empty `Managers/` folder tree:
    DataContext/, Domain/, ViewModels/, ServiceModels/, Extensions/, Data/,
    Business/, Facades/, and a top-level Controllers/ folder.

[C] - Target net10.0, nullable+ImplicitUsings on, RootNamespace AspireCafe.Menu.API
    - PackageReferences: Aspire.Microsoft.EntityFrameworkCore.SqlServer 9.4.0,
      Microsoft.EntityFrameworkCore.Design 9.0.0 (PrivateAssets all),
      Swashbuckle.AspNetCore 7.2.0
    - ProjectReference to ../AspireCafe.ServiceDefaults

[R] CRITICAL — Do NOT:
    1. Add a Domain folder anywhere outside `Managers/Domain/`. There is exactly
       ONE Domain folder per microservice, inside Managers/.
    IMPORTANT — Do NOT:
    2. Create any code files yet — only the csproj and empty directories.

[U] Skeleton step before we generate domain, models, layers, and controller.

[B] N/A.
```

## Prompt B2 — MenuItem Domain model 💬

```text
[S] Create `Managers/Domain/MenuItem.cs` with class `MenuItem` containing:
    Id (Guid), Name (string), Description (string), Price (decimal),
    Category (string), ImageUrl (string), IsAvailable (bool),
    PrepTimeMinutes (int), CreatedUtc (DateTime), UpdatedUtc (DateTime).

[C] - Namespace `AspireCafe.Menu.API.Managers.Domain`
    - All string properties initialized to `string.Empty`
    - XML doc-comment on the class explaining it's the internal domain model

[R] Do NOT:
    1. Add EF Core attributes ([Key], [Required], etc.). Configuration belongs
       in OnModelCreating.
    2. Add validation attributes. Those live on ViewModels, never Domain.
    3. Add navigation properties — Menu has no aggregates in this POC.

[U] Internal model used by Data and Business layers. Never exposed to controllers.

[B] N/A.
```

## Prompt B3 — MenuItemViewModel (incoming) 💬

```text
[S] Create `Managers/ViewModels/MenuItemViewModel.cs` with DataAnnotations
    matching the fields of MenuItem EXCEPT Id, CreatedUtc, UpdatedUtc.

[C] - Required + StringLength(120) on Name
    - Range(0.0, 9999.99) on Price
    - Required + StringLength(50) on Category
    - StringLength(500) on Description and ImageUrl
    - Range(0, 240) on PrepTimeMinutes
    - IsAvailable defaults to true

[R] CRITICAL — Do NOT:
    1. Include Id, CreatedUtc, or UpdatedUtc — those are server-controlled.
    IMPORTANT — Do NOT:
    2. Reference the Domain model. ViewModel and Domain are independent.

[U] Bound from incoming HTTP requests via [FromBody].

[B] N/A.
```

## Prompt B4 — MenuItemServiceModel (outgoing) 💬

```text
[S] Create `Managers/ServiceModels/MenuItemServiceModel.cs` containing all the
    public fields a client should see: Id, Name, Description, Price, Category,
    ImageUrl, IsAvailable, PrepTimeMinutes.

[R] Do NOT:
    1. Expose CreatedUtc or UpdatedUtc to clients — internal bookkeeping.
    2. Add validation attributes — this is outbound only.

[U] Returned to API consumers. Decoupled from Domain so internal property
    changes never break clients.

[B] N/A.
```

## Prompt B5 — Mapping extensions 💬

```text
[S] Create `Managers/Extensions/MenuItemMappingExtensions.cs` with these
    static extension methods:
    - ToDomain(this MenuItemViewModel) → MenuItem  (new Guid, UTC timestamps)
    - ApplyUpdate(this MenuItem, MenuItemViewModel)  (mutates, updates UpdatedUtc)
    - ToServiceModel(this MenuItem) → MenuItemServiceModel
    - ToServiceModels(this IEnumerable<MenuItem>) → IEnumerable<MenuItemServiceModel>

[C] - All string copies use `.Trim()`
    - All decimal values rounded to 2 dp via `decimal.Round(x, 2)`
    - Timestamps from `DateTime.UtcNow`

[R] CRITICAL — Do NOT:
    1. Use AutoMapper / Mapster / reflection. Hand-rolled property assignments.
    2. Mutate the input ViewModel in any method.
    IMPORTANT — Do NOT:
    3. Skip the `.Trim()` on string copies — we never want trailing whitespace
       in our DB.

[U] Used in the Business layer to convert between the three model types.
    Hand-rolled mapping makes diffs reviewable.

[B] N/A.
```

## Prompt B6 — MenuDbContext + seed 🤖

```text
[S] Create `Managers/DataContext/MenuDbContext.cs` with `DbSet<MenuItem>
    MenuItems` and a static `SeedAsync(MenuDbContext db, CancellationToken ct)`
    that calls EnsureCreatedAsync and inserts 10 menu items across the
    categories Coffee, Tea, Pastry, Food.

[C] - Use primary constructor: `MenuDbContext(DbContextOptions<MenuDbContext>) : DbContext`
    - In OnModelCreating, configure:
      - ToTable("MenuItems"), HasKey Id
      - Name Required, MaxLength 120
      - Description MaxLength 500
      - Category Required, MaxLength 50
      - ImageUrl MaxLength 500
      - Price HasPrecision(8, 2)
      - HasIndex on Category
    - Seed at minimum: Espresso $3.50, Cappuccino $4.75, Latte $5.25,
      Cold Brew $4.50 (Coffee); Matcha Latte $5.75, Chai Latte $5.25 (Tea);
      Croissant $3.95, Blueberry Muffin $3.25 (Pastry); Avocado Toast $9.50,
      Caprese Panini $11.25 (Food)

[R] CRITICAL — Do NOT:
    1. Add Migrations folder — this POC uses EnsureCreatedAsync.
    2. Reference other microservices' contexts (OrdersDb, PaymentsDb).
    IMPORTANT — Do NOT:
    3. Skip the `if (await db.MenuItems.AnyAsync(ct)) return;` idempotency check
       in SeedAsync.

[U] Called once at API startup; data lives in the persistent SQL Server volume
    so seeded items survive container restarts.

[B] N/A.
```

## Prompt B7 — Data layer (repository) 🤖

```text
[S] Create `Managers/Data/MenuDataManager.cs` containing interface
    `IMenuDataManager` and implementation `MenuDataManager(MenuDbContext db)`.
    Methods: GetAllAsync, GetByCategoryAsync(string), GetByIdAsync(Guid),
    AddAsync(MenuItem), UpdateAsync(MenuItem), DeleteAsync(Guid). All take a
    `CancellationToken`. All async. AsNoTracking on reads.

[R] CRITICAL — Do NOT:
    1. Put any business rules in this layer. It only talks to EF.
    2. Throw domain-level exceptions. Return null/false for "not found".
    IMPORTANT — Do NOT:
    3. Eager-load anything (no .Include) — MenuItem has no related entities.
    4. Use synchronous EF methods (no .ToList(), only .ToListAsync()).

[U] Called only by the Business layer.

[B] N/A.
```

## Prompt B8 — Business layer 🤖

```text
[S] Create `Managers/Business/MenuBusinessManager.cs` with `IMenuBusinessManager`
    and impl `MenuBusinessManager(IMenuDataManager data)`. Methods mirror the
    data manager but accept/return ViewModel + ServiceModel types and call the
    mapping extensions.

[C] - GetAllAsync / GetByCategoryAsync / GetByIdAsync return ServiceModels
    - CreateAsync(MenuItemViewModel) → maps to Domain, calls data.AddAsync,
      returns ServiceModel
    - UpdateAsync(Guid, MenuItemViewModel) → loads, ApplyUpdate, saves, returns
      ServiceModel or null if not found
    - DeleteAsync(Guid) → delegates to data

[R] CRITICAL — Do NOT:
    1. Reference EF Core or DbContext from this file. Only the IMenuDataManager.
    2. Return Domain entities from any method.

[U] Called only by the Facade. Houses the business rules (none for Menu yet).

[B] N/A.
```

## Prompt B9 — Facade 🤖

```text
[S] Create `Managers/Facades/MenuFacade.cs` with `IMenuFacade` and impl
    `MenuFacade(IMenuBusinessManager business)`. Methods named for the consumer:
    GetMenuAsync, GetByCategoryAsync, GetItemAsync, AddItemAsync, UpdateItemAsync,
    RemoveItemAsync. Each one-liner delegates to business.

[R] Do NOT:
    1. Inject the Data layer here — Facade talks to Business only.
    2. Add logic — Facade is a stable surface. Composition belongs in Business.

[U] The ONLY thing controllers depend on. If we later split the business layer
    into multiple managers, the facade keeps the controller stable.

[B] N/A.
```

## Prompt B10 — Controller 🤖

```text
[S] Create `Controllers/MenuController.cs` exposing:
    GET    /api/Menu                  → list all
    GET    /api/Menu/category/{cat}   → by category
    GET    /api/Menu/{id:guid}        → by id (404 if missing)
    POST   /api/Menu                  → create (201 CreatedAtAction)
    PUT    /api/Menu/{id:guid}        → update (404 if missing)
    DELETE /api/Menu/{id:guid}        → 204 / 404

[C] - [ApiController], [Route("api/[controller]")], [Produces("application/json")]
    - Inject IMenuFacade via primary constructor
    - Use ProducesResponseType attributes for swagger docs

[R] CRITICAL — Do NOT:
    1. Inject DbContext, Data, or Business managers — only IMenuFacade.
    2. Return Domain entities anywhere — only MenuItemServiceModel.

[U] Called by the Angular POS over CORS. Will also appear in Swagger UI.

[B] N/A.
```

## Prompt B11 — Program.cs wiring 🤖

```text
[S] Create `Program.cs` for the Menu API. Wire up: AddServiceDefaults,
    AddSqlServerDbContext<MenuDbContext>("MenuDb"), DI for Data → Business →
    Facade (Scoped), Controllers, Swagger, CORS policy named "PosCors" allowing
    localhost:4200 (http+https) with any header/method. After build: seed via
    `MenuDbContext.SeedAsync`. In Development, enable Swagger UI.

[R] CRITICAL — Do NOT:
    1. Allow CORS from `*` — only the POS origins.
    2. Skip the seeding step or move it to a hosted service for this POC.
    IMPORTANT — Do NOT:
    3. Expose Swagger in non-Development.

[U] Entrypoint Aspire executes. The "MenuDb" connection name MUST match the
    name registered in the AppHost (`sqlServer.AddDatabase("MenuDb")`).

[B] N/A.
```

---

# PART C — Orders microservice

Same architecture, different domain. Below are the **deltas** that differ from Menu.

## Prompt C1 — Order domain (Order + OrderItem + OrderStatus enum) 💬

```text
[S] Create `Managers/Domain/Order.cs` with:
    - enum OrderStatus { Pending=0, Submitted=1, Preparing=2, Ready=3,
      Delivered=4, Cancelled=9 }
    - class Order: Id, TableNumber (int), ServerName, Status (OrderStatus),
      Subtotal, TaxAmount, Total (all decimal), CreatedUtc, UpdatedUtc,
      List<OrderItem> Items.
    - class OrderItem: Id, OrderId, MenuItemId, Name, UnitPrice, Quantity, Notes.

[R] CRITICAL — Do NOT:
    1. Add a FK navigation property from OrderItem back to Order. We only
       navigate Order→Items.
    2. Compute totals here. Totals belong in the mapping extensions where we
       can keep rounding consistent.

[U] Internal aggregate. The Order is the root.

[B] N/A.
```

## Prompt C2 — Order mapping extensions with tax calculation 💬

```text
[S] Create `Managers/Extensions/OrderMappingExtensions.cs` with:
    - private const decimal TaxRate = 0.07m
    - ToDomain(OrderViewModel) → Order  with new Guid for order and each item,
      Status=Submitted, calls RecalculateTotals before returning
    - extension method RecalculateTotals(this Order) that sets Subtotal, TaxAmount,
      Total all rounded to 2 dp
    - ToServiceModel(this Order) → OrderServiceModel  (Status as string via .ToString())
    - ToServiceModels(IEnumerable<Order>) → IEnumerable<OrderServiceModel>

[R] CRITICAL — Do NOT:
    1. Read tax rate from configuration in this POC. Constant is fine for now;
       leave a `// TODO: move to per-table tax engine` comment.
    2. Use floating-point math anywhere — strictly decimal with explicit Round.
    IMPORTANT — Do NOT:
    3. Calculate tip here. Tip is the Payments service's responsibility.

[U] Used by the Business layer when a new order is submitted from the POS.

[B] N/A.
```

## Prompt C3 — Orders Data + Business + Facade + Controller 🤖

```text
[S] Replicate the Menu pattern (B7→B10) for Orders, but with these additions:
    - Data: GetActiveAsync (Status != Delivered && != Cancelled, ordered by
      CreatedUtc asc — FIFO for the kitchen), GetByTableAsync(int).
      Include(o => o.Items) on all reads.
    - Business: SubmitAsync(OrderViewModel) throws InvalidOperationException
      if items.Count == 0. UpdateStatusAsync(Guid, string) parses the string
      via Enum.TryParse, throws InvalidOperationException on bad value.
    - Controller routes:
      POST   /api/Orders                       (201 CreatedAtAction)
      GET    /api/Orders                       (all)
      GET    /api/Orders/active                (kitchen queue)
      GET    /api/Orders/table/{tableNumber}   (table routing)
      GET    /api/Orders/{id:guid}             (single, 404 if missing)
      PATCH  /api/Orders/{id:guid}/status      (body: OrderStatusUpdateViewModel)

[R] CRITICAL — Do NOT:
    1. Implement cross-service calls to the Menu API to validate MenuItemId.
       For this POC we trust the POS to send valid ids. Leave a `// TODO:
       cross-service validation` note in Business.
    2. Implement cancellation/refund logic — separate workflow.
    IMPORTANT — Do NOT:
    3. Calculate tip or process payment here. That's the Payments service.
    4. Hard-delete orders. Use status Cancelled.

[U] Called by the POS at order submit, and by the kitchen display (Active
    Orders page) every 5 seconds via polling.

[B] When updating an existing order's status, ONLY change Status and UpdatedUtc.
    Do NOT alter line items, totals, or table number.
```

---

# PART D — Payments microservice

## Prompt D1 — Payment domain + view + service models 💬

```text
[S] Create:
    - Domain/Payment.cs with enums PaymentStatus { Pending=0, Authorized=1,
      Captured=2, Failed=9 } and PaymentMethod { Cash=0, CreditCard=1,
      DebitCard=2, MobileWallet=3 }, and class Payment containing OrderId,
      TableNumber, Subtotal, TaxAmount, TipAmount, TipPercent, Total, Method,
      Status, Last4 (string, 4 chars), AuthorizationCode, timestamps.
    - ViewModels/PaymentViewModel.cs taking OrderId, TableNumber, Subtotal,
      TaxAmount, TipPercent (nullable), TipAmount (nullable), Method (string),
      Last4 (string, StringLength 4 min and max). Plus
      TipCalculationRequestViewModel containing only Subtotal.
    - ServiceModels/PaymentServiceModel.cs with Method/Status as strings, plus
      TipSuggestionServiceModel { Subtotal, List<TipOption> } and TipOption
      { Percent, Amount, Label }.

[R] CRITICAL — Do NOT:
    1. Accept or store full PAN, CVV, expiry, or card-holder name. EVER.
       Last4 only.
    2. Log Last4 at Information level — Debug max.
    IMPORTANT — Do NOT:
    3. Default Method to anything other than CreditCard.
    4. Set TipPercent and TipAmount as required — they're mutually exclusive
       and resolved in the mapping extensions.

[U] PCI scope minimization: tokenization happens client-side in a real
    deployment. POC simulates authorization with a fake auth code.

[B] N/A.
```

## Prompt D2 — Payment mapping extensions with tip resolution 💬

```text
[S] Create `Managers/Extensions/PaymentMappingExtensions.cs` with:
    - static (decimal tipAmount, decimal tipPercent) ResolveTip(decimal subtotal,
      decimal? tipPercent, decimal? tipAmount)
      Rules: if subtotal<=0 → (0,0). If tipAmount has a positive value, compute
      tipPercent from it. Otherwise tipPercent defaults to 18 when null;
      compute tipAmount = subtotal * (percent/100). All values rounded to 2 dp.
    - ToDomain(this PaymentViewModel) using ResolveTip and computing
      Total = subtotal + tax + tipAmount.
    - ToServiceModel(this Payment).
    - static BuildTipSuggestions(decimal subtotal) returning a
      TipSuggestionServiceModel with options at 15% "Good", 18% "Great",
      20% "Excellent", 25% "Outstanding".

[R] CRITICAL — Do NOT:
    1. Apply a minimum tip floor or "service fee" — explicit user choice only.
    2. Round during intermediate calculations — only at boundaries.
    IMPORTANT — Do NOT:
    3. Use ResolveTip outside payment processing — it's payment-specific logic.

[U] The "automatic tip calculation" feature. The default of 18% applies only
    when both TipAmount and TipPercent are null.

[B] N/A.
```

## Prompt D3 — Payments Data + Business + Facade + Controller 🤖

```text
[S] Standard layered build (matches B7→B10) plus:
    - Data: GetByIdAsync, GetByOrderAsync(Guid), GetAllAsync, AddAsync,
      UpdateStatusAsync(Guid id, PaymentStatus status, string authCode).
    - Business.ProcessAsync(PaymentViewModel):
        1. Map vm.ToDomain() and AddAsync (Status=Pending)
        2. Simulate authorization: generate AUTH-XXXXXXXX (8 hex chars),
           call UpdateStatusAsync(id, Captured, authCode).
        3. Return the captured ServiceModel.
    - Facade `IPaymentFacade` MUST expose these exact method names (the
      controller calls them by these names — drift here breaks the build):
        ProcessPaymentAsync(PaymentViewModel, CancellationToken)
        GetPaymentAsync(Guid, CancellationToken)
        GetPaymentsByOrderAsync(Guid, CancellationToken)
        GetAllAsync(CancellationToken)
        GetTipSuggestions(decimal subtotal)
    - Controller routes:
      POST   /api/Payments                                (201)
      GET    /api/Payments                                (all)
      GET    /api/Payments/{id:guid}                      (single)
      GET    /api/Payments/order/{orderId:guid}           (by order)
      GET    /api/Payments/tip-suggestions?subtotal=N     (returns
                                                           TipSuggestionServiceModel)

[R] CRITICAL — Do NOT:
    1. Make HTTP calls to a real payment processor. Simulated authorization
       only — generate the auth code in C#.
    2. Persist any card data beyond Last4 and the auth code.
    3. Implement refund or void flows yet. Out of POC scope.
    4. Rename the Facade methods listed in [S]. The Controller already
       references those exact names — drifting (e.g. `CalculateTipSuggestions`
       instead of `GetTipSuggestions`, or `GetByOrderAsync` instead of
       `GetPaymentsByOrderAsync`) is a CS1061 build break.
    IMPORTANT — Do NOT:
    5. Process the same Payment row twice (idempotency). Leave a `// TODO:
       idempotency key` comment in ProcessAsync.

[U] The POS calls ProcessPaymentAsync immediately after the Order is created.
    Auth codes appear on the success receipt screen.

[B] When updating a payment's status, ONLY change Status, AuthorizationCode,
    and UpdatedUtc. Do NOT alter the financial fields.
```

---

# PART E — Angular 20 POS

## Prompt E1 — Angular workspace scaffold 🤖

```text
[S] Create `src/AspireCafe.POS/` with: package.json (Angular 20, scripts start
    using PORT env var defaulting to 4200), angular.json (application builder,
    standalone), tsconfig.json + tsconfig.app.json (strict), src/main.ts
    (bootstrapApplication), src/index.html with the Inter + Playfair Display
    Google fonts, src/styles.css with the CSS variables described in [C], and
    a Dockerfile (node:22-alpine → nginx:alpine).

[C] - Use standalone components only. NO NgModules.
    - bootstrapApplication(AppComponent, appConfig)
    - package.json devDependencies pin `"typescript": "~5.8.0"`. Angular 20's
      compiler-cli has peerDependency `typescript >=5.8.0 <5.9.0` — older
      pins throw `verifySupportedTypeScriptVersion` at build time.
    - CSS variables: --bg-deep #1a1410, --bg-surface #241c17,
      --accent #d4a574 (caramel), --text-primary #f5ede2, --success #7fb069,
      --danger #c9504a. Font families: 'Playfair Display' display, 'Inter' sans.
    - src/environments/environment.ts with menuApi/ordersApi/paymentsApi
      pointing to localhost:5101/5102/5103/api

[R] CRITICAL — Do NOT:
    1. Generate NgModules anywhere — Angular 20 standalone components only.
    2. Add Material, PrimeNG, Bootstrap, or any other UI library. Hand-rolled
       CSS only — the cafe aesthetic should look bespoke, not framework-y.
    3. Pin TypeScript to ~5.6 or ~5.7. Angular 20 requires 5.8.x. AI often
       grabs a stale "safe" version from training data — explicitly pin 5.8.
    IMPORTANT — Do NOT:
    4. Use the Webpack-based builder. Use @angular-devkit/build-angular:application.
    PREFERRED — Do NOT:
    5. Add ESLint/Prettier configs in this scaffold — keep noise low.

[U] Served by Aspire as an npm app on :4200. Customer-visible POC; the team
    showed early mockups in cafe-warm tones — keep it that way.

[B] N/A.
```

## Prompt E2 — Core models + services 🤖

```text
[S] Create:
    - `src/app/core/models/models.ts` with TypeScript interfaces matching the
      ServiceModels exposed by the APIs: MenuItem, CartLine, OrderItem, Order,
      OrderSubmit, TipOption, TipSuggestion, Payment, PaymentSubmit.
    - `src/app/core/services/menu.service.ts`, `order.service.ts`,
      `payment.service.ts`: each `@Injectable({providedIn:'root'})`, using
      inject(HttpClient) and the environment URLs.
    - `src/app/core/services/cart.service.ts` using SIGNALS:
      _lines, _tableNumber (default 1), _serverName (default 'Server'),
      computed itemCount, subtotal, tax (subtotal*0.07), total. Methods:
      add(MenuItem), increment, decrement, remove, updateNotes, clear,
      setTable, setServer.

[C] - Use Angular 20 signal API (`signal`, `computed`, `.asReadonly()`)
    - All monetary computed values rounded to 2 dp via +x.toFixed(2)
    - HTTP services return Observable<T>

[R] CRITICAL — Do NOT:
    1. Use NgRx, NGXS, Akita, or any external state library. Signals only.
    2. Persist cart state to localStorage in this POC — it's a single session.
    IMPORTANT — Do NOT:
    3. Compute tip in CartService. Tip lives in PaymentComponent.
    4. Bake the tax rate anywhere except CartService — single source.

[U] Cart is the only piece of cross-component state. Other features reach in
    via inject(CartService).

[B] N/A.
```

## Prompt E3 — App shell with top bar and routes 🤖

```text
[S] Create:
    - `src/app/app.config.ts`: provideRouter, provideHttpClient(withFetch()),
      provideAnimations, provideZoneChangeDetection({eventCoalescing:true}).
    - `src/app/app.routes.ts`: '' → 'menu', '/menu' → MenuComponent (lazy),
      '/payment' → PaymentComponent (lazy), '/orders' → TableRoutingComponent
      (lazy). All loadComponent dynamic imports.
    - `src/app/app.component.ts`: standalone shell with a sticky top bar
      containing brand (☕ + "AspireCafe / Point of Sale"), three nav links
      with active state, and three status cards (Table #, Items, Subtotal).
      Subtotal card is the "highlight" variant.

[C] - Brand mark: 48px circle with gradient from --accent to --espresso (#6b3410)
    - Nav links: pill-shaped, active uses --accent-glow background
    - Payment link disabled when cart.itemCount() === 0
    - Status cards live-bind to CartService signals

[R] Do NOT:
    1. Use ngFor / ngIf — use the new @for / @if control-flow syntax.
    2. Hard-code totals — read from CartService signals.

[U] This shell stays mounted across navigation; cart state is global.

[B] N/A — new file.
```

## Prompt E4 — Menu component (browse + order) 🤖

```text
[S] Create `src/app/features/menu/menu.component.ts` as a standalone component:
    - 2-column layout: menu grid (responsive auto-fill) on the left, sticky
      cart panel on the right
    - Filter pills above the grid: "All" + each unique category. Selected pill
      uses --accent background.
    - Item cards: image area (gradient bg + a category-appropriate emoji),
      name (Playfair), price (caramel), description (muted), category + prep-
      time chips. Whole card is clickable → cart.add(). Hover lifts the card.
    - Cart panel: table# + server inputs (bound to cart signals), line list
      with qty + / − controls, totals (subtotal / tax / total), primary CTA
      "Proceed to Payment →" disabled while empty, plus "Clear Order" ghost
      button when items present.
    - On load: menuService.getMenu().subscribe; show spinner while loading
      and a friendly error block when the API can't be reached.

[C] - Use signals for items / loading / error / selectedCategory
    - Use computed for categories (Set of distinct), visibleItems (filtered)
    - Provide a small bgFor(item) helper returning a category-specific gradient
      and an emojiFor(item) helper mapping item names to emojis (☕ 🥛 🧊 🥐 🧁
      🥑 🥪 🍵)

[R] CRITICAL — Do NOT:
    1. Allow items where IsAvailable=false to be added to the cart — show an
       "86'd" badge instead.
    IMPORTANT — Do NOT:
    2. Block the UI while loading. Show the spinner, render when ready.
    3. Re-fetch the menu on every category change. Filter client-side.

[U] The first page baristas see. Must be tap-friendly on a 10" tablet.

[B] N/A.
```

## Prompt E5 — Payment component with tip selector 🤖

```text
[S] Create `src/app/features/payment/payment.component.ts` as a standalone
    component with a 3-stage state machine (signal of 'review'|'processing'|
    'complete'):

    REVIEW stage (2-column layout):
      LEFT card "Tip": 4 tip cards (15/18/20/25%) showing percent + computed
        amount + label (Good/Great/Excellent/Outstanding). Selected card has
        --accent border + glow. Below: "Custom Amount" pill that reveals a
        $-prefixed number input.
      LEFT card "Payment Method": 4 method cards (Credit/Debit/Cash/Mobile)
        with icons. For Credit/Debit show a Last-4-digits input (numeric,
        maxlength 4).
      RIGHT (sticky): receipt card showing the table badge, line items, tax,
        live tip line with a "(N%)" badge, grand total in Playfair caramel,
        primary "Pay $X.XX →" CTA, ghost "Back to Menu" button.

    PROCESSING stage: centered spinner card "Authorizing your payment of $X.XX…"

    COMPLETE stage: centered success card with a green check (pop animation),
      "Payment Complete", "Order routed to Table N", receipt summary with
      Method, Auth Code, optional •••• Last4, and two CTAs ("Start New Order"
      → cart.clear() + /menu, "View All Active Orders" → /orders).

[C] - On submit: call orderService.submit(...), then on success call
      paymentService.process(...). If either fails, return to 'review'.
    - effectiveTipAmount = computed: customTipAmount if isCustomTip, else the
      currently-selected option's amount.
    - grandTotal = computed: subtotal + tax + effectiveTipAmount.
    - canPay = computed: cart not empty AND (Cash/Mobile OR last4 matches /^\d{4}$/).
    - On init, call paymentService.getTipSuggestions(subtotal). On error,
      fall back to client-side calculation.

[R] CRITICAL — Do NOT:
    1. Submit the order if cart.itemCount() === 0 — redirect to /menu in ngOnInit.
    2. Allow the Pay button to fire twice — stage transition is the guard.
    3. Display, log, or store more than the last 4 digits anywhere in the
       Angular layer.
    IMPORTANT — Do NOT:
    4. Show stale tip suggestions if the user changes table or items — for
       this POC we assume cart is finalized before navigating here.
    5. Auto-advance from 'complete' — keep the receipt visible until the
       cashier chooses next action.

[U] Tip is the most-touched UI on the page — make the selected card highly
    distinguishable so the cashier can confirm at a glance before pressing
    Pay.

[B] If a network error occurs, REVERT to 'review' stage and keep all selected
    values intact. Do NOT clear the cart on failure.
```

## Prompt E6 — Active Orders / Table Routing component 🤖

```text
[S] Create `src/app/features/table-routing/table-routing.component.ts` as a
    standalone component that:
    - On init, calls orderService.getActive() and sets a 5-second polling
      interval (clearInterval in ngOnDestroy).
    - Renders a responsive grid of order cards. Each card displays:
      - Large Playfair table number (caramel) with "TABLE" label
      - Status pill colored by status (Submitted=accent, Preparing=warning,
        Ready=success)
      - 4px left border in the same status color
      - Server name
      - Item lines (qty pill + name + optional italic notes)
      - Total in Playfair caramel
      - Status-advance buttons: Start (Submitted→Preparing), Ready
        (Preparing→Ready), Deliver (Ready→Delivered)
    - Empty state with 🍽️ icon when no active orders.

[C] - Use [attr.data-status] binding for CSS state selection
    - Buttons call orderService.updateStatus(orderId, nextStatus).subscribe(reload)
    - Manual Refresh button + auto-poll every 5s

[R] CRITICAL — Do NOT:
    1. Allow users to skip statuses (e.g. Submitted → Delivered). Show only
       the legal next-status button per the state machine.
    2. Show Delivered or Cancelled orders here — the API already filters them.
    IMPORTANT — Do NOT:
    3. Use WebSockets/SignalR in this POC. Polling is intentional — a TODO
       in the markdown notes a future move to SignalR.
    PREFERRED — Do NOT:
    4. Animate every poll refresh — only animate status changes.

[U] Lives on a kitchen tablet. Must remain readable across the room.

[B] N/A.
```

---

# PART F — Wire-up, run, and recover

## Prompt F1 — Wire AppHost project references ✏️

```text
[S] Edit `src/AspireCafe.AppHost/AspireCafe.AppHost.csproj` to add
    ProjectReference items for the three API csproj files we just created.

[C] Single file edit. Keep the existing PackageReference items unchanged.

[R] CRITICAL — Do NOT:
    1. Add a ProjectReference to AspireCafe.ServiceDefaults from the AppHost.
       The APIs reference ServiceDefaults; the AppHost doesn't need it directly.
    2. Add a ProjectReference to AspireCafe.POS — it's an npm app, not a
       .NET project.

[B] B-DOMINANT (Edit Mode):
    - ONLY ADD the three ProjectReference lines.
    - Do NOT modify any existing element in the csproj.
    - Do NOT reformat unrelated lines.
    - All other project state must be byte-for-byte identical.
```

## Prompt F2 — Sanity-check end-to-end ✏️

```text
[S] Run `dotnet build` on the solution, then `dotnet run --project
    src/AspireCafe.AppHost`. Verify in the Aspire dashboard that all four
    resources reach Healthy state, then open the POS at port 4200 and walk
    the full flow: add 3 items → set table 5 → proceed to payment → select
    20% tip → pay with Credit/Last-4 1234 → confirm success → view in
    /orders → advance through Start/Ready/Deliver.

[R] If any step fails:
    CRITICAL — Do NOT:
    1. Just say "should work" — paste the actual error from logs.
    IMPORTANT — Do NOT:
    2. Fix multiple issues at once. Use the iterative refinement loop
       (Identify → Diagnose → Refine → Re-execute → Verify, slide 26).
    3. Disable health checks to mask startup issues.

[B] Code should not be edited unless a test/run failure is reproduced. Reading
    only.
```

---

# PART G — Diagnostic prompts (use when output is wrong)

> From slide 25, the five failure modes. Drop these into chat when something
> in the output above goes sideways.

### G1 — "AI implemented business rules I didn't ask for"

```text
[S] Re-review the file `<path>`. Identify any logic that wasn't explicitly in
    the original [S] block.

[R] CRITICAL — Do NOT:
    1. Add eligibility, discount, loyalty, refund, or tax-engine logic
       anywhere in this codebase yet. POC scope is fixed.
    Replace any such code with a comment `// TODO: not in POC scope`.

[B] Do NOT change method signatures or interfaces. Only remove or stub
    surprise logic.
```

### G2 — "Output doesn't match our architecture"

```text
[S] Compare `<path>` against `src/AspireCafe.Menu.API/` for layering. The Menu
    service is the architectural reference.

[C] Strict onion: Controller → Facade → Business → Data → DbContext.
    Mapping in Extensions. Each layer's dependency points inward only.

[R] CRITICAL — Do NOT:
    1. Allow a Controller to inject IBusinessManager or IDataManager.
    2. Allow a Facade to inject IDataManager or DbContext.
    3. Allow a Business class to import EF Core namespaces.

[B] Refactor in place — file paths and public class names stay identical.
```

### G3 — "Plausible but incorrect logic" (Plausibility Trap)

```text
[S] Walk me through `<method>` step-by-step using a worked numerical example
    with subtotal=$12.34, tip=18%, tax=7%. State each intermediate value to
    2 dp.

[R] Do NOT change the code yet. Diagnostic only.

[B] N/A — analytical step.
```

### G4 — "Generic output, not cafe-specific"

```text
[S] Re-do `<file>` with concrete cafe context: tap-friendly tablet UI, baristas
    standing at a counter, expected order time under 30 seconds.

[C] Visual style: dark espresso bg, caramel accents, generous touch targets,
    minimum 14px text, primary CTAs at least 48px tall.

[R] Do NOT use generic placeholder copy like "Item 1" or "Lorem ipsum". Use
    real cafe-specific names from MenuDbContext.SeedAsync.

[B] N/A.
```

### G5 — "Agent mode created files I didn't expect"

```text
[S] List every file you created in the last action. For each, state whether
    it was in my original [S] block.

[R] CRITICAL — Do NOT:
    1. Delete anything yet. Wait for my confirmation.

AGENT MODE ADDITION (next attempt):
    - Restrict file creation to these directories: <list>
    - Do NOT create files outside these directories.
    - Verify the build passes before declaring complete.
```

---

# How this maps back to the SCRUB deck

| Deck slide                                 | Pattern used here                                                               |
| ------------------------------------------ | ------------------------------------------------------------------------------- |
| Slide 7 — SCRUB Five Elements              | Every prompt is `[S][C][R][U][B]`                                               |
| Slide 8 — Tight vs Vague Scope             | Each `[S]` names exact methods/routes/files                                     |
| Slide 9 — Constraints match architecture   | Versions and patterns pinned via the pre-loaded CLAUDE.md                       |
| Slide 10/11 — Restrictions in HRIS-style   | Tiered CRITICAL/IMPORTANT/PREFERRED throughout                                  |
| Slide 13 — Behavior dominant in Edit Mode  | Prompts F1 and the G-series enforce "only ADD"                                  |
| Slide 17 — Agent Mode Addition             | Prompts marked 🤖 include file-placement constraints                            |
| Slide 21 — Prompt Chaining                 | A→B→C→D→E→F is the chain: Context → Plan → Execute → Verify                     |
| Slide 22 — Tiered Negative Specification   | Every [R] block uses the three tiers                                            |
| Slide 23 — Layered SCRUB                   | Per service: Domain → Mapping → Data → Business → Facade → Controller → Program |
| Slide 25 — Five Failure Modes              | The G-series matches each failure mode 1-to-1                                   |
| Slide 26 — Iterative Refinement Loop       | Prompt F2 explicitly invokes the loop on failure                                |
| Slide 29/30 — Custom Instructions pre-load | Phase 0 `CLAUDE.md` does exactly this                                           |

> **The key insight from slide 33:** *"They know what to exclude."*
> Notice how much of each prompt is `[R]` rather than `[S]`. That's the discipline.
