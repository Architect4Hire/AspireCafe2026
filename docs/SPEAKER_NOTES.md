# AspireCafe Conference Session — Speaker Notes

> 34 slides. Target runtime: **90 minutes**. Two live demos.
>
> These notes are written to be read aloud or paraphrased. Each slide has:
> - **Time** — how long to stay on it (cumulative time guidance)
> - **Say** — the spine of what to say
> - **Show** — what to do or point to on screen
> - **Watch** — interaction or pacing cues
>
> Total speaking time accounts for ~80 minutes of talk + ~10 minutes of unstructured Q&A buffer.

---

## Slide 1 — Cover  (0:00 — 1 min)

**Say.** Welcome. Today we're going to build a working cafe point-of-sale system — three microservices, an Angular 20 frontend, a SQL Server backend, all orchestrated by Aspire — and we're going to do it using SCRUB at every single step. By the end of this session you'll have a complete, version-controlled prompt library you can drop into your own repo on Monday morning.

**Show.** Let your eyes linger on the bottom tech band — that's the whole stack we'll touch today.

**Watch.** If you have laptops out, this is a build-along session. The full repo is downloadable from the QR code I'll show at the end.

---

## Slide 2 — Agenda  (1:00 — 2 min)

**Say.** Quick map. We spend the first hour walking step-by-step through the build — solution skeleton, then Aspire orchestration, then each microservice's onion layers, then the Angular shell and feature components. That's 75 minutes of build content. We pause for a 10-minute end-to-end demo, and we wrap with how to take this home — building your own SCRUB prompt library.

**Show.** Point at item 6, Payments. Note that one is where R earns its keep — the Plausibility Trap lives in money math.

**Watch.** Read the room. If everyone is new to SCRUB, slow down on slide 5. If they're SCRUB veterans, accelerate through 5 and dwell on slide 17.

---

## Slide 3 — Pull quote  (3:00 — 1 min)

**Say.** This is the riff on the original quote from the Deep Dive deck. In most software, a bad prompt wastes time. In a cafe POS, a bad prompt quietly miscalculates the tip on every receipt for six months — and nobody notices because it always says some number that "looks right." That's the Plausibility Trap. Financial code is the new HRIS code.

**Show.** Let it sit silently for a beat after you read it.

**Watch.** This is a planted seed. You'll cash it in on slide 17.

---

## Slide 4 — Why AspireCafe  (4:00 — 3 min)

**Say.** Why a cafe POS as the teaching vehicle? Five reasons. One — it has three genuinely separate microservices, so we get to practice the onion architecture three times. Two — real money math, so R becomes the star. Three — an end-to-end user flow that crosses every service. Four — Aspire orchestrates everything as containers, no Docker Compose required. Five — strong opinions: hand-rolled mapping, signals everywhere, no AutoMapper. The lessons travel to any architecture.

**Show.** Point at the second row. Money math is where the Plausibility Trap lives — and where R earns the title "most important element."

**Watch.** If someone asks "why not just use a SaaS POS" — you'd never build this for production from scratch. The point is the practice ground.

---

## Slide 5 — SCRUB Five Elements  (7:00 — 4 min)

**Say.** Quick recap. Five letters. **S** — Scope. What am I asking for. Be specific. Name the methods. **C** — Constraints. Match my codebase, not generic patterns from training data. **R** — Restrictions. The starring element. "Do NOT…". What you exclude prevents incorrect output more reliably than what you include. **U** — Usage. Who calls this? What's the audit story? **B** — Behavior preservation. Critical for edit mode — what must not change.

**Show.** Tap each column header as you say the letter. Linger longest on R.

**Watch.** If anyone says "we already do this," nod and say — great, the question is whether you do it consistently. We will today.

---

## Slide 6 — Section divider  (11:00 — 30s)

**Say.** Alright — let's build. Nine SCRUB prompts. One running cafe.

**Show.** Brief breather. Click through.

---

## Slide 7 — Solution Structure overview  (11:30 — 3 min)

