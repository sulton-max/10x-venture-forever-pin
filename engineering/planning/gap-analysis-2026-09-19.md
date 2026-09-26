# ForeverPin gap analysis

*Last updated: 2026-09-19*

## Verdict

ForeverPin is suitable for continued local product work, but the reviewed build is not ready for public use or paid subscriptions. Existing automated checks miss failures in the default static URL flow, guest identity, concurrent creation, and production-shaped health probes.

This is an audit, not an implementation batch. It preserves the SDK, deployment and frontend work already in progress. Vue migration and promo/marketing remain excluded. No public deployment, payment, account takeover against another person, or physical scan was performed.

## Scope and evidence

Reviewed the product's domain and request models, write handlers, identity integration, billing, rendering integration, redirect routing, analytics, frontend forms, deployment generator, container configuration, workflows, version plan and verification records. Reviewed the backend SDK adoption plan to separate shared-library ownership from product work.

| Evidence class | What this audit establishes |
|---|---|
| Direct local HTTP reproduction | Empty static URL rendering, missing mobile-app redirect, non-HTTP redirect, invalid static update, creation-cap race, acceptance of an invented guest ID, host-filter/probe mismatch |
| Current source inspection | Pointer retargeting, subscription state/entitlement logic, analytics transaction and drain gaps, release-generator coverage, bootstrap timeout |
| Existing SDK audit | SVG attribute escaping and requested PNG format defects; not reproduced again here |
| Existing product verification record | 213 backend tests and 4 frontend tests passed in the deployment lane; not rerun as a full suite in this audit |
| Fresh workflow check | `actionlint` passed for the three working-tree workflows |
| Not established | Hosted CI success, deployed production behavior, real Google/Stripe flows, browser execution of injected markup, physical QR scan reliability, load capacity |

The initial source baseline was `c434993`. Another active lane committed the staged deployment foundation during this review; the final observed HEAD was `341d0a9`. The index was empty at the final source check, while workflow/runtime follow-up changes remained unstaged or untracked. This report does not certify a future commit assembled from those changes.

### Runtime identity

Probes ran in a separate disposable Compose project, `foreverpin-gap-audit`, on management port `18021` and redirect port `18023`. The existing local ARM64 images were used, rather than rebuilding or changing another lane's image tags:

- Management: `sha256:a9467b8f966e521c9956aabb0c3a66eccd7cef17f12e1e83c8f1198b4edf471d`.
- Redirect: `sha256:c01347f9f3ce00f57ed36b7b512ea4120385b779f81d8754033335af63d81ce4`.

Source inspection corroborated the reproduced mechanisms. These image results are not proof for an exact future source commit or the hosted AMD64 images.

All audit containers, network and disposable database/key volumes were removed successfully. Existing DryDock pilot resources were untouched. Native approval allowed the required local Docker/socket work; no unresolved execution-permission blocker remained.

## Findings

P1 means close before exposing the affected capability to real users. P2 means a concrete correctness or operational gap with narrower impact. A planned SDK fix is not closed until the product consumes and verifies it.

### F01 — P1: guest identity trusts a caller-selected ID

**Evidence:** An invented `user-id=<UUID>` cookie, without guest provisioning, successfully created a code with HTTP 200. The product scopes ownership through the SDK's resolved ID. The SDK adoption audit independently traced raw-cookie trust and guest-to-account reassignment.

**Impact:** Possession of an owner UUID can stand in for authentication in guest-scoped paths. UUID secrecy is not an ownership check. This audit used a new synthetic ID, not someone else's account.

**Owner:** Backend SDK A07 plus product adoption. The live SDK working tree already contains an authenticated, expiring guest-cookie codec in progress; ForeverPin still references runtime `10.0.45-beta`. Do not duplicate that implementation.

**Acceptance:** Reject raw, tampered and expired tokens; reject registered-account IDs supplied as guest credentials; retain valid guest ownership and permitted claiming. Verify a protected guest cookie and registered session across container replacement.

Reference: [SDK adoption A07](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/wow-two-sdk-beta/wow-two-sdk.backend.beta/engineering/planning/foreverpin-adoption/foreverpin-adoption.md:161).

### F02 — P1: static URL and mobile-app content produces an empty payload

