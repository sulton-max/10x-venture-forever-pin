# ForeverPin — Engineering planning

*Last updated: 2026-09-13*

## Versions

| Version | Delivered scope | State |
|---|---|---|
| v0.1 | Foundation, guest ownership, management | Complete |
| v0.2 | SQL migrator adoption | Complete |
| v0.3 | Google accounts and guest claiming | Complete |
| v0.4 | Backend and frontend SDK adoption | Complete |
| v0.5 | Styling and server preview | Complete |
| v0.6 | Rendering SDK extraction | Complete |
| v0.7 | Static content, barcodes, downloads | Complete |
| v0.9 | Static/dynamic content and delivery | In progress |

Release detail: [version track](version-track/version-track.md).
The reserved `v0.8` adoption slot has no delivery record.
The hero experiment is not a later active release.

---

## Current work

1. Finish the remaining model review in v0.9 iteration 8.
2. Implement the mode-lock and copy-dialog contracts in iteration 9.
3. Complete load-aware validation, write guarantees, and service boundaries in iteration 10.
4. Resolve the content-delivery decisions and implement the minimal delivery path.
5. Complete print guidance and verify the user flows.

[Current-work analysis](current-work.md) explains dependencies and verified gaps.
Detailed checkboxes live only in [v0.9](version-track/v0.9/v0.9.md).
Open decisions do not block independent model review.
Vue migration and SDK publication remain separate work; readiness is not assumed from an expected date.

---

## Parallel records

- [Polish](polish-track/p0.1/p0.1.md): presentation cleanup and test naming.
- [Backlog](backlog.md): launch gates and later capabilities.
- [Product context](../../product/context.md): business state and pricing decisions.

---

## Architecture and evidence

- [Content contract](../architecture/content-model.md): mode, rules, wire, and delivery decisions.
- [Routing](../architecture/routing-engine.md): current redirect behavior.
- [Billing](../architecture/billing.md): provider integration and plan enforcement.
- [Verification](../operations/verification.md): current local results and unverified external gates.
- [Rebrand](../operations/rebrand.md): names, compatibility exceptions, and remote status.