**Say.** Here's where we're headed. The Aspire AppHost at the bottom orchestrates one SQL Server container with three databases, three API microservices each running on its own port, and an Angular 20 POS at the top — and every one of those arrows is a service-discovery reference Aspire wires up automatically. Note the color coding on the API cards — blue for Menu, teal for Orders, red for Payments. Those colors match the SCRUB letter colors we'll see throughout the deck.

**Show.** Trace the arrows from POS down to APIs down to DBs.

**Watch.** Common question: "why one SQL Server container with three databases instead of three containers?" Answer — for dev-mode this is faster to start; in production you'd keep the per-service database pattern but on separate elastic pools. We cover that in Next Steps.

---

## Slide 8 — Step 1: Solution Bootstrap  (14:30 — 3 min)

**Say.** Prompt #1. We're in Copilot Agent Mode because we're scaffolding five projects at once. Look at the structure — Scope names every project exactly. Constraints pins versions. Restrictions in this case is about scope creep — "do not add any project not listed." That's a one-line preventive measure that saves you finding out later that AI added a TaxService.csproj nobody asked for.

**Show.** Point at the R block specifically — both the CRITICAL and IMPORTANT tiers.

**Watch.** Mention: this prompt has nothing about the source code yet. We're just laying paving stones. Resist the temptation to scope-creep S.

---

## Slide 9 — Step 2: Aspire AppHost  (17:30 — 4 min)

**Say.** Prompt #2 — orchestration. Same Agent Mode. The S block is now explicit that the AppHost is **three files together**, not just one: the csproj, Program.cs, and `Properties/launchSettings.json`. That third one trips people up. The AppHost is technically an ASP.NET Core executable — without launch profiles it has no URL to bind to and `dotnet run` returns immediately. No dashboard. No DCP. No resources started. No error message. That's CRITICAL #3 in the R block. We also call out the inline SDK reference for the csproj — `<Sdk Name="Aspire.AppHost.Sdk" Version="9.4.0" />`, not the deprecated `<IsAspireHost>true</IsAspireHost>` workload property — and the standard SQL password parameter pattern. The fourth file, `appsettings.Development.json`, holds a dev-only fallback for the SQL password so first-run works without `dotnet user-secrets set`. Aspire 9.4 will otherwise prompt for the value in the dashboard before starting SQL.

**Show.** Highlight the green `Properties/launchSettings.json` line in [S]. Then CRITICAL #3 with its "silent no-op" warning.

**Watch.** Some folks may not have seen Aspire before. Acknowledge: "If Aspire is new to you, just know it's the orchestrator — it spins up the containers, gives you a dashboard, and wires service discovery." Don't go deeper unless asked. If anyone hits this in a follow-up: the launchSettings.json must contain at minimum `applicationUrl`, `DOTNET_DASHBOARD_OTLP_ENDPOINT_URL`, and `DOTNET_RESOURCE_SERVICE_ENDPOINT_URL`.

---

## Slide 10 — Step 3: ServiceDefaults  (21:30 — 2 min)

**Say.** Prompt #3 — the shared library every API depends on. OpenTelemetry, health checks, resilience, service discovery. The single most important detail here is in C: the namespace must be `Microsoft.Extensions.Hosting` so callers don't need a using statement. That's the kind of convention you encode once in your prompt and reuse forever.

**Show.** Highlight the namespace line.

**Watch.** Folks coming from older ASP.NET will ask: "is this the same as Startup.cs?" — no. Aspire's ServiceDefaults is purely cross-cutting concerns, not request pipeline.

---

## Slide 11 — The Onion — One Microservice, Five Layers  (23:30 — 4 min)

**Say.** Before we generate code, let's look at the shape we're generating. Every microservice in AspireCafe has the same five-layer structure. Layer 1: Domain and DbContext. Layer 2: View and Service models — separate types, always. Layer 3: Mapping extensions — hand-rolled, no AutoMapper. Layer 4: Data, Business, and Facade managers. Layer 5: the Controller and Program.cs. Each layer is a separate SCRUB prompt — that's Layered SCRUB from the Deep Dive deck. The folder on the right shows exactly how we organize the Managers directory inside each service.

