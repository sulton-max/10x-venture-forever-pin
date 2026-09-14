# Handoff — backend convention sweep, forever-pin side

> Current state: [engineering plan](engineering/planning/planning.md).
> Earlier tree status and test counts below are historical snapshots.

*Last updated: 2026-08-19*

> Rows the settled backend conventions imply for this repo. The SDK-side list lives in
> `wow-two-sdk.backend.beta/be-convention-sweep.md`; this file carries only what forever-pin changes.

## Status

⬜ nothing done. Conventions are still settling in `wow-two-ws/conventions/development/backend/dotnet/`.

---

## Host configuration

Settled 2026-08-19: a host `Add*` names a **subject**, never a layer
(`host-configuration.md` § *Naming a registration method*).

| # | Change | Where |
|---|---|---|
| H1 | `AddPersistence()` → `AddPostgresDatabase()` | `ForeverPin.Api/` + `ForeverPin.Redirect.Api/Configurations/` |
| H2 | `AddApplicationServices()` dissolves — mediator wiring to `AddMediator()`, `ICodeRepository` + `ISlugGenerator` to `AddCodes()` | `ForeverPin.Api/Configurations/HostConfiguration.Extensions.cs` |
| H3 | `AddCodeServices()` → `AddCodes()`; `AddRoutingServices()` likewise | both hosts |
| H4 | `Program.cs` is three statement groups, one comment each — verify both hosts | both hosts |

- the ban is on layer names, not on shared resources: `AddPostgresDatabase()` is fine, `AddPersistence()` is not,
  because persistence spans the database, file storage and caching and no single call carries the word.

---

## JSON seams

`CodeContentJson` and `CodeRuleJson` each hold a cached `JsonSerializerOptions` plus two one-line calls into
`JsonSerializer`. The serialization itself is `System.Text.Json`; the wrapper adds only the options binding
and a null/blank guard.

| # | Question | Where |
|---|---|---|
| J1 | Delete the per-type seam class once J2 lands — call sites use the SDK serializer | `ForeverPin.Domain/Codes/{Content,Rules}/` |
| J2 | The beta SDK **must** own this shape — a serializer holding its options in a type-keyed dictionary | `ForeverPin.Common/` → beta SDK |

- `JsonbOptions` lives in `ForeverPin.Common.Domain.Serialization.Json` — an SDK-extraction basket, so the lift
  is already staged.
- `JsonbOptions.Create(...)` builds a *new* `JsonSerializerOptions` per call, and `System.Text.Json` caches
  type metadata per options instance — which is the only reason a `static readonly` field exists today.
- a type-keyed options dictionary inside the SDK serializer removes that reason: the cache lives in the
  registry, so no product declares a static field, and the per-type class has nothing left to hold.
- J1 is therefore a deletion, not a redesign — it waits on J2.

---

## Carried from the earlier handoff

Never resumed, still open — see `wow-two-ws/be-sweep-handoff.md` § *forever-pin*:

1. Backend code sweep to the settled starters — `Repository` · `Controller` · `Broker` · `Client` summaries,
   51 enum members (`Represents` → `Refers to`), ~20 validator constructors losing their `=>`, field starters.
2. Iteration 8 sub-step (c) — member names BE↔FE, the unreproduced calendar defect.
3. Iterations 9 · 10 · 11 · 12 — mode-lock UX, validation sweep, print export, SDK converters.
4. Iteration 1 holds 7 undecided forks; three gate CM9 — **F3** the resolve-page host, **CM16** the per-type
   resolve path, and whether **`geo`** takes the Page path.
5. `IGeoBroker` uses `NoopGeoBroker`, so country routing cannot match; language routing is wired.

---

## Don't

- **Don't rename across the tree from this file alone** — each row cites the convention that settled it;
  read that section before acting, because the conventions are still moving.
- **Agents never commit.** Stage one cohesive change, print the message, stop.
