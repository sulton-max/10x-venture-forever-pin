# ForeverPin — Backlog

*Last updated: 2026-10-20*

Every unbuilt item, grouped; top of each group = next. Shipped work lives in [`version-track/`](version-track/).

## Features

| Feature | State | Spec |
|---|---|---|
| QR code generation | shipped v0.1 | — |
| SVG and PNG downloads | shipped v0.1 | — |
| Rule-based scan routing with a fallback | shipped v0.1 | — |
| Guest login and private code ownership | shipped v0.1 | — |
| Code editing, search, enabling, disabling and deletion | shipped v0.1 | — |
| Marketing site: landing, pricing and blog | shipped v0.1 | — |
| Paid plans through Stripe Checkout and the customer portal | shipped v0.1 | — |
| Google sign-in and sign-out | shipped v0.3 | — |
| Guest-code claim on sign-in | shipped v0.3 | — |
| Cross-device code access | shipped v0.3 | — |
| Server-rendered preview matching the download | shipped v0.5 | — |
| Module and finder-eye shapes | shipped v0.5 | — |
| Colors, gradients and transparent backgrounds | shipped v0.5 | — |
| Center emoji | shipped v0.5 | — |
| Dark mode | shipped v0.5 | — |
| Content types with per-type forms | shipped v0.7 | — |
| Barcode symbologies | shipped v0.7 | — |
| Interactive landing hero | shipped v0.7 | [landing-hero-concept.md](../../product/marketing/landing-hero-concept.md) |
| Static or dynamic mode per code | shipped v0.9 | — |
| Dynamic-only short links | shipped v0.9 | — |
| Device, country, language and time conditions | shipped v0.9 | — |
| Optional fallback rules and fallback pointers | shipped v0.9 | — |
| Code copy into the other mode | shipped v0.9 | — |
| Field-level validation messages | shipped v0.9 | — |
| Country lookup for country rules | shipped v0.10 | — |
| Redirect-target warnings on saved codes | shipped v0.10 | — |
| Builder control help | shipped v0.11 | — |
| Print-sized SVG export with sizing guidance | shipped v0.11 | — |
| Per-code scan analytics | shipped v0.11 | — |
| Scanner pages for unknown, paused and unmatched codes | shipped v0.11 | — |
| Guest browser-storage notice | shipped v0.11 | — |
| Crawlable public pages | shipped v0.11 | — |
| Dynamic non-URL content delivery | planned | — |
| Phone, SMS, email and geo device actions | planned | — |
| Lost-cookie recovery | planned | — |
| Account data export and deletion | planned | — |
| Destination screening and abuse controls | planned | — |
| Abuse reporting and takedown | planned | — |
| Per-code timezones and scheduled windows | planned | — |
| A/B weights and grouped conditions | planned | — |
| Deeper language, referrer and UTM routing | planned | — |
| Custom domains | planned | — |
| Password-protected content | planned | — |
| Expiry, scan caps and one-time links | planned | — |
| Logo and file upload | planned | — |
| PDF export | planned | — |
| Frames and call-to-action captions | planned | — |
| Image-like QR rendering | planned | — |
| Animated exports | planned | — |
| Static-content version history | planned | — |
| Bulk generation and serialized payloads | planned | — |
| Public API keys and developer endpoints | planned | — |
| Scan webhooks and exports | planned | — |
| Agency workspaces and white-label controls | planned | — |
| GS1 Digital Link | planned | — |
| Hosted editable content pages | planned | — |
| NFC pairing | planned | — |
| Self-host edition | planned | — |
| Free-generator entry page | planned | — |
| Content-type and comparison landing pages | planned | — |
| Social preview image for shared links | planned | — |

## Launch and product alignment

| Item | Type | Notes |
|---|---|---|
| Verify real Google sign-in with the configured OAuth client | product | — |
| Verify Stripe test-mode checkout, portal and webhooks | product | — |
| Tie paid entitlements to subscription status | fix | every subscription update maps to Active; [gap analysis](../research/gap-analysis-2026-09-19.md) F08 |
| Decide the free dynamic allowance and entry pricing | product | — |
| Align static-code limits with the chosen pricing | product | — |
| Reconcile marketing claims with implemented capabilities | product | — |
| Define guest-subscription merging across accounts | product | — |
| Choose the permanent `go` short-link host | product | printed into every dynamic code |
| Configure and verify deployment, TLS and redirect domains | engineering | — |
| Verify database backup and restore | engineering | — |
| Measure redirect bursts and scan-queue loss | engineering | — |
| Add destination screening and abuse controls | feature | request budgets exist |
| Add reporting and takedown handling | feature | — |
| Add account data export and deletion | feature | promised in the comparison table |
| Verify brand clearance and social handles | product | — |
| Validate the never-deactivate value proposition with users | product | — |

