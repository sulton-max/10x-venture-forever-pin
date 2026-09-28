# Backend SDK adoption

*Last updated: 2026-09-28*

## Status

- [x] Implement the authorized backend adoption across both hosts and all 12 projects.
- [x] Verify the SDK Release suites and the seven local package/symbol pairs.
- [x] Complete the final product regressions against the local candidate: 231 passed, zero skipped.
- [x] Publish the SDK corrections and verify all seven `10.0.59-beta` packages on NuGet.
- [x] Pin `10.0.59-beta` and verify a normal NuGet restore and all 231 backend tests.
- [x] Commit the backend adoption without the frontend lane: `0ac30af`, .NET SDK pin `36dc3a0`.

This is the backend evidence for [v0.10](../planning/version-track/v0.10/v0.10.md).
The SDK's [adoption sweep](../../../../wow-two-sdk-beta/wow-two-sdk.backend.beta/engineering/planning/foreverpin-adoption/foreverpin-adoption.md)
owns the component inventory and deferred platform decisions. Frontend acceptance remains separate.

## Implemented cut

| Area | Result |
|---|---|
| Hosts and build | Both hosts consume current SDK defaults, startup diagnostics, validated settings and explicit environment aliases. Package versions and .NET 10.0.300 are centralized. Unused direct provider references are removed. |
| Errors and validation | Registered SDK ProblemDetails mapping owns unexpected errors and field paths. Preview validates before rendering; updates validate using the owner-scoped entity's persisted mode. Cancellation remains cancellation. |
| Identity | Protected guest capabilities replace raw GUID trust. Google authentication creates a separate account identity and atomically claims guest rows under ordered owner locks. Stale guest updates cannot modify a transferred row. |
| Persistence | SDK JSON profiles and deep snapshot comparison replace product registry/wrapper duplication. Owner locks protect plan caps and subscription writes; bounded slug retries use savepoints and cryptographic IDs. |
| Payloads and routing | SDK payload exporters, language parsing and device classification replace generic product implementations. URL/mobile destinations use their URL fields. Product union types and routing policy remain local. |
| Analytics | A scan batch writes events and counters in one transaction. Shutdown drains the bounded queue within its budget; rejected, persisted and failed batches are observable. Delivery remains best effort. |
| API structure | Controllers, requests and DTOs use domain-first placement. Application models have `Model` names; HTTP projections retain existing wire properties and routes. |
| Tests | Host-local configuration replaces global environment mutations. SDK tests own generic renderer/migration mechanics; product tests cover stored data, endpoints, migrations and concurrency. |

The management host binds Data Protection storage lazily through options. Production still requires
`DATA_PROTECTION_KEY_PATH` / `DataProtection:KeyPath`; persisted keys keep guest capabilities valid across restarts.
Both product test hosts run in `Development`, preserving dependency-lifetime validation.

## SDK release dependency