**Show.** Walk through the L1 → L5 progression slowly. Point at the Managers folder layout.

**Watch.** This is the conceptual core. If anyone is fuzzy on the architecture, this is the slide to dwell on. Ask "Make sense?" before moving on.

---

## Slide 12 — Step 4a: Menu Layer 1 (Domain + EF)  (27:30 — 3 min)

**Say.** Layer 1 of the Menu service. We're in Claude Code Chat now — single-file generation, conversational. Look at what S contains: the POCO field list, the EF config, and the seed data — all explicit. The biggest restriction here is in IMPORTANT: don't implement business logic inside the entity or DbContext. No `Validate()`, no `CalculatePrice()`. Those go in the Business layer two steps later. If you skip this restriction, AI will helpfully add a `MenuItem.IsValid()` method and you'll fight it for a week.

**Show.** Read aloud "IMPORTANT - Do NOT 2".

**Watch.** Mention: the seed data is part of the prompt. Don't generate it separately — that's how seeds and entities drift.

---

## Slide 13 — Step 4b: Menu Models & Mapping  (30:30 — 3 min)

**Say.** Layer 2 of Menu. Three files. ViewModel for incoming, ServiceModel for outgoing, MappingExtensions for translation. The CRITICAL restriction is the showcase: no AutoMapper, no Mapster, no reflection-based mapper. Hand-rolled extension methods. We've all been burned by AutoMapper silently breaking after a property rename. The second CRITICAL — ViewModel and ServiceModel must be separate types even when they look identical today. Because in three months they won't.

**Show.** Read aloud both CRITICAL items.

**Watch.** Hand-rolled mapping is a strong opinion. If someone pushes back ("AutoMapper saves time"), acknowledge: "Sure — and it costs review time every PR. We trade compile-time clarity for setup time, on purpose." Move on.

---

## Slide 14 — Step 4c: Menu Onion Stack (Agent Mode)  (33:30 — 4 min)

**Say.** Layer 3 — and now we shift to Copilot Agent Mode because we're producing four files in coordinated directories. Notice the red `AGENT` badge inside the code block — that's the visual marker for Agent Mode-specific additions. The CRITICAL restriction is the heart of onion architecture: only the DataManager talks to the DbContext. Not the Business layer, not the Facade. If you let DbContext leak upward, you've broken the onion. The AGENT MODE INSTRUCTIONS block at the bottom — that's the addition the Deep Dive deck calls out. Tell the agent exactly which directories it can create in, and verify the build passes.

**Show.** Point at the red AGENT badge, then at the four directory paths at the bottom.

**Watch.** This is where Agent Mode shines OR fails spectacularly. The directory restrictions are the difference.

---

## Slide 15 — Step 4d: Menu Program.cs  (37:30 — 3 min)

**Say.** Layer 4 — the wire-up. Small file. The whole prompt could fit in a tweet but every line matters. The CRITICAL here is CORS: never open it to wildcard. Restrict to localhost:4200. The IMPORTANT clarifies the seeding strategy — `EnsureCreatedAsync` for POC; real systems get proper EF migrations. We acknowledge the trade-off in the prompt itself rather than leaving it to a code reviewer to catch.

**Show.** Highlight the CORS CRITICAL.

**Watch.** Someone will ask about migrations. Defer: "Next Steps doc has the migration upgrade path."

---

## Slide 16 — Step 5: Orders Microservice  (40:30 — 5 min)

**Say.** Orders is a compressed version of the same five-layer pattern. I won't repeat all four prompts — same shape. What's different is the domain. We have an Order aggregate with OrderItem children, a status enum that defines the state machine, and a 7% tax constant inside RecalculateTotals. The CRITICAL restrictions on this slide are domain-specific. One — never look up menu prices from the Menu database. The POS sends the name and unit price in the view model. That prevents cross-context coupling and a synchronous call across services. Two — always recompute totals server-side. Never trust the client's math. Three — no DELETE endpoint. Orders are cancelled, not destroyed.