**Evidence:** Both content encoders return null, and the static mapper substitutes an empty string. Two distinct accepted static URLs returned byte-identical SVG files, SHA-256 `da12dd38b7c864fbefc83d29b76b89fdc2f93b54c503283f3f756351487d2594`. Both create and render endpoints returned 200.

**Impact:** The frontend defaults to static URL creation, so the default path can report success without encoding the requested destination. The QR was not decoded by this audit; the empty payload is established by the mapper and corroborated by the identical rendered output.

**Owner:** Product payload mapping, coordinated with SDK A10 serializer work. URL routing and QR payload construction are separate entry points; fixing one does not fix the other.

**Acceptance:** Static URL and mobile-app SVG/PNG exports decode to their exact intended payloads. Reject unsupported or absent payloads rather than silently rendering empty content.

References: [URL encoder](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.backend-services/ForeverPin.Domain/Codes/Content/Url/Models/UrlContentValueObject.cs:10), [mobile-app encoder](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.backend-services/ForeverPin.Domain/Codes/Content/MobileApp/Models/MobileAppLinkContentValueObject.cs:15), [static mapper](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.backend-services/ForeverPin.Domain/Codes/Rules/CodePayloadMapper.cs:28).

### F03 — P1: dynamic mobile-app links still return 404

**Evidence:** A valid App Store URL was accepted, its image rendered, and its short link returned 404. The working-tree routing fix special-cases URL content only; mobile-app content still falls through to its null encoder.

**Impact:** One advertised content type saves successfully but fails at scan time.

**Owner:** Product routing / SDK A10 adoption. Coordinate with the active URL-routing lane.

**Acceptance:** Real URL and mobile-app variants both resolve to validated HTTP(S) destinations. Test every advertised mobile-app store, not text content containing a URL.

Reference: [destination selection](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.backend-services/ForeverPin.Redirect.Api/Infrastructure/Routing/RoutingService.cs:52).

### F04 — P1: non-URL dynamic content has no safe delivery contract

**Evidence:** A dynamic text payload `javascript:void(0)` was accepted and returned HTTP 302 with that exact `Location`. Routing emits non-URL `Encode()` output as a redirect destination.

**Impact:** Text, WiFi, contact and calendar payloads are not uniformly navigable URLs. An arbitrary text payload can become a redirect header. This observation does not establish browser script execution.

**Owner:** Product delivery decision D03 / v0.9 iteration 13. The recorded preference for a small page from the redirect host remains undecided.

**Acceptance:** Define a per-type delivery matrix: validated redirect, explicit device action, downloadable content or rendered page. Reject unsupported combinations until their path is implemented. Test scheme handling and safe text output.

Reference: [routing fallback](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.backend-services/ForeverPin.Redirect.Api/Infrastructure/Routing/RoutingService.cs:56).

### F05 — P1: updating a static code bypasses its one-rule invariant

**Evidence:** A static code accepted a PUT containing a default rule plus a conditional rule; the response persisted both. Update validation constructs its rule set with a null mode. The UI still offers another rule.

**Impact:** Stored state violates the static-mode model. The mapper can use one rule while the UI suggests multiple routes.

**Owner:** Product validation / SDK A06 integration; already tracked in v0.9 iterations 9–10.

**Acceptance:** Load and authorize the target, validate against its persisted mode, and reject multiple rules for static updates through the API. Disable the corresponding UI action.

Reference: [update validator](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.backend-services/ForeverPin.Application/Codes/Core/Validators/CodeUpdateCommandValidator.cs:24).

### F06 — P2: reordering rules silently retargets a default pointer

**Evidence:** The frontend rewrites conditional orders sequentially but copies `targetOrder` unchanged. Reordering A(order 1), B(order 2), pointer(1) into B, A, pointer(1) submits B(order 1), A(order 2), pointer(1). The pointer now targets B. The drag handler only moves the array entry.

**Impact:** An otherwise valid edit changes the catch-all destination. Removing/reindexing rules can instead leave a dangling pointer.

**Owner:** Product frontend. This is source-confirmed; no browser drag reproduction was performed.

**Acceptance:** Preserve pointer identity through reorder, insertion, deletion, edit and copy. Remap existing references explicitly; require a choice when deleting their target. Test the mapper as well as the UI.

