# ForeverPin

*Last updated: 2026-09-13*

## Product

**Pin it once. It points forever.**

Generate styled QR codes and barcodes, or create a programmable short link that can change its destination after printing.
A subscription downgrade must not deactivate an existing redirect.

---

## Audience

- Small businesses with printed surfaces: menus, signage, packaging, and events.
- Developers seeking programmable redirects without enterprise sales.
- Small agencies managing client codes and campaigns.

Enterprise governance and payment rails are outside the current launch scope.

---

## Pricing boundary

| Plan | Current display | Implemented creation cap |
|---|---|---|
| Free | $0 | 3 codes |
| Solo | $5/month | 25 codes |
| Pro | $15/month | 200 codes |
| Dev/Agency | $39/month | Unlimited |

The cap currently counts static and dynamic codes together.
Custom domains, customer analytics, advanced routing, API access, and agency workspaces are not implied by these rows.

Marketing proposes unlimited free static generation and a $12/year entry experiment.
The launch pricing and free dynamic allowance remain open; neither proposal is implemented by this document.

---

## Principles and validation

- Preserve printed redirects across plan changes.
- Avoid advertising and unsolicited scan notifications.
- Make ownership and export behavior explicit.
- Validate paid demand before expanding the roadmap.

Portfolio checkpoints: first paying customer by week 4; $100 MRR by month 3;
$500 by month 6; $2,000 by month 12. These are targets, not measured results.

Market research: `wow-two-ws/ideas/forever-pin-spec.md`.
Current state: [context](context.md). Future capabilities: [backlog](../engineering/planning/backlog.md).
