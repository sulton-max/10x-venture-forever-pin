# Backlog

*Last updated: 2026-09-26*

Active adoption tasks live in [v0.10](version-track/v0.10/v0.10.md).
Planned feature tasks live in [v0.11](version-track/v0.11/v0.11.md).
This list holds work outside those tracks; order is pull priority.

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

Minimal dynamic-content delivery belongs to `v0.11`; a designed hosted-page editor does not.

---

## Marketing

- [ ] Verify branded promo exports visually
- [ ] Build the free-generator entry page
- [ ] Add content-type and comparison landing pages
- [ ] Add sitemap and crawlable content delivery
- [ ] Prepare short-form launch content
- [ ] Add a privacy-first acquisition funnel
- [ ] Evaluate creator integrations and wedding/event channels

---

## Parked hero experiment

Parked outside the release sequence. Concept and specification:
[interactive landing hero](../../product/marketing/landing-hero-concept.md).
The recorded implementation state is retained; the remaining work is not part of SDK adoption.

- [x] Add a `<HeroSim>` canvas behind the landing hero.
- [x] Bake real scannable QR card sprites from `qrcode`.
- [x] Add a scrim that keeps the hero headline legible.
- [x] Keep clash detection on so codes never overlap.
- [x] Add a drift mode with wall bounce and click push-away.
- [x] Add a bump mode with weight-based ramming and rests.
- [x] Add a mouse-chase mode that packs around the cursor.
- [x] Cap DPR and frame rate.
- [x] Pause the sim off-screen and on a hidden tab.
- [x] Auto-reduce the sprite count on narrow viewports.
- [x] Add the temporary dev control bar with live sliders.
- [x] Encode the daily theme and app links into the sprites.
- [x] Fall back to a static scatter on `prefers-reduced-motion`.
- [x] Cap the sprite count.
- [ ] Run the deferred mobile responsive pass.
- [ ] Polish each mode with the sliders.
- [ ] Bake the chosen slider values as defaults.
- [ ] Remove the whole dev bar.
- [ ] Add random-mode-per-refresh.
- [ ] Add swarm, route, and assemble modes.
- [ ] Add scannable Easter-egg codes.
- [ ] Wire real ForeverPin short links into the sprites.

---

## Engineering follow-ups

- [ ] Reconcile frontend naming forks against settled conventions
- [ ] Complete backend convention adoption
- [ ] Finish the remaining polish-track tasks
- [ ] Simplify E2E DTO mirrors while retaining raw-JSON assertions
- [ ] Review dependency vulnerability warnings with the SDK lane
- [ ] Establish product CI/CD and a repeatable deployment configuration

The active SDK-adoption scope is tracked in `v0.10`; implementation and verification remain open.
Backend convention work beyond that migration stays in this backlog.