References: [request normalization](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.frontend-services/src/application/codes/createCodeForm.ts:191), [rule reorder](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.frontend-services/src/presentation/codes/routing/components/RuleControls.tsx:72).

### F07 — P1: concurrent creation exceeds the plan cap

**Evidence:** The free cap is 3. Ten parallel creates with one fresh guest saved 4 codes in one run and 8 in a repeat. The handler counts and inserts in separate operations.

**Impact:** A user can exceed an enforced commercial/storage limit by racing requests. Client-side controls cannot enforce this guarantee.

**Owner:** Product write transaction; SDK A09 can supply shared primitives. Slug allocation has a separate exists/insert race and unbounded retry loop; a unique database constraint prevents duplicates but does not provide a graceful collision outcome.

**Acceptance:** Concurrent creates never exceed the effective cap. Enforce at the database transaction boundary with a tested isolation/locking strategy. Force slug collisions and verify bounded recovery. Include concurrent subscription upserts and guest-transfer cases in write-integrity coverage.

Reference: [count and insert](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.backend-services/ForeverPin.Infrastructure/Codes/Core/CommandHandlers/CodeCreateCommandHandler.cs:35).

### F08 — P1 before billing: subscription status does not govern entitlements

**Evidence:** All subscription-update events are mapped to Active. Deletion sets Canceled but retains the paid plan. Code creation and billing limits use that plan without status or period checks. The adapter discards provider event identity/state needed for reliable reconciliation; an event for an unknown subscription is acknowledged without repair.

**Impact:** Canceled or delinquent state can keep paid creation limits; out-of-order delivery can leave incorrect state. Keeping existing printed links alive is intentional and does not require granting new paid creation indefinitely.

**Owner:** Product billing policy / SDK decision D04. Define grace periods and entitlement timing rather than inventing them inside the adapter.

**Acceptance:** Preserve provider state, deduplicate/reconcile retries, handle out-of-order events, and calculate effective entitlements from an explicit policy. Test cancellation, failed payment, expiry, late checkout, duplicate events and existing-link continuity. Decide how a paid guest subscription follows account claiming.

References: [subscription event handling](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.backend-services/ForeverPin.Infrastructure/Billing/CommandHandlers/BillingWebhookCommandHandler.cs:47), [retained plan](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.backend-services/ForeverPin.Infrastructure/Billing/CommandHandlers/BillingWebhookCommandHandler.cs:119), [creation entitlement](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.backend-services/ForeverPin.Infrastructure/Codes/Core/CommandHandlers/CodeCreateCommandHandler.cs:36).

