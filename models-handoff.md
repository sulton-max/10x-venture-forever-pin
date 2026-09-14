# Handoff — forever-pin FE models + enums remainder

> Historical analysis: implementation and active task status are recorded in [the engineering plan](engineering/planning/planning.md).

*Last updated: 2026-07-04*

> Tail of the SDK migration (`v0.7` iteration 6). The wire flip is **done + green**; these are the
> frontend-convention conformance items deliberately deferred out of that pass. Self-contained — a
> fresh session can execute the "do-now" batch without re-analysis.

---

## Context — already done, do NOT redo

- SDK migration complete: `@wow-two-beta/ui@0.0.70` + `WoW2.Sdk.Backend.Beta@10.0.42-beta`.
- Envelope (`ApiResponse`/`ApiError`/`ProblemDetails` from `@wow-two-beta/ui/http`), Temporal dates
  (`parseJson` reviver → `createdAt: Temporal.Instant`), camelCase enums both sides — all shipped.
- Green: FE typecheck + vitest 9 · Unit 107 · Integration 18 · E2E 68 · Migrations 10.
- Full record: `engineering/planning/version-track/v0.7/v0.7.md` § Iteration 6.
- Convention sources: `wow-two-ws/conventions/development/frontend/code-style/{models,type-mapping,enums}.md`.

Paths below are relative to `engineering/codebase/forever-pin.frontend-services/src/`.

---

## A. Do-now batch — quick, low-risk, no backend/E2E dependency (~1 FE pass)

### A1. Nullability `| null` → `?` (7 fields, `types/index.ts`)

Convention: never `T | null`; optional = `field?: T`; the backend omits null keys.

| Field | Line | Side | Note |
|---|---|---|---|
| `CodeDto.content` | `:173` | read | pure `?` swap — received, never built |
| `CodeDto.rules[].conditionValue` | `:191` | read | `?` swap |
| `Me.user` | `:211` | read | `?` swap |
| `PreviewStyle.logo` | `:159` | write | see below |
| `PreviewStyle.gradient` | `:163` | write | see below |
| `PreviewStyle.emoji` | `:164` | write | see below |
| `PreviewRequest.content` | `:197` | write | + `components/QrPreview.tsx:10` |

- Read-side (3) = zero-risk `?` swaps.
- Write-side (4): the wire currently *sends* explicit `null`; convention = omit the key. Backend
  `StyleApiRequest.Logo?/Gradient?/Emoji?` are already optional → omitting is compatible.
  - Build site: `screens/CreateCodeScreen.tsx:153` (`logo: null`) + `useState<PreviewGradient | null>` `:76`
    + `useState<PreviewEmoji | null>` `:78`. Internal React state can STAY `| null` (idiomatic); only
    the **wire model type** + the serialized body must drop null. Strip nulls at the build boundary
    (conditional spread, e.g. `...(logo && { logo })`) or type the wire model `?` and omit on build.

### A2. Collections `T[]` → `ReadonlyArray<T>` (1 field)

- `PreviewGradient.stops: PreviewGradientStop[]` → `ReadonlyArray<PreviewGradientStop>` (`types/index.ts:144`).
- Built mutably in the gradient editor — the `ReadonlyArray` field still accepts a mutable array (covariant). Trivial.

### A3. Enum `Unresolved` first member — 3 domain enums

Convention (`enums.md`): a domain enum must include `Unresolved: "unresolved"` as its **first** member
(defensive fallback for bad/future wire data), plus a `{Enum}Labels` entry, and map unknown wire → `Unresolved` (+log).

- **Add to** `BarcodeFormat` · `RuleConditionType` · `Plan` (`types/index.ts`) — first member + `{Enum}Labels[X.Unresolved]`.
- **Exempt** (UI value-sets / style tokens): `ModuleShape` · `FinderShape` · `GradientType`.
- **Convention-gray** (string unions, not const-objects, not displayed): `CodeType` · `UserKind` — leave, or promote to const-object domain enums if strict conformance wanted.
- **Dropdown leak — exactly 2 sites** use `Object.values(<domain enum>)` → add `.filter(v => v !== X.Unresolved)`:
  - `screens/CreateCodeScreen.tsx:314` (`BarcodeFormat`)
  - `components/RuleBuilder.tsx:83` (`RuleConditionType`)
  - (`Plan` dropdowns use `PAID_PLANS` explicit → no leak.)
- **Fallback-on-read guard** — reads are direct identity now; add a small `parseEnum(values, wire) ?? X.Unresolved`
  at the ~3 domain-enum read sites: `CreateCodeScreen.tsx:103` (`setSymbology(code.barcodeFormat)`),
  `toDrafts` conditionType (`CreateCodeScreen.tsx:35`), `BillingScreen.tsx:120` (`billing?.plan ?? Plan.Free`).
  (This is what `enumFromWire` was, minus the case-insensitive bridge — now the wire is exact camelCase.)

> **Value note:** `Unresolved` is defensive-only. Real, but low payoff for a POC where the same team owns
> both sides and camelCase is enforced. Fine to skip until the FE-architecture pass (§B) picks it up.

### Verify (A)

```bash
cd engineering/codebase/forever-pin.frontend-services
pnpm typecheck && pnpm test
```

No backend rebuild needed — §A is FE-only and wire-compatible.

---

## B. Defer batch — the FE Clean-Arch migration (own track, not this handoff)

Large, cross-cutting, **no wire benefit** — do it as its own version/effort, not folded into the SDK-sync tail.

- **Bare-model renames** (no shape mismatch → drop `Dto`): `CodeDto`→`Code` · `SessionUrlDto`→`SessionUrl`
  · `LimitsDto`→`Limits` · `UsageDto`→`Usage`. Ripples every import + `readData<CodeDto>` call site.
- **One-type-per-file + slice barrels**: 37 exports in one `types/index.ts` → `domain/{slice}/` + `integration/`
  per `conventions/development/frontend/architecture/architecture.md`, one type per `PascalCase.ts` + lowercase `index.ts` barrels.

---

## Related open threads — tracked elsewhere (pointers, not this handoff)

- **Other 3 SDK-migration consumers** — drydock · secrets-vault · sift. Untouched. Per-consumer checklist +
  targets table already in the UI repo: `wow-two-sdk-beta.ui/sdk-migration.md`.
- **SDK consistency** — `Testing/ApiContracts` + `Codes/StyleSpecJson` still use PascalCase converters
  (tolerant/storage-side). Spawned as a task chip; each needs its own SDK push.
- **v0.7 features** — barcodes UI (iter 2) · download/print export (iter 3) · validation + info popovers (iter 4).
  Tracked in `v0.7.md`; untouched by the migration.
- **Uncommitted tree** — this session's SDK-migration changes + another lane's polish-track `ColorPicker` edit
  coexist in `CreateCodeScreen.tsx`. Nothing committed (git is the developer's). Commit per lane.
