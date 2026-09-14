# Routing engine

*Last updated: 2026-09-13*

## Evaluation

1. An inactive code returns `NotFound`.
2. Conditional rules run in ascending `Order`; the first match supplies content.
3. A dedicated default supplies its content; a pointer default uses its target's content.
4. No match and no default returns `NotFound`.
5. The current resolver encodes content into a redirect destination.

There is no subscription, expiry, password, or scan-limit gate on this path.
The never-deactivate promise concerns plan changes; an owner can deactivate a code.

---

## Conditions

| Condition | Source | Behavior |
|---|---|---|
| Device | User-Agent | Device mapper |
| Country | `IGeoBroker` | No country while `NoopGeoBroker` is registered |
| Language | Accept-Language | First primary language tag |
| TimeOfDay | UTC clock | Daily window; supports midnight wrap |

The endpoint constructs `ScanContext`; `RoutingService` evaluates without I/O.
`Default` is a rule role, not a condition.

---

## Content delivery

Static symbols encode the payload directly and do not reach this service.
Dynamic symbols encode the short link.

Dynamic WiFi, calendar, vCard, and other non-web payloads need a resolve page.
The current implementation redirects any nonempty encoding; it does not yet distinguish page delivery.
Resolve-page decisions and mode invariants live in [content model](content-model.md).

---

## Source

- `ForeverPin.Redirect.Api/Endpoints/RedirectEndpoints.cs`
- `ForeverPin.Redirect.Api/Infrastructure/Routing/RoutingService.cs`
- `ForeverPin.Domain/Codes/Rules/Models/`

Paths are relative to `engineering/codebase/forever-pin.backend-services/`.