**Show.** Point at restrictions 1 and 2 — they're the architectural keystones.

**Watch.** Question: "what if menu prices change between order and payment?" — that's why we snapshot the unit price into the order. The order captures price-at-order-time, which is correct accounting behavior.

---

## Slide 17 — The Plausibility Trap, Tip Calculation  (45:30 — 5 min)

**Say.** This is where the deck earns its keep. Two columns. Left — what AI happily generates when you don't write restrictions. Defaults to 20% because of US convention. Computes tax inside Payments. Uses float math for percentages. Stores full PAN "for refunds later." All of it compiles. All of it passes basic tests. All of it is wrong. Right — what tiered restrictions catch. Default tip is 18% because that's the reviewed-and-approved default — not a coding decision, a business decision. No PAN ever — Last4 only. Decimal.Round always — float will drift by a cent at high subtotals and nobody will know why. This is the Plausibility Trap, applied to your tip jar.

**Show.** Slow walk through the WITHOUT column. Then the WITH column. Pause on "Compiles. Tests pass. Wrong receipts every time."

**Watch.** This is the single most important slide of the build section. Pause for questions.

---

## Slide 18 — Step 6: Payments Microservice  (50:30 — 4 min)

**Say.** The Payments prompt itself. The Scope is detailed because the rules are subtle. The ResolveTip method has a specific resolution order: explicit amount wins → percent is computed from amount → falls back to 18%. If subtotal is zero or negative, return zero zero. Notice the green block in Scope listing the five EXACT Facade method names — `ProcessPaymentAsync`, `GetPaymentAsync`, `GetPaymentsByOrderAsync`, `GetAllAsync`, `GetTipSuggestions`. Why list them so explicitly? Because the controller already calls them by those names. If AI drifts to a "cleaner" name like `CalculateTipSuggestions` or `GetByOrderAsync`, you get CS1061 at build time. We literally caught this exact pair of build breaks while preparing this session — they're now CRITICAL #4 in the R block. The CRITICALs go from "never store PAN" to "never default to anything but 18%" to "never accept negatives" to "never rename the Facade methods." All four are about defending downstream callers.

**Show.** Read aloud the five Facade method names in green. Then CRITICAL 4.

**Watch.** Someone may ask: "why 18 and not 20?" — exactly the right question. Answer: "because it's a business decision documented in the prompt. The number is reviewed. The code can change to 22%, but only through review." If someone questions why Facade naming matters that much — point at the diagnostic card later in the deck, where CS1061 has its own row.

---

## Slide 19 — LIVE DEMO 1  (54:30 — 10 min)

**Say.** Demo time. I'm going to run the Payments prompt twice — once without the R block, once with. We'll compare the output side by side.

**Show.** Switch to your editor. Have the prompt without R queued. Run it. Read the output. Highlight the silent 20% default, the float math, the missing audit state.

Then run the version with the tiered R. Diff in the editor. Show:
- Default explicitly set to 18% with a comment.
- decimal everywhere.
- PaymentStatus.Pending → Captured transition with a separate call.

End by saying: "This is what tiered restrictions buy you. Five minutes of writing them. Hours of not chasing bugs later."

**Watch.** Have a fallback recording in case live AI hiccups. Demo gods are fickle.

---

## Slide 20 — Step 7a: Angular POS Shell  (64:30 — 3 min)

**Say.** Frontend. Angular 20, standalone components, signals everywhere. The shell sets up three lazy routes, a CartService in root, three HTTP services. Two C details worth pausing on. First — `inject()` for DI, `@if`/`@for` for control flow, `signal()`/`computed()` for state. That's the Angular 20 idiom set. Second — the TypeScript version pin. `~5.8.0`. Highlighted in green. Why call it out so explicitly? Because Angular 20's compiler-cli has a hard peer-dependency on TypeScript `>=5.8.0 <5.9.0`. Pin anything older — `~5.6`, `~5.7` — and the build dies at `verifySupportedTypeScriptVersion`. AI training data still has stale "safe" version pins from earlier Angular releases. The R block has its own CRITICAL forbidding that specifically. The other CRITICAL is unusual for an Angular prompt — no localStorage, no sessionStorage. In-memory signals only. Why? Because a POS at the counter restarts often, and we don't want stale carts from yesterday rehydrating into today. The IMPORTANT — no Material, no PrimeNG, no Tailwind, no ngrx. Strong opinion, deliberate.

