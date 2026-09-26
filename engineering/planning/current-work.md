# Current work

*Last updated: 2026-09-26*

## Position

`v0.10` is the active SDK adoption release. `v0.11` is the owner's planned feature track.
The owner accepted `v0.9`'s implemented models, storage, authoring, and basic copy milestone.
That scope acceptance is not a claim that unresolved product correctness or manual checks passed.
Promo and marketing remain parked, including the [hero experiment](backlog.md#parked-hero-experiment).

---

## Ownership

| Lane | Owns | Completion evidence |
|---|---|---|
| Frontend migration | Private pnpm root, `apps/web`, Vue SDK, product contracts | Automated checks, live runtime smoke, visual comparison |
| Backend migration | Backend SDK and its required product integration | Separate chat's verified migration and regression results |
| Owner | Product acceptance and release completion | Explicit acceptance of both lanes and manual checks |

The [v0.10 checklist](version-track/v0.10/v0.10.md) remains open while backend adoption is unfinished.
Frontend work does not change backend implementation or infer the other chat's progress.

---

## Adoption order

1. Adopt a private pnpm root at `forever-pin.frontend-services/`, with the product in `apps/web`.
2. Adopt `@wow-two-beta/ui-vue@0.0.7` and the SDK API, form, session, and query contracts.
3. Preserve product routes, content editing, preview, downloads, and existing ownership behavior.
4. Run frontend automated checks and product runtimes; compare existing flows and visuals.
5. Integrate backend migration evidence and obtain owner acceptance for the combined result.

The [frontend adoption analysis](../architecture/frontend-adoption.md) records the full source audit,
prioritized defects, migration boundaries, and visual candidate.
SDK publication establishes availability; product adoption needs its own integration evidence.
The migration preserves the accepted product scope without treating known defects as acceptance criteria.
Optional `packages/` hold code shared by multiple apps; module federation remains outside this adoption.

---

## Planned feature scope

The [v0.11 checklist](version-track/v0.11/v0.11.md) owns every unfinished content-model,
mode/copy, validation, print, converter, delivery, and manual-verification task.
The five manual flow checks remain unchecked and await the owner's actual pass.
Known gaps and their evidence remain in the [gap analysis](gap-analysis-2026-09-19.md).

Before exposing affected capabilities, verify payload correctness, static-mode invariants,
pointer preservation, safe dynamic-content delivery, and concurrent-write guarantees.
Version labels do not remove those release gates.

---

## Contracts and decisions

The [content model](../architecture/content-model.md) owns mode, wire, and delivery contracts.
The [validation analysis](validation.md) owns validation ordering and persistence probes.
Resolve-page hosting, per-type delivery, calendar reproduction, and print acceptance remain open.
Backend adoption may satisfy individual feature tasks; reconcile their evidence with the owning chat
before changing their checklist state.

---

## Planning boundary

The owner authorized `v0.11` as a future feature track while `v0.10` is active.
This explicit scope overrides the default latest-folder and single-next-version planning rules.
No `v0.11` implementation or manual pass is implied by creating its plan.