The final consumer version is `10.0.59-beta`. Publication run
[36233039580](https://github.com/wow-two-sdk/wow-two-sdk.backend.beta/actions/runs/36233039580)
was triggered by source `77be87af3c6d9ee422013df2b47c546d0082a552` and tested release commit
`6924fa6fbce151d45a1089fb0632176f93691134`. The workflow succeeded: 611 tests passed, one existing
Kafka test skipped, all seven packages verified available, and tag `v10.0.59-beta` resolves to that revision.
The source includes the HTTP/data/idempotency follow-up and two adoption corrections:

1. SDK commit `f84aeba`: `FloatingCalendarEventModel.End` is optional, matching the existing product contract. The exporter omits `DTEND`
   when absent and validates it when supplied. Calendar timezone policy remains deferred.
2. SDK commit `c40fd57`: `WebApiTestHost` defaults to `Development`, so invalid dependency lifetimes fail during test startup.

The local package family is `10.0.59-beta.local.20260926.2`, under
`/tmp/foreverpin-sdk-candidate-20260926-2`. This is an unpublished working-tree candidate; its repository metadata
identifies the base commit, not a committed snapshot of the two corrections. All seven package/symbol pairs passed
the SDK artifact verifier. Publication must rebuild the committed revision normally.

`Directory.Packages.props` now pins runtime and test packages to `10.0.59-beta`. Acceptance requires all seven
published packages to be available and a complete product run without candidate overrides.

## Verification

SDK verification before the final test-host change: 610 passed, one existing Kafka skip across seven Release suites.
The affected Web suite was rerun after that change: 103 passed, zero skipped.
Logs: `/tmp/sdk-adoption-release-tests.log`, `/tmp/sdk-adoption-web-tests-final.log`.

Final product verification passed against published `10.0.59-beta`, without local feeds or version overrides:

| Suite | Passed | Skipped |
|---|---:|---:|
| Both HTTP hosts | 81 | 0 |
| PostgreSQL integration | 30 | 0 |
| Embedded product migrations | 1 | 0 |
| Unit and stored-contract regressions | 119 | 0 |
| Total | 231 | 0 |

`bash engineering/scripts/verify.sh backend` exited successfully. Evidence: `/tmp/foreverpin-adoption-published-tests.log`.
Restore assets for all 12 active solution projects resolve SDK references to `10.0.59-beta`; no candidate feed remains.
Coverage includes deep JSON snapshots, concurrent caps/subscriptions/claims, stale guest updates after a claim,
scan-batch rollback/drain, safe error responses, persisted guest keys, loaded-mode validation and product field paths.
The host configuration test confirms both actual HTTP hosts use `Development`.

Earlier local candidate verification also passed the same 231 tests. Evidence: `/tmp/foreverpin-adoption-tests-final.log`.
The following historical commands were used from `engineering/codebase/forever-pin.backend-services`:

```bash
dotnet restore ForeverPin.BackendServices.slnx --disable-build-servers -m:1 \
  -p:BuildSpa=false -p:BackendSdkVersion=10.0.59-beta.local.20260926.2 \
  --configfile /tmp/foreverpin-sdk-candidate-20260926-2/NuGet.Config --force
dotnet test ForeverPin.BackendServices.slnx --no-restore --disable-build-servers -m:1 \
  -p:BuildSpa=false -p:BackendSdkVersion=10.0.59-beta.local.20260926.2 \
  -p:UseSharedCompilation=false --verbosity minimal
```

The temporary NuGet configuration maps `WoW2.Sdk.Backend.Beta*` to the candidate directory and other packages to
NuGet. No repository feed or global NuGet configuration is changed. Native execution is required for Docker,
test-runner sockets and restricted restores; successful native retries establish the actual result.

## Extraction cut — 10.0.60-beta

The SDK extracted nine mechanisms from this product; this cut replaces the local copies.
SDK record: [ForeverPin extractions](../../../../wow-two-sdk-beta/wow-two-sdk.backend.beta/engineering/planning/foreverpin-extractions/foreverpin-extractions.md).

| Area | Result |
|---|---|
| Locks | `Database.AcquireTransactionLocksAsync("owner", ids)` replaces the product helper; Google subjects use the same call. |
| Unique saves | The slug insert uses `TrySaveChangesUniqueAsync("ix_codes_slug")`; the handler keeps its bounded retry. |
| Hosting | Both hosts drop their `DeploymentHosting` copies: SDK proxy trust, persistent keys and database readiness. |
| Scans | `AddBatchPipeline<ScanRecordModel, ScanBatchHandler>` replaces the channel recorder and flush worker. |
| Country routing | The redirect host locates scans with DB-IP Lite in `IP_LOCATION_DIRECTORY`, refreshed monthly. |
| Redirect cache | Scanned codes cache in memory; update, deactivate and delete evict them on commit through `pg_notify`. |
| Stripe webhooks | The EF inbox applies each event id once; `last_event_at` ignores events older than the applied one. |
| Warnings | URL destinations raise non-blocking warnings; create and update return them as `warnings`, shown after save. |
| Antiforgery | Cookie writes echo `XSRF-TOKEN` as `X-XSRF-TOKEN`; the signed Stripe webhook is exempt. |
| Environment | Both hosts read only listed variables through `UseEnvironmentAliases`; an empty variable no longer overrides. |
| Proxy trust | Both hosts bind `Deployment:TrustedProxies` and `TrustedNetworks` with `TrustProxiesFrom`. |
| SPA hosting | `ApiDefaultsOptions.SpaHosting` serves the bundle behind secure headers; unknown `/api` routes stay JSON 404s. |
| Sign-in popups | `AllowCrossOriginOpenerPopups` replaces the HTML header rewrite; the embedder policy is off for Google's script. |

Migration `012-billing-events` adds `inbox_messages` and `subscriptions.last_event_at`; stored payloads are unchanged.
The owner-lock key hash changed with the SDK helper, so every management instance must run the same release.
Forwarded headers are trusted only from loopback, `Deployment:TrustedProxies` and `Deployment:TrustedNetworks`.
The Vue client refreshes a stale antiforgery token once through `/api/runtime-config`, after sign-in or sign-out.
`@wow-two-beta/ui-vue` gained the same echo and `decodeWarned`; the product drops its bridges after the next publish.

Verification against local candidate `10.0.60-beta.local.20260927.3`, built from the SDK's committed HEAD:

| Suite | Passed | Skipped |
|---|---:|---:|
| Both HTTP hosts | 89 | 0 |
| PostgreSQL integration | 29 | 0 |
| Embedded product migrations | 1 | 0 |
| Unit and stored-contract regressions | 119 | 0 |
| Frontend unit tests | 7 | 0 |

New coverage: country routing from a located client, eviction after edit and deactivation, redelivered and
out-of-order Stripe events, warning envelopes, and a forged cookie write rejected while the SPA client passes.
The SDK-owned queue overflow test moved to the SDK. Publication must precede the final restore check.

A browser smoke ran the management host from candidate `.3`, serving the built SPA on a fresh PostgreSQL:
all 12 migrations applied; guest creation, previews and saves passed antiforgery; the insecure-URL warning
rendered after save; the country-rule and footer DB-IP credits linked to `db-ip.com`.
Development runs serve the project's `wwwroot` through static web assets, so the smoke used a non-Development environment.

Candidate `.4` adds environment aliases, configured proxy trust and popup-safe opener headers; the same 238 tests pass.
A second smoke served the Vue build: the shell carried `same-origin-allow-popups` without an embedder policy,
unknown API paths answered JSON 404s, and a guest's insecure-URL save showed its warning.

Candidates `.5` to `.8` add lazily bound request budgets (`dc74ad7`), route documents from the SPA fallback
(`158eef4`, `db3bc89`), and the SPA served behind the secure headers with asset caching (`80aeb63`).
Against `.8` the product passes 251 backend tests; publication must include `80aeb63`.

## Remaining acceptance

The published-package backend acceptance for `10.0.59-beta` is complete; `10.0.60-beta` awaits publication.
The frontend lane owns its verifier, browser comparison and manual product-flow acceptance.

The database migrations add indexes, the webhook inbox and an event watermark without changing stored payloads.
Production deployment, real Google/Stripe credentials, browser QA and physical code scanning are unverified.
Non-HTTP delivery, calendar timezone meaning, durable analytics, complete payments state, identity redesign and
translation catalogs remain explicit deferred work.
