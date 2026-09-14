# Documentation audit

*Last updated: 2026-09-13*

## Result

The current documentation uses ForeverPin and the renamed source paths.
Release checklists now describe outcomes and remaining work; architecture documents own the durable contracts.
Historical research and review evidence remain identifiable as historical.

---

## Sources of truth

| Question | Document |
|---|---|
| What runs and how is it verified? | [Development](../development/development.md), [verification](verification.md) |
| What is implemented and what remains? | [Current work](../planning/current-work.md) |
| Which tasks are active? | [v0.9](../planning/version-track/v0.9/v0.9.md) |
| Which capabilities are deferred? | [Backlog](../planning/backlog.md) |
| What contracts guide the implementation? | [Architecture](../architecture/architecture.md), [content model](../architecture/content-model.md) |
| Which old names must remain? | [Rebrand](rebrand.md) |

---

## Sweep

| Area | Changes |
|---|---|
| Entry points | Compact README, repository instructions, codebase map, and lazy document index |
| Architecture | Current routing, rendering, billing, React structure, persistence, and scan collection; extracted content-model decisions |
| Release tracks | Compacted v0.1–v0.7; retained recorded completion and manual-check state |
| Active release | Compacted v0.9; retained all 36 previously open top-level tasks, closing only backend verification with fresh evidence |
| Delivery | Added explicit non-URL delivery and stale test-name tasks; retained owner manual checks |
| Hero experiment | Marked v0.11 parked; kept it separate from the active product release |
| Polish | Consolidated duplicate mode/copy work into v0.9; corrected obsolete request-model proposals |
| Product and marketing | Distinguished current prices/capabilities from proposed positioning and launch claims |
| Operations | Recorded native permission workflow, executed tests, rename compatibility, and completed remote rename |
| Research and handoffs | Retained useful evidence and proposals with historical/current boundaries; refreshed code links |
| Naming | Rebranded prose, examples, filenames, source references, launch scripts, and promo compositions |

The old planning decision table survives in [historical decisions](../architecture/decisions.md).
Detailed research remains under `engineering/research/` and `product/analysis/`.
Neither a historical verdict nor a renamed review link marks a review complete.

---

## Open boundaries

- v0.9 remains active; the owner has not completed its manual verification.
- Application Vue migration is excluded from this work. The other chat owns SDK readiness.
- Pricing, resolve-page hosting, per-type delivery, and the SDK read seam remain decisions or dependencies.
- Live OAuth, Stripe, deployment, public domain continuity, and physical scanner checks remain unverified.
- GitHub rename and both local remote URLs are complete; see [rebrand](rebrand.md).

---

## Recovery

Original changed-file bytes and path mappings were saved before the rename in
`/private/tmp/forever-pin-rename-gmc40zz9/manifest.json` and its sibling backup files.
This is a local temporary recovery copy, not a durable archive.
Applied SQL migrations retain their original bytes.
