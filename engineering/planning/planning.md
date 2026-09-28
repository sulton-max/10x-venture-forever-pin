# ForeverPin — Engineering planning

*Last updated: 2026-09-28*

## Versions

| Version | Scope | State |
|---|---|---|
| v0.1 | Foundation, guest ownership, management | Complete |
| v0.2 | SQL migrator adoption | Complete |
| v0.3 | Google accounts and guest claiming | Complete |
| v0.4 | Backend and frontend SDK adoption | Complete |
| v0.5 | Styling and server preview | Complete |
| v0.6 | Rendering SDK extraction | Complete |
| v0.7 | Static content, barcodes, downloads | Complete |
| v0.9 | Content models, persistence, authoring, and basic copy | Complete |
| v0.10 | Frontend and backend SDK adoption | In progress |
| v0.11 | Content correctness, delivery, copy UX, and print verification | Planned |

Release detail: [version track](version-track/version-track.md).
The reserved `v0.8` adoption slot has no delivery record.
The owner explicitly authorized the planned `v0.11` track alongside active `v0.10`.

---

## Current work

1. Publish backend SDK `10.0.60-beta` from `80aeb63` or later; verify the pin without the candidate feed.
2. Publish `@wow-two-beta/ui-vue`, then drop the product's antiforgery and warning bridges.
3. Compare the migrated product visually; runtime smoke and browser checks already pass.
4. Take the `v0.11` Iteration 1 decisions that gate non-URL content delivery.
5. Keep `v0.10` open until owner acceptance of both adoption lanes.

[Current work](current-work.md) defines ownership, dependencies, and acceptance boundaries.
Task checkboxes live in [v0.10](version-track/v0.10/v0.10.md) and [v0.11](version-track/v0.11/v0.11.md).
The completed `v0.9` milestone records implemented scope; it does not certify public readiness.

---

## Parallel records

- [Polish](polish-track/p0.1/p0.1.md): presentation cleanup and test naming.
- [Backlog](backlog.md): launch gates, parked hero work, and later capabilities.
- [Product context](../../product/context.md): business state and pricing decisions.

---

## Architecture and evidence

- [Content contract](../architecture/content-model.md): mode, rules, wire, and delivery decisions.
- [Routing](../architecture/routing-engine.md): current redirect behavior.
- [Billing](../architecture/billing.md): provider integration and plan enforcement.
- [Verification](../operations/verification.md): local results and unverified external gates.
- [Rebrand](../operations/rebrand.md): names, compatibility exceptions, and remote status.
