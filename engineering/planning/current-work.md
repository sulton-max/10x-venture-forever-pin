# Current work

*Last updated: 2026-09-15*

## Position

The product is in v0.9. Its models, storage, forms, and basic copy flow are implemented.
Automated verification is green; the delivery and correctness gaps below remain real.
Vue application migration is excluded from this lane.
Promo and marketing work, including the promo source's repository placement, is parked by the owner.

---

## Execution order

| Order | Work | Dependency |
|---|---|---|
| 1 | Model review remainder | Ready; separate name and comment batches |
| 2 | Static edit and copy UX | Model review; existing SDK controls |
| 3 | Validation and write guarantees | Settled read/service seam and concurrency contract |
| 4 | Dynamic-content delivery | Resolve-page host and per-type delivery decisions |
| 5 | Print guidance and manual flow checks | Final rendering/delivery behavior |

The [v0.9 checklist](version-track/v0.9/v0.9.md) is the task source.
This document explains ordering and evidence, not a second checklist.

---

## Model review

- Completed: content/rule shapes, persistence, type renames, and the completed summary pass.
- Reviewed: the four backend entity types and their member summaries; comment-only changes preserve declarations.
- Remaining: other type/member summaries, backend/frontend member names, and consistent frontend DTO names.
- Calendar: the old defect report lacks a reproduction; reproduce before changing encoding.
- Preserve frontend wire-role names rather than copying backend value-object suffixes.

---

## Mode and copy correctness

- Current copy flow prefills a new code; it does not mutate the source's mode.
- The approved single Copy dialog and target-mode explanations are unbuilt.
- Static edit still offers an additional routing rule.
- Update validation receives no mode, so it cannot enforce the static one-rule constraint.
- Fix the UI guard and server guarantee together; a UI-only restriction is insufficient.

---

## Validation and writes

- Creation has content, rule, and whole-set validation.
- Update needs the loaded entity for mode-dependent invariants.
- Existence and ownership must precede deeper input checks.
- Check-then-write probes need concurrency guarantees.
- Slug creation avoids existing values but still has a check/insert race.
- Preserve load → attach → mutate when using a tracked write seam.
- The product should consume the agreed SDK contract rather than duplicate it.

---

## Delivery decisions

- The resolver currently redirects any nonempty encoded payload.
- Dynamic WiFi, vCard, calendar, phone, SMS, email, text, and geo require an explicit delivery policy.
- The recorded preference is small HTML from the redirect host; the owner has not closed that choice.
- Resolve the per-type map, including geo, before implementing the page path.
- Keep the minimal page separate from a future hosted-page editor product.

Full decision IDs and invariants: [content model](../architecture/content-model.md).

---

## Independent work

- Backend convention adoption and small presentation polish can remain separate batches.
- Pricing/marketing reconciliation is a product decision after mode behavior is stable.
- Country lookup, analytics, custom domains, and launch operations remain outside the current core implementation.
- SDK Vue readiness must be confirmed by its owning chat and actual release evidence.

---

## Boundaries

- No current version or manual verification was marked complete by this sweep.
- No SDK implementation or application migration was performed.
- The user committed the rebrand and verification tooling; documentation is a separate batch.
- GitHub rename and both local remote URLs are complete. See [rebrand](../operations/rebrand.md).