## Accounts

| Item | Type | Notes |
|---|---|---|
| Design lost-cookie recovery with proof of ownership | feature | — |

## Routing and analytics

| Item | Type | Notes |
|---|---|---|
| Decide total-versus-unique scan semantics without fingerprinting | product | — |
| Complete the Redis configuration read/write path | engineering | — |
| Derive the scan counter from scan events | engineering | the async denormalized counter stays until then |
| Add per-code timezones and scheduled windows | feature | — |
| Add A/B weights and grouped conditions | feature | — |
| Add language, referrer and UTM routing depth where required | feature | — |

## Domains and link controls

| Item | Type | Notes |
|---|---|---|
| Add custom-domain ownership, TLS and host-aware resolution | feature | — |
| Design password-protected content delivery | feature | needs complete authoring and delivery flows, not dormant columns |
| Add explicit expiry, scan caps and one-time links | feature | needs complete authoring and delivery flows, not dormant columns |

## Styling and files

| Item | Type | Notes |
|---|---|---|
| Add file upload and logo controls | feature | — |
| Add PDF export | feature | — |
| Research frames and call-to-action captions | feature | — |
| Add versioned style upgrades at the first breaking change | engineering | — |
| Complete shape/eye controls and remaining color-panel polish | feature | — |
| Explore image-like QR rendering without reducing scan reliability | feature | — |
| Add animated exports if product demand supports them | feature | — |

## Builder

| Item | Type | Notes |
|---|---|---|
| Review the content labels against the current models | engineering | — |
| Explain content-type replacement while editing | feature | — |
| Map field kinds to native input types | feature | — |
| Draw per-content design alternatives | product | — |
| Review the fill, shape, emoji, contrast and preview controls | engineering | — |
| Evaluate extracting the background controls | engineering | — |
| Unify builder separators and label spacing | engineering | — |
| Verify gradient-stop and emoji-layer rendering | engineering | — |
| Review the rule controls for presentation polish | engineering | — |

## Content and developer features

| Item | Type | Notes |
|---|---|---|
| Add static-content version history | feature | — |
| Add bulk generation and serialized payloads | feature | — |
| Add public API keys and developer endpoints | feature | — |
| Add scan webhooks and exports | feature | — |
| Add agency workspaces and white-label controls | feature | — |
| Evaluate GS1 Digital Link | feature | — |
| Integrate hosted editable content pages as a separate product | product | minimal non-URL delivery comes first |
| Evaluate NFC pairing and a self-host edition | feature | — |

## Marketing

| Item | Type | Notes |
|---|---|---|
| Verify branded promo exports visually | product | — |
| Build the free-generator entry page | feature | — |
| Add content-type and comparison landing pages | feature | — |
| Add a social preview image for shared links | feature | — |
| Prepare short-form launch content | product | — |
| Add a privacy-first acquisition funnel | product | — |
| Evaluate creator integrations and wedding/event channels | product | — |

## Engineering

| Item | Type | Notes |
|---|---|---|
| Reconcile frontend naming forks against settled conventions | engineering | — |
| Complete backend convention adoption | engineering | rows in [backend convention updates](../research/backend-convention-updates.md) |
| Simplify E2E DTO mirrors while retaining raw-JSON assertions | engineering | — |
| Offer one-click Report on defect notices | feature | needs an incident ingest; `feedbackQueryErrors` takes the reporter |
| Move the marketing header onto `Navbar` | engineering | the product frame is on `AppShell` |

## Landing hero

| Item | Type | Notes |
|---|---|---|
| Run the deferred mobile responsive pass | feature | parked; [concept](../../product/marketing/landing-hero-concept.md) |
| Tune each hero mode and bake the chosen defaults | feature | — |
| Add random-mode-per-refresh | feature | — |
| Add swarm, route and assemble modes | feature | — |
| Add scannable Easter-egg codes | feature | — |
| Wire real ForeverPin short links into the sprites | feature | — |
