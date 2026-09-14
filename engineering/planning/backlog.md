# Backlog

*Last updated: 2026-09-13*

Active-release tasks live in [v0.9](version-track/v0.9/v0.9.md).
This list holds unshipped work outside that release; order is pull priority.

## Launch and product alignment

- [ ] Verify real Google sign-in with the configured OAuth client
- [ ] Verify Stripe test-mode checkout, portal, and webhooks
- [ ] Decide the free dynamic allowance and entry pricing
- [ ] Align static-code limits with the chosen pricing model
- [ ] Reconcile marketing claims with implemented capabilities
- [ ] Define guest-subscription merging across accounts
- [ ] Configure and verify deployment, TLS, and redirect domains
- [ ] Verify database backup and restore
- [ ] Measure redirect bursts and scan-queue loss
- [ ] Add destination screening and abuse controls
- [ ] Add reporting and takedown handling
- [ ] Verify brand clearance and social handles
- [ ] Validate the never-deactivate value proposition with users

---

## Accounts

- [ ] Design lost-cookie recovery with proof of ownership
- [ ] Refresh stored Google profile details on login

Guest accounts, Google login, guest-code claiming, and cross-device ownership already exist.

---

## Routing and analytics

- [ ] Activate country lookup
- [ ] Add customer analytics and aggregation endpoints
- [ ] Decide total-versus-unique scan semantics without fingerprinting
- [ ] Add routing cache with edit invalidation
- [ ] Complete the Redis configuration read/write path
- [ ] Add per-code timezones and scheduled windows
- [ ] Add A/B weights and grouped conditions
- [ ] Add language/referrer/UTM routing depth where required

The redirect remains plan-agnostic. No scan limit is enabled by a subscription tier.

---

## Domains and link controls

- [ ] Add custom-domain ownership, TLS, and host-aware resolution
- [ ] Design password-protected content delivery
- [ ] Add explicit expiry, scan caps, and one-time links

Expiry and passwords need complete authoring and delivery flows; dormant database columns are insufficient.

---

## Styling and files

- [ ] Add file upload and logo controls
- [ ] Add PDF export
- [ ] Research frames and call-to-action captions
- [ ] Add versioned style upgrades at the first breaking change
- [ ] Complete shape/eye controls and remaining color-panel polish
- [ ] Explore image-like QR rendering without reducing scan reliability
- [ ] Add animated exports if the product demand supports them

SVG/PNG download, module/finder shapes, gradients, and emoji already exist.

---

## Content and developer features

- [ ] Add static-content version history
- [ ] Add bulk generation and serialized payloads
- [ ] Add public API keys and developer endpoints
- [ ] Add scan webhooks and exports
- [ ] Add agency workspaces and white-label controls
- [ ] Evaluate GS1 Digital Link
- [ ] Integrate hosted editable content pages as a separate product
- [ ] Evaluate NFC pairing and a self-host edition

Minimal dynamic-content delivery belongs to v0.9; a designed hosted-page editor does not.

---

## Marketing

- [ ] Finish the parked hero experiment and mobile review
- [ ] Verify branded promo exports visually
- [ ] Build the free-generator entry page
- [ ] Add content-type and comparison landing pages
- [ ] Add sitemap and crawlable content delivery
- [ ] Prepare short-form launch content
- [ ] Add a privacy-first acquisition funnel
- [ ] Evaluate creator integrations and wedding/event channels

---

## Engineering follow-ups

- [ ] Reconcile frontend naming forks against settled conventions
- [ ] Complete backend convention adoption
- [ ] Finish the remaining polish-track tasks
- [ ] Simplify E2E DTO mirrors while retaining raw-JSON assertions
- [ ] Review dependency vulnerability warnings with the SDK lane
- [ ] Establish product CI/CD and a repeatable deployment configuration
- [ ] Verify the reserved SDK-adoption scope before assigning a release

Vue SDK readiness and application migration stay in [their handoff](../../vue-port-handoff.md).
No SDK publication or migration completion is inferred from its old snapshots.
