# ForeverPin.E2ETests

End-to-end tests — the real management API **and** redirect hot path, exercised over HTTP against a **real Postgres**, no mocks. The **primary** test tier.

## Test tiers (where a behavior is proven)

| Project | Tier | What it proves | DB |
|---|---|---|---|
| `ForeverPin.UnitTests` | Unit | Pure logic — routing permutations, render formats, plan-limit math. No I/O. | none |
| `ForeverPin.IntegrationTests` | Integration | Below-HTTP DB branches only — repository edge cases (cascade delete, `ExecuteUpdate`, audit stamping, single-row upsert, Stripe-id lookup, type round-trips) and the cached config store. | Postgres container **or** in-memory SQLite (switchable) |
| `ForeverPin.E2ETests` | E2E | The whole flow over HTTP across **both** hosts — identity, codes CRUD, billing, the redirect wedge. Mocks only the two external seams (Google, Stripe). | Postgres container (host-boot) |

The rule: a behavior observable through an endpoint is proven **here** (E2E); only a genuinely below-HTTP branch stays in `ForeverPin.IntegrationTests`.

## Why E2E-first (not unit / mock-heavy integration)

forever-pin has **no external 3rd-party APIs in the hot paths** (geo = `NoopGeoResolver`, device detection = in-proc UA parse, analytics = in-proc channel, code-gen = managed). Only Postgres is external; Redis is optional; Google and Stripe are the two outbound seams, both faked at the host boundary (`FakeGoogleTokenVerifier`, `FakeBillingBroker`). So E2E mocks almost nothing and covers the whole flow — including the two-host wedge that handler/unit tests can't see.

## Prerequisite: Docker

Testcontainers spins an ephemeral `postgres:16-alpine` per run. **Docker must be running** (locally + in CI).

## Run

```bash
dotnet test ForeverPin.E2ETests/ForeverPin.E2ETests.csproj
```

One shared container for the whole run (xUnit collection fixture); both hosts boot in-proc against it; migrations auto-apply on startup; **Respawn** truncates the data tables (never `migration_history`) between tests. The shared `FakeBillingBroker` is reset between tests alongside the DB.

## Coverage

- **Identity:** anonymous → guest mint (sets `user-id` cookie) → guest.
- **Auth:** Google sign-in (find-or-create) · invalid-token `401` · `/me` profile · same-device guest-code claim · cross-device ownership · logout (fake verifier seam).
- **Codes CRUD + search**, owner-scoping (`401` anon · `404` cross-owner, no existence leak), slug immutable on edit, `scanCount`/`createdAt` preserved, rules replaced.
- **Image render** (svg / png content-type + bytes).
- **Billing:** checkout (paid → gateway URL + resolved price; Free → `400`; anon → `401`) · portal (`404` no-customer · URL with customer) · `/me` snapshot (Free/Pro/Agency limits + live usage; anon → `401`) · the create-time **402** cap per plan · the Stripe-webhook lifecycle (subscribe / upgrade via `subscription.updated` / cancel) over the webhook endpoint with a fake gateway.
- **Wedge (hero):** create via Api → scan via Redirect (`302`) → `scanCount` increments → edit the rule via Api → next scan routes to the **new** destination (slug unchanged).
- **Never-deactivate-on-downgrade:** after a cancel webhook, a printed code STILL resolves through the Redirect host (cross-host, plan-agnostic hot path).

## Harness

`Harness/` rides the wow-two backend-beta SDK testing package (`WebApiTestHost<T>`, `MultiHostFixture`, `PostgresFixture` + Respawn). `AppFixture` boots both hosts over one shared container and swaps the two external seams (`IGoogleIdTokenVerifier`, `IBillingBroker`) plus fake Stripe settings for deterministic E2E. Billing tests stage a webhook event on the shared `FakeBillingBroker` and assert the result through `/api/billing/me` and the redirect host.