**Show.** Highlight the green `"typescript": "~5.8.0"` in [C], then CRITICAL 3 in [R].

**Watch.** UI library opinion is divisive. Acknowledge: "you might disagree, and that's fine — the point is to encode the choice in the prompt so AI doesn't pick for you."

---

## Slide 21 — Step 7b: Menu Component  (67:30 — 3 min)

**Say.** Menu and cart, the main screen. The CRITICAL is the same one from Orders, restated in the UI prompt: never send Subtotal/Tax/Total to the API. Server is authoritative. The IMPORTANT — no edit or delete buttons for menu items. This is the staff view, read-only. Admin lives elsewhere. The Usage block is where we put the human factor — tap targets ≥ 40×40 pixels. That's a real accessibility line for staff using tablets all day.

**Show.** Highlight the Usage block.

**Watch.** If anyone asks "why no Material" — you'll address it on the takeaways slide.

---

## Slide 22 — Step 7c: Payment & Tip Component  (70:30 — 3 min)

**Say.** Three stages: review, processing, complete. The CRITICALs are all about input safety. Strip non-digits in Last 4. No submit on empty cart. No double-submit while a request is in flight. That third one is single-handedly responsible for a huge percentage of duplicate-charge bugs. We bake it into the prompt rather than discovering it in production.

**Show.** Read aloud all three CRITICAL items.

**Watch.** This is also where the front-end ties to the back-end's idempotency story (which we put in Next Steps).

---

## Slide 23 — Step 7d: Table Routing  (73:30 — 3 min)

**Say.** The kitchen view. Polls every five seconds. The CRITICAL — always clear the interval on destroy. Sounds tiny, but a forgotten interval on a kiosk that runs for weeks will eat memory. The IMPORTANT is interesting — we explicitly say no WebSockets or SignalR for the POC, and we mark the polling line with a TODO pointing at real-time as the upgrade path. That comment is a future prompt's `[U]` block in waiting.

**Show.** Point at IMPORTANT 2.

**Watch.** Someone will ask "why not WebSockets" — same answer as migrations. Next Steps.

---

## Slide 24 — Tiered Restrictions Applied  (76:30 — 4 min)

**Say.** Step back. Let's look at the tiered restrictions in aggregate across the whole project. CRITICAL — safety and correctness. Violations break the system or the business. No PANs, no cross-service DbContext, no hard-deletes, decimal.Round always. IMPORTANT — architecture and data integrity. Violations create tech debt. No AutoMapper, no rules in controllers, no EF entities in service models, no skipping the Facade. PREFERRED — style and polish. Violations are PR nits. XML docs, named constants, CancellationToken plumbing, mapping in its own file. The tiers help you AND the AI prioritize what matters most.

**Show.** Walk through each tier, slowly.

**Watch.** Mention: tiered restrictions are also how you write a code-review checklist. The mapping is one-to-one.

---

## Slide 25 — Mode Matrix  (80:30 — 3 min)

**Say.** Same SCRUB across five modes. Inline gets it as code comments. Chat gets the full bracketed notation. Agent gets full SCRUB plus directory restrictions and a build-verify. Claude Code Chat gets a Context Preamble — "I'm working on AspireCafe, an Aspire microservices POS, read src/AspireCafe.Menu.API/Managers/Facades/MenuFacade.cs for the pattern." Edit Mode gets SCRUB with B dominant. Same core. Mode-specific additions adapt it.

**Show.** Slow scan across the rows.

**Watch.** Question I always get: "which one is best?" — wrong question. They're for different jobs. Refer to the right column.

---

