# AspireCafe — Next Steps

This POC proves the architecture (Aspire + .NET 10 microservices + Angular 20) and the three target user flows. Below is what would be next on the road to a real cafe deployment, grouped by priority. Each item is sized as **S/M/L** and includes the SCRUB hooks you'd attach when implementing.

---

## P0 — Required before any pilot

### 1. Real payment processor integration (L)
- Swap the `PaymentBusinessManager.ProcessAsync` stub for Stripe Terminal or Square Reader SDK.
- Tokenize on the client; the API should NEVER see a PAN.
- SCRUB hook: this is exactly the kind of `[R] CRITICAL — Do NOT process card data server-side` rule the deck calls out as the most important element in HRIS-style domains.

### 2. AuthN + AuthZ (M)
- Add OpenID Connect (Microsoft Entra ID, Auth0, or Keycloak).
- Roles: `Barista`, `Manager`, `Kitchen`. Map to endpoints.
- The current CORS-only model is fine for the POC, not for production.

### 3. EF Core migrations instead of EnsureCreatedAsync (S)
- Each service generates its own initial migration.
- AppHost runs migrations once on startup via a hosted service.

### 4. Cross-service order/menu validation (M)
- Currently the Orders API trusts the client to send valid `MenuItemId`s. Add a typed `HttpClient` (via Aspire service discovery) to call Menu.API and validate.
- Cache with a short TTL since the menu changes infrequently.

### 5. Idempotency keys on Payments (S)
- A double-tap on "Pay" should never charge twice. Add an idempotency key column and unique index.

---

## P1 — Operational excellence

### 6. Real-time order updates via SignalR (M)
- Replace the 5-second polling on `/orders` with a SignalR hub on Orders.API.
- Kitchen sees new orders instantly; cashier sees status changes live.

### 7. Structured logging + correlation IDs (S)
- ServiceDefaults already wires OpenTelemetry. Add a request-scoped CorrelationId header that propagates across service-to-service HTTP calls.
- Add Serilog + Seq sink for local dev (Aspire can host Seq as a resource).

### 8. Health-check depth (S)
- Today: shallow `/health`. Add EF Core `AddDbContextCheck<>` and a SQL ping. Aspire dashboard already wires up the UI.

### 9. Per-environment tax engine (M)
- The 7% constant in `OrderMappingExtensions` needs to come from configuration (US tax differs by jurisdiction; many cafes operate in multiple cities).
- Worst case for the SCRUB deck: this is the **Plausibility Trap** in slide 11 — generated calculation logic that "looks fine" but is jurisdictionally wrong.

### 10. Cancellation + refund flows (M)
- Orders can be `Cancelled` but only Submitted ones. No refund flow yet.
- Payment refunds need processor support and policy guard-rails (manager approval, time window).

---

## P2 — User experience polish

### 11. Modifiers & options (L)
- Menu items need variants ("Latte: oat / almond / 2% / whole; small / medium / large").
- Data model: `MenuItem` becomes a head row + `MenuItemOption` rows with priceAdjust.
- Cart line stores the chosen modifiers and shows them on the receipt.

### 12. Split checks (M)
- Two diners at table 7 want to split: same Order, multiple Payment rows.
- The Payments domain already accepts a Subtotal so this is mainly UI work.

### 13. Receipt printing / emailing (S)
- After payment, optionally email the receipt. Print to a thermal printer via a kiosk-side helper.

### 14. Offline mode for the POS (L)
- IndexedDB queue of pending orders + payments. Sync when reconnected.
- Conflict resolution: payments win; menu items reconcile by id.

### 15. Accessibility audit (S)
- Color contrast on the dark theme is borderline on a few elements. Run axe-core and fix.
- Add keyboard navigation to the menu grid (currently mouse/touch-first).

---

## P3 — Platform & scale

### 16. Containerize APIs with Aspire publish (S)
- `dotnet run --project AspireCafe.AppHost --publisher manifest` produces a manifest that maps cleanly to Azure Container Apps or Kubernetes.

### 17. Move secrets out of AppHost parameters (S)
- For local dev, `dotnet user-secrets` is fine. For deployment, Azure Key Vault or HashiCorp Vault.

### 18. Database per service, but shared infrastructure (M)
- Today: one SQL Server container with three databases. Production: keep the per-service DB pattern but on separate elastic pools.

### 19. Event-driven coordination (L)
- When an Order is Submitted, publish `OrderSubmitted` to a queue (Azure Service Bus or RabbitMQ).
- Payments listens for `OrderSubmitted` to pre-warm a payment intent.
- Eventual consistency reduces inter-service HTTP coupling.

### 20. Multi-tenant: chain of cafes (L)
- Each cafe = a tenant id stamped on every row.
- Per-tenant menu, per-tenant tax rate, per-tenant Stripe account.

---

## SCRUB prompt patterns to attach

When you take on any item above, build the prompt with this skeleton:

```
[S] One sentence. Name the exact deliverable.
[C] Reference: src/AspireCafe.Menu.API/ for architecture patterns. C# 14, .NET 10.
[R] CRITICAL — Do NOT:
    1. <the specific scope creep this item invites>
    2. Modify any of the other microservices.
    IMPORTANT — Do NOT:
    3. Skip the test for the boundary condition.
[U] AspireCafe POC — see CLAUDE.md for system context.
[B] Existing public interfaces of <touched files> must remain byte-for-byte identical.
```

---

## Suggested order if you have one week

| Day | Items | Why |
|-----|-------|-----|
| Mon | #5 idempotency + #7 correlation IDs | Cheap operational wins |
| Tue–Wed | #1 real payment processor | Biggest unknown — surface risks early |
| Thu | #2 AuthN/AuthZ scaffolding | Unblocks everything else |
| Fri | #3 migrations + #4 cross-service validation | Solidify the data story |

Everything else can be sequenced once you have payment + auth + migrations in place.
