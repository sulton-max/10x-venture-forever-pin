# Content model

*Last updated: 2026-09-13*

## Invariants

- A code has a mode, a content type, a style, and one or more rules.
- A rule carries content; every rule's content matches the code's content type.
- Static mode bakes one rule's payload. Dynamic mode bakes a short link.
- Mode is chosen at create and omitted from update requests.
- A mode change creates a new code; printed symbols cannot change their bytes.
- Slugs and short URLs exist only for dynamic codes.
- Static scans bypass the service, so server scan analytics cannot measure them.

---

## Decisions

| ID | Contract |
|---|---|
| CM1 | Mode belongs to the code, not its content type |
| CM2 | A static code has exactly one rule; multiple rules require dynamic mode |
| CM3 | Mode locks at creation |
| CM4 | Neither direction permits an in-place mode flip |
| CM5 | A mode change copies into a newly created code |
| CM6 | Explain baked content when static mode is chosen |
| CM7 | Display mode separately from content type |
| CM8 | Dynamic delivery is intended for every content type |
| CM9 | Non-URL dynamic payloads need a minimal resolve page |
| CM10 | Static-content version history is deferred; its seam remains open |
| CM11 | Mode-specific paywall design is deferred |
| CM12 | Keep a short slug distinct from the internal entity ID |
| CM13 | Only dynamic creation allocates a slug |
| CM14 | Phone, SMS, and email use a page with an explicit action |
| CM15 | Content belongs inside each rule |
| CM16 | Delivery path is per content type; the mapping remains unresolved |
| CM17 | Static codes bypass the redirect service |

CM2 is enforced during create; update still needs the persisted mode.
CM9 and CM14 are design requirements, not completed delivery features.

---

## Rule roles

- `ConditionalRuleValueObject`: `Order`, `Condition`, `ConditionValue`, `Content`.
- `DefaultRuleValueObject`: dedicated fallback content.
- `DefaultPointerRuleValueObject`: `TargetOrder` of a conditional rule.
- At most one default role exists. A pointer must target a conditional rule.
- Without a fallback, an unmatched scan may return `404`.
- Reordering updates order and pointer references atomically.
- Rules persist together in the `codes.rules` jsonb document.
- Nested subtype registries bind rule and content discriminators explicitly.

A stable rule GUID was rejected: a pointer targets the order within one atomically saved rule set.
The default-role invariant prevents pointer chains and cycles.

---

## Content and wire

- Ten types: URL, text, phone, SMS, email, geo, calendar, WiFi, vCard, mobile app.
- Phone values remain strings to preserve `+`, leading zeros, and extensions.
- Geo coordinates are numeric; range validation applies.
- Calendar uses backend `DateTime` and frontend `Temporal.PlainDateTime` without a timezone.
- WiFi encryption is explicit; password requirements depend on encryption.
- Mobile app content retains a store and URL. Store is not the scanner's device.
- Backend value-object names and frontend wire names need not match.
- Member names and optionality must match their wire contract.
- Style is required on create, update, and preview; logo/gradient/emoji are optional.
- Update omits mode at serialization; the edit form still carries it.
- Preview uses mode and rules; it has no separate client-supplied payload channel.

---

## Open decisions

- F3: serve minimal HTML from the redirect host or redirect to the SPA.
  - Redirect-host HTML avoids a second hop and frontend bundle loading.
  - SPA delivery shares presentation infrastructure but adds those costs.
  - The recorded preference is redirect-host HTML; no owner decision closes it.
- CM16/F6: classify every content type as web redirect or page delivery; resolve geo explicitly.
- Presets: define mobile-app presets and whether geo needs the same abstraction.
- CM10: define the seam for later static-content version history.
- Pointer analytics: approve which rule is reported; current code records the target order.
- Conditions: retain the enum/value shell or introduce polymorphic conditions.
- vCard: decide whether one contact method is required.
- Rendering: decide the SDK payload contract without referencing product entities upstream.

The older nullable-encoding and content-as-sibling alternatives are obsolete.
The current content base retains polymorphic encoding; one code-level payload resolver selects what to bake.

---

## UX contracts

- Copy opens one dialog asking for target mode and showing restrictions.
- A dynamic-to-static copy warns that routing rules are dropped.
- Static edit disables Add a routing rule with an explanation.
- Single-rule and multi-rule layouts use consistent containers and actions.
- Removing the only rule stays disabled with the minimum-rule explanation.
- Control explanations and the union-aware field-array contract remain open.

Validation ordering and persistence probes: [validation analysis](../planning/validation.md).
Implementation tasks: [v0.11](../planning/version-track/v0.11/v0.11.md).