## Slide 26 — Edit Mode: Tightening ResolveTip  (83:30 — 3 min)

**Say.** Quick worked example. Imagine we're past the build. The Payments service is live. We realize we need to reject tips that are more than 200% of the subtotal — likely an input error. Edit Mode. The Scope is two surgical guards. The R restricts what changes to those two methods only. The B section — bolded, prominent — is what makes Edit Mode safe. ALL existing callers compile unchanged. Existing tests pass without modification. Method ordering stays exactly as written. Only ADD the guards. This is the kind of prompt you write in 45 seconds and the AI executes in 5 seconds, safely.

**Show.** Read the B block aloud.

**Watch.** This is also the structure for "add logging" or "add XML docs" — any add-only edit.

---

## Slide 27 — Diagnostic Card  (86:30 — 3 min)

**Say.** Output didn't match what you wanted. Don't rewrite the whole prompt. Diagnose. Use the card. Top **four** rows are real build breaks we caught and fixed during this session's preparation, highlighted in green. **Row 1**: AppHost runs but nothing starts — no dashboard, no resources, no errors. The S gap: missing `Properties/launchSettings.json`. AppHost is an ASP.NET Core exe; without launch profiles it has no URL to bind to and silently no-ops. **Row 2**: NETSDK1206 — Aspire workload deprecation; fix is the inline `<Sdk Name="Aspire.AppHost.Sdk"/>` element. **Row 3**: CS1061 — Facade method not found; S gap, names drifted between Controller and Facade; pin exact method names in Scope. **Row 4**: `verifySupportedTypeScriptVersion` throw — C gap, TypeScript version pin too old for Angular 20; fix is `typescript ~5.8.0` in package.json. The rest of the rows are the standard catalog. Each symptom maps to one letter. Fix that letter. Re-run.

**Show.** Walk down the table row by row. Pause especially on the top four highlighted rows — all four came from real builds during preparation.

**Watch.** This is the most-photographed slide. Pause long enough for screenshots. The silent-no-op row is the most insidious — there's no error message; the AppHost just doesn't do anything.

---

## Slide 28 — Custom Instructions  (89:30 — 3 min)

**Say.** Pre-loading. Drop these two files at the repo root and every subsequent prompt gets shorter. copilot-instructions.md handles C, R, U for Copilot. CLAUDE.md handles the same plus B for Edit Mode in Claude Code. Same conventions, same restrictions, same domain context — loaded once, applied everywhere. You stop repeating ".NET 10, no AutoMapper, no PANs" in every prompt. You write Scope, you write feature-specific R if needed, and you're done.

**Show.** Compare the two columns. Note that B only lives in CLAUDE.md.

**Watch.** Folks may ask if these get out of sync. Answer: keep them in source control with a CI check that compares the C/R/U sections.

---

## Slide 29 — LIVE DEMO 2: End-to-End  (92:30 — 10 min)

**Say.** Final demo. AspireCafe is running. We'll browse the seeded menu, build an order, route it to a table, tip it, and watch it land in the kitchen view — all while the Aspire dashboard shows traces flowing across three services.

**Show.**
1. Start at the POS tab. Click "Cold Brew" twice, "Croissant" once. Show the cart totals updating.
2. Type "7" into the Table # box.
3. Click "Proceed to Payment". Show the four tip presets — 15/18/20/25.
4. Tap 20%. Show the grand total updating.
5. Click Charge. Watch the spinner. Auth code appears.
6. Switch to the Orders tab. Wait up to 5 seconds. The order appears with status Submitted.
7. Click "Start Preparing". Then "Mark Ready". Then "Mark Delivered."
8. Switch to the Aspire dashboard. Show the trace — one span starts in POS, branches into Orders.API and Payments.API, hits both databases.

**Watch.** This is the moneymaker. Have it rehearsed. If the build fails live, fall back to a recording.

---

## Slide 30 — Iterative Refinement Loop  (102:30 — 3 min)

