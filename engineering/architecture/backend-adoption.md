# Backend SDK adoption

*Last updated: 2026-09-26*

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

## Remaining acceptance

The published-package backend acceptance is complete. The adoption commit remains pending.
The frontend lane owns its verifier, browser comparison and manual product-flow acceptance.

The database migration adds owner/creation and subscription-ID indexes without changing stored payload formats.
Production deployment, real Google/Stripe credentials, browser QA and physical code scanning are unverified.
Cache invalidation, country-provider selection, non-HTTP delivery, calendar timezone meaning, durable analytics,
complete payments state, full browser CSRF/identity redesign and translation catalogs remain explicit deferred work.