Stripe documents duplicate delivery and lack of guaranteed event ordering in its [webhook guide](https://docs.stripe.com/webhooks), and distinct subscription states in its [subscription webhook guide](https://docs.stripe.com/billing/subscriptions/webhooks).

### F09 — P1: render validation and SVG trust need the coordinated SDK fix

**Evidence:** The SDK adoption audit reproduced a color value injecting an extra SVG attribute and Code128 PNG requests returning SVG. The product inserts returned preview SVG through `dangerouslySetInnerHTML`. A first-party endpoint does not make user-derived SVG attributes trusted.

**Impact:** Unsafe markup can reach the DOM; requested export format is not guaranteed. No browser script execution or resource-exhaustion attack was attempted in this audit.

**Owner:** SDK A05 and product A06. Rendering validators and renderer changes are already in the SDK's active working tree.

**Acceptance:** Bound renderer inputs and raster allocation, reject invalid enum/geometry values, escape all SVG attributes, and verify PNG signature/MIME/output agreement. Test the product's anonymous preview and saved-image entry points against the published package. Close the DOM trust assumption explicitly.

References: [SDK rendering audit](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/wow-two-sdk-beta/wow-two-sdk.backend.beta/engineering/planning/foreverpin-adoption/foreverpin-adoption.md:136), [preview HTML sink](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.frontend-services/src/presentation/codes/common/createCode/components/QrPreview.tsx:103).

### F10 — P2: scan events and counters can diverge

**Evidence:** Scan rows are saved before separate counter updates without one transaction. Queue overflow drops writes; cancellation exits without draining; a failed flush clears its buffer. Failure loss is logged, but overflow is not observed by the caller.

**Impact:** Counters can disagree with event history. Restarts and bursts can lose queued scans even when redirects succeed.

**Owner:** SDK adoption A09; durable delivery and retention policy remain D08.

**Acceptance:** Atomic event/counter flush, tested failure behavior, bounded shutdown drain and observable drops. Describe best-effort delivery honestly until durability is implemented; do not imply a complete analytics history.

References: [flush transaction boundary](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.backend-services/ForeverPin.Redirect.Api/Infrastructure/Analytics/ScanFlushBackgroundService.cs:74), [queue overflow](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.backend-services/ForeverPin.Redirect.Api/Infrastructure/Analytics/ChannelScanRecorder.cs:13).

### F11 — P1 deployment: domain-only host filtering breaks health probes

**Evidence:** With `AllowedHosts=foreverpin.test` on management and `redirect.foreverpin.test` on redirect, loopback requests returned 400; requests with the matching Host header returned 200. Both the image and generated release Compose probe `http://localhost:8080/health` without a configured Host header.

**Impact:** A healthy application can be marked unhealthy under production host settings, preventing deployment readiness.

**Owner:** Deployment lane.

**Acceptance:** Define an explicit internal probe-host contract, implement it in both image and release bundle, and run readiness checks using domain-restricted settings. Do not replace host filtering with a blanket wildcard merely to satisfy the probe.

References: [image healthcheck](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/deployment/Dockerfile:32), [release healthcheck](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/deployment/create-release.py:32).

### F12 — P2 deployment: the tested Compose path differs from the released path

**Evidence:** The shared verifier runs `compose.local.yml`; the publisher creates the digest-pinned release bundle afterward. It does not run that generated bundle with its settings mounts, external network and restricted host settings. The generated network alias uses optional `DEPLOY_ENVIRONMENT` interpolation, unlike its required settings paths/network name.

**Impact:** Release-only configuration faults escape the current smoke test. An omitted environment name can create shared aliases without environment isolation.

**Owner:** Deployment lane, coordinated with DryDock's input validation. Upstream validation may reduce the alias risk; the product bundle does not enforce it itself.

**Acceptance:** Exercise the generated release artifact in isolation with production-shaped settings. Require and validate the environment identifier. Verify manifest/image/source/hash consistency and reject absent settings or invalid platform values.

References: [local smoke path](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/.github/workflows/verify.yml:105), [network alias](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/deployment/create-release.py:34).

### F13 — P2 deployment: publication failure recovery is incomplete

**Evidence:** Images are pushed before the integration smoke completes. The release guard rejects an existing release, including a draft; draft creation, asset upload and publication are separate operations.

**Impact:** A failed smoke can leave registry artifacts with a release-looking tag. A failed upload/publication can leave a draft that blocks a normal rerun. Digest-pinned deployments prevent tag movement from silently changing an already recorded deployment, but do not clean up or resume a partial release.

**Owner:** Deployment lane. This is source analysis; no remote publication was performed.

**Acceptance:** Distinguish candidate artifacts from published releases, document or implement idempotent recovery, and test interruption at image publication, draft creation and asset upload. Verify existing assets and source identity before resuming; avoid automatic destructive deletion of operator data.

References: [image publication](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/.github/workflows/verify.yml:70), [draft publication](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/.github/workflows/publish-docker-image.yml:71).

### F14 — P2 frontend: runtime-config loading has no bounded wait

**Evidence:** The root is rendered only after runtime configuration resolves. Fetch has no application timeout or abort deadline. Rejections show a plain reload message, but a hanging request keeps the initial root empty until the browser/network layer fails.

**Impact:** An unhealthy config endpoint can appear as a blank application.

**Owner:** Product frontend. Source-confirmed; slow-network browser behavior was not exercised.

**Acceptance:** Render a bounded loading/error state, time out the request and offer retry. Preserve the distinction between optional settings and settings required to start safely.

References: [bootstrap gate](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.frontend-services/src/bootstrap/main.tsx:12), [config fetch](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/codebase/forever-pin.frontend-services/src/integration/common/client.ts:17).

## Remaining release gates and planned work

These are not all newly discovered defects. Keep their task status in the existing version/adoption plans rather than duplicating those checklists here.

| Gate | Remaining evidence or decision | Where it belongs |
|---|---|---|
| SDK adoption | Published and retrievable package family; product re-pin; consumer build/tests; protected-cookie and renderer regressions | Backend adoption A03–A12. Current product runtime is 10.0.45-beta; testing is 10.0.40-beta. Local SDK edits are not a release. |
| Browser/product completion | Approved single Copy dialog, static edit controls, pointer editing, all ten types in both modes, print/export guidance and physical scans | v0.9 iterations 9–13 and manual verification |
| Content/routing decisions | Resolve-page host, per-type delivery, geo behavior, pointer attribution, preset definition and versioning seam | Content-model decisions; do not infer acceptance from adjacent implementation |
| Country routing | NoopGeoBroker remains registered; country conditions cannot perform real country matching | Existing routing backlog / SDK decision D05; provider and proxy trust need explicit choices |
| Public environment | Hosted AMD64 CI and image artifacts, TLS/domain routing, forwarded-header trust, real Google login/logout/claiming and Stripe test-mode flows | Deployment and operations; local HTTP smoke bypasses browser cookie transport and does not verify real provider state |
| Recovery/operations | Backup restoration, previous-version rollback or documented forward recovery, secret/key handling, monitoring, load behavior and analytics retention | Deployment/operations; fresh-schema migration success is not rollback evidence |
| Dependency/error safety | Review current restore advisories, pin floating build dependencies, sanitize server errors and preserve cancellation semantics | SDK A03/A11 and product adoption. Existing logs report warnings; this audit does not assign fresh advisory severities. |
| Model review/polish | Remaining BE/FE member naming, frontend DTO consistency, calendar reproduction and control explanations | Current-work and v0.9. A suspected calendar defect still lacks a reproduction. |

### Verification gaps that explain the green baseline

- Frontend coverage currently consists of four content-operation tests; it does not cover form normalization, drag/pointer semantics or copy/edit flows.
- Current deployment smoke checks saved ownership, SVG presence and dynamic URL redirect. It does not decode the output, exercise static payloads, or verify the content/mode matrix.
- Manually replaying a raw guest cookie after container replacement does not establish data-protection-key correctness. A protected token and a registered session need explicit verification.
- Billing tests preserving printed redirects after cancellation do not test removal of new paid creation privileges.
- A same-version container replacement is not a previous-version rollback test.
- Source and ARM64 runtime checks do not establish hosted AMD64 native rendering.

### Claims deliberately not made

- Null rules were rejected with HTTP 400 in the local probe; no null-rule acceptance defect was found.
- The active redirect repository uses the database directly; an unused cache class is not evidence of live stale-cache behavior.
- Repository updates use tracked mutation; no detached-update failure was established.
- The API's development certificate does not need installation to execute the approved backend test workflow.
- Local health with permissive settings does not establish health under domain-only filtering.
- Planned SDK changes, latest package numbers in another plan and a successful local pack do not establish consumer adoption.
- No new migration, Vue migration, marketing work, git staging or product source edit was performed by this audit.

## Execution order

1. Carry F01/F09 into the existing SDK release/adoption gate; verify the published package in ForeverPin. Do not duplicate active SDK edits.
2. Close the independent product defects: static payload mapping, mobile-app redirect, stored-mode validation, pointer preservation and quota concurrency. Coordinate routing files with the active deployment lane.
3. Close deployment host/probe and generated-bundle gaps before treating the new pipeline as deployment evidence.
4. Resolve the per-type delivery decision and billing entitlement policy before enabling their unfinished public paths.
5. Complete browser/provider/print and recovery verification against one identified source commit and its image digests.

The audit does not require waiting idle for both SDK upgrades. Product-owned mapping, rule and deployment work can proceed in disjoint files. Package-sensitive identity, rendering and shared validation work should follow the owning SDK lane.

## Sources and task ownership

- [Current work](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/planning/current-work.md).
- [v0.9 task source](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/planning/version-track/v0.9/v0.9.md).
- [Backend SDK adoption](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/wow-two-sdk-beta/wow-two-sdk.backend.beta/engineering/planning/foreverpin-adoption/foreverpin-adoption.md).
- [Product verification record](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/operations/verification.md).
- [Deployment record](/Users/max/Projects/10x-ws/workbench/career/engineering/wow-two/wow-two-ws/workbench/ventures/10x-venture-forever-pin/engineering/deployment/deployment.md).