**Say.** Bad output isn't failure — it's signal. Five-step loop. Identify what's wrong. Diagnose which SCRUB letter failed. Refine — add the missing element, do NOT rewrite the whole prompt. Re-execute — Edit Mode for surgical fixes, full re-prompt for regeneration. Verify the fix didn't break something else. Each pass is small. The whole prompt rarely changes — usually just one letter.

**Show.** Step through the five circles.

**Watch.** This is the principle. The diagnostic card is the lookup table.

---

## Slide 31 — Build Your Prompt Library  (105:30 — 4 min)

**Say.** Take this home. Create a `prompts/` directory in your repo. Three subfolders — backend, frontend, cross-stack. Inside each, one markdown file per concern. Every file has the same five sections: When to Use, Best Mode, the SCRUB Template, Agent Mode Addition if applicable, Known Limitations. Treat prompts like code. Version control. PR review. Retire what doesn't work. Your three best prompts from today are the seeds. By the end of the sprint you'll have ten. By the end of the quarter, fifty.

**Show.** Tree on the left, five-section template on the right.

**Watch.** End by saying: "the markdown of these exact prompts is included in the conference repo. Don't write yours from scratch — fork mine."

---

## Slide 32 — Key Takeaways  (109:30 — 3 min)

**Say.** Eight takeaways. Read them in order:

1. SCRUB is a system, not a suggestion.
2. Restrictions is the star. What you exclude matters more than what you include.
3. Tiered restrictions help you AND the AI prioritize.
4. Layered SCRUB scales — each layer constrains the next.
5. Pre-load C, R, U, B in CLAUDE.md and copilot-instructions.md.
6. Mode-specific additions matter as much as the core.
7. Diagnose by element — don't rewrite, identify which letter failed.
8. Build a version-controlled prompt library.

**Show.** Walk down the list. Don't elaborate — they should speak for themselves.

**Watch.** Promise the audience: if they hold themselves to even four of these eight, they'll feel the difference inside a week.

---

## Slide 33 — Resources & Run It Locally  (112:30 — 3 min)

**Say.** Take-home. The repo includes the full solution, the SCRUB_PROMPTS markdown — that's every prompt I showed today, fully written — a NEXT_STEPS doc for the production hardening path, and SPEAKER_NOTES for you to re-run this session for your team. Two commands: set the SQL password as a user secret, then `dotnet run` against the AppHost project. The Aspire dashboard opens automatically. The POS is on localhost:4200. Swagger per API. Done.

**Show.** Read the two-command snippet aloud.

**Watch.** Mention QR code if you have one. Otherwise read the repo URL.

---

## Slide 34 — Closing Quote  (115:30 — 1 min)

**Say.** And we close where the Deep Dive deck closed. The best prompt engineer isn't the most clever — they're the most disciplined. They know what to exclude. Build AspireCafe, build something else — the discipline is the same. Thank you.

**Show.** Let it sit. Don't click forward.

**Watch.** Hold for applause and then take questions.

---

## Q&A Crib Sheet  (116:30 onward)

Likely questions and short answers:

**"Why not AutoMapper?"** — Setup time is fast, but every PR pays review tax checking the mappings still work. Hand-rolled is one file per entity, reviewed once.

**"Why not Material UI?"** — Adds 200kb to the bundle and a maintenance dependency. CSS variables and pure CSS handle it for a 5-screen POS.

**"Why one SQL container with three DBs instead of three containers?"** — Faster dev startup. Production keeps the per-service database pattern but on shared elastic pools.

**"Why no real Stripe / Square integration?"** — Slide 17 is the answer. POC simulates capture. Real PCI is in Next Steps.

**"Why polling instead of SignalR?"** — Scope. Polling works for the POC. The TODO in the code is the upgrade path. Slide 23.

**"Does SCRUB work with [Cursor / Cody / other tool]?"** — Yes. The core SCRUB letters are universal. Mode-specific additions adapt — pretty much every tool has Inline / Chat / Agent equivalents.

**"How do you keep CLAUDE.md and copilot-instructions.md in sync?"** — CI check that diffs the C / R / U sections. Or one source of truth that generates both.

**"How long did this take to build?"** — Two days of pair-prompting against the SCRUB framework. Without SCRUB it would have been five.

**"You showed CS1061 and NETSDK1206 on the diagnostic card — what were they?"** — Two real build breaks caught while preparing the session. CS1061: the Payments controller called `facade.GetPaymentsByOrderAsync` and `facade.GetTipSuggestions`, but AI had named the Facade methods `GetByOrderAsync` and `CalculateTipSuggestions`. Fix in the prompt was to pin Facade names explicitly in Scope and tell AI the Controller is canonical. NETSDK1206: AppHost csproj had `<IsAspireHost>true</IsAspireHost>`, which is the deprecated workload-era pattern. Fix is the inline `<Sdk Name="Aspire.AppHost.Sdk" Version="9.4.0" />` element. Both are caught by the diagnostic card if you don't notice them immediately.

**"Why didn't the SCRUB prompt prevent CS1061 the first time?"** — Honest answer: it didn't list the Facade method names explicitly. The S block said "build the onion stack" but trusted AI to keep the naming consistent across files. Now the prompt pins names and adds a CRITICAL restriction. That's the iterative refinement loop in action — diagnose the gap, refine the prompt, ship the better prompt.

**"What's the TypeScript version diagnostic row about?"** — `verifySupportedTypeScriptVersion` is a runtime check inside `@angular/compiler-cli`. Angular 20's peer-dependency range is `typescript >=5.8.0 <5.9.0`. When AI generated the package.json, it grabbed `~5.6.0` — a "safe" version it had seen in older Angular projects from training data. The build threw immediately on `ng build`. The fix in the prompt: explicitly pin `~5.8.0` in [C] AND add a CRITICAL in [R] forbidding older pins. Two letters, one bug fixed forever.

**"What's the 'AppHost runs but nothing starts' row about?"** — The AppHost executable is technically ASP.NET Core, and like any ASP.NET Core app it relies on `Properties/launchSettings.json` to know which URL to listen on, plus environment variables that tell DCP and the dashboard where to bind their endpoints. Without that file, `dotnet run --project AspireCafe.AppHost` returns immediately — no error, no dashboard window, no SQL container, nothing. AI generated everything except the launchSettings file because the C# code "looked complete." The prompt now lists launchSettings.json as a required deliverable in [S] and adds a CRITICAL in [R] explicitly forbidding skipping it.

---

## Timing Audit

| Section                                    | Cumulative |
| ------------------------------------------ | ---------- |
| Cover + Agenda + Quote + Why               | 0:00 – 4:00   |
| SCRUB Recap                                | 4:00 – 7:00   |
| Section divider + Solution structure       | 7:00 – 11:30  |
| Steps 1-3 (Bootstrap → ServiceDefaults)    | 11:30 – 23:30 |
| Onion architecture overview                | 23:30 – 27:30 |
| Step 4 (Menu microservice, four prompts)   | 27:30 – 40:30 |
| Step 5 (Orders)                            | 40:30 – 45:30 |
| Plausibility Trap                           | 45:30 – 50:30 |
| Step 6 (Payments)                          | 50:30 – 54:30 |
| **LIVE DEMO 1**                            | 54:30 – 64:30 |
| Step 7 (Angular, four prompts)             | 64:30 – 76:30 |
| Tiered restrictions + Mode matrix          | 76:30 – 83:30 |
| Edit Mode + Diagnostic card                | 83:30 – 89:30 |
| Custom instructions                        | 89:30 – 92:30 |
| **LIVE DEMO 2**                            | 92:30 – 102:30 |
| Refinement loop + Prompt library + Takeaways + Resources + Close | 102:30 – 116:30 |
| Q&A buffer                                 | 116:30 – 130:00 |

> If you're tight on time, the safe cuts (in order): trim DEMO 1 to 5 min, compress Step 7a-d to two slides (combine 21+22 and 23 mentions only), and skip slide 30 (Refinement Loop — it's covered implicitly in the Diagnostic Card).
