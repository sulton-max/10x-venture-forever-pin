# Deployment

*Last updated: 2026-10-20*

## Implemented shape

- The management host serves the built frontend from `wwwroot`, behind the SDK secure headers and compression.
- Hashed assets cache for a year as immutable; every document revalidates, so a deploy reaches returning visitors.
- Public pages ship their own documents, so link previews read each page's title and description.
- The redirect host runs separately and shares PostgreSQL.
- Host startup applies embedded SQL migrations, stamped with the product version `vX.Y`.
- `Dockerfile` builds the `management` and `redirect` images from one source revision.
- `deploy.yml` (descriptor 2) declares both services, their sites and settings for CI, the generator and Wheelhouse.
- `compose.local.yml` runs both hosts with disposable local PostgreSQL; `smoke.py` exercises them.
- `/health` checks database readiness on both hosts; `api/system/status` reports each host's version and commit.
- The management image persists data-protection keys in `/data/keys`.
- The SPA loads public environment settings from `/api/runtime-config`.
- Development ports are listed in the [README](../../README.md).

---

## Sites

| Site | Served by | Paths | Holds |
|---|---|---|---|
| `app` (primary) | `management` | `/`, `/api` | the Vue app, the marketing pages and the API |
| `go` | `redirect` | `/` | every printed short link, and the scanner pages |

- The `go` host is printed into every dynamic code; choose it once, before the first code ships.
- Wheelhouse binds each site to one host per environment; nothing here names a host.

---

## Settings contract

- Each target's settings mount as `/app/appsettings.Target.json`; the be sdk reads it on a deployed host.
- Wheelhouse derives `AllowedHosts`, `Deployment:TrustedProxies` and `Sites:{app,go}:Url`; nobody types them.
- `Sites:go:Url` becomes `ApiSettings:RedirectBaseUrl`; `Sites:app:Url` sets Stripe's three return pages.
- The database layer fills `DatabaseSettings:ConnectionString` from the target's own database.
- `Auth:Google:ClientId` is optional: without it, Google sign-in hides and guests still work.
- Billing stays off until `Billing:SecretKey` is set; then it needs the webhook secret and `Billing:Prices:Basic`.
- `Billing:SecretKey` and `Billing:WebhookSecret` are typed secrets, kept in Wheelhouse's vault.
- `IpLocation:EnableDownload` (boolean, default true) lets the redirect host fetch the monthly DB-IP Lite file.
- The redirect image keeps that file in `/data/geo`; a volume avoids re-downloading.
- Production refuses loopback Stripe return pages; dev runs as .NET `Development`, prod as `Production`.

---

## CI and releases

- `publish-docker-image.yml` calls the shared `delivery.yml` of `wow-two-platform.pipelines`; no other CI file.
- Checks: `Backend` (all four .NET tiers), `Frontend` (lint, test, build), `Browser` (Playwright, desktop and phone).
- A push that changes only docs, planning or markdown builds, tests and publishes nothing.
- `main` releases `vX.Y.Z`: `X.Y` from the newest version-track folder, `Z` from the release tags.
- Other branches build `sha-<commit>` candidates that reach dev only by hand.
- Each image gets `APP_VERSION` and `VCS_REF`; the app bar and `api/system/status` show them.
- Images publish as `ghcr.io/sulton-max/10x-venture-forever-pin/<service>`; the bundle rides the GitHub release.
- After a dev rollout, the `verify` check asks both services for `/health` and `api/system/status`.
- `codeql.yml` scans C#, TypeScript and workflows weekly and on code pushes; Renovate proposes weekly updates.

---

## Go-live on Wheelhouse

Owner steps, in order; each needs an account or a decision.

1. Push `main`; confirm the first `publish-docker-image` run releases `v0.11.0` with both images.
2. Check both GHCR packages are public, or give the products host a pull token.
3. In Wheelhouse prod, import the catalog entry `foreverpin` and add targets `foreverpin-dev` and `foreverpin-prod`.
4. Place each target on a products server with a PostgreSQL database.
5. Bind hosts: dev `app` and `go` under the server pattern; prod `foreverpin.com` and the chosen `go` host.
6. First dev deploy: enter the Google client ID; leave billing empty until Stripe is ready.
7. Google: add the dev and prod origins to the OAuth client; production needs its own project and verified domain.
8. Stripe: a $12 yearly Basic price per mode; webhook `https://<app host>/api/billing/webhook` per environment.
9. Promote the dev-verified release to prod.

---

## Local verification

- `docker compose config` passed with an explicit test password.
- Both images built from a clean container context.
- The three-service stack reached healthy state.
- Both `/health` endpoints returned HTTP `200`.
- The SPA returned HTML, while an unknown `/api/*` route returned problem JSON.
- `/api/runtime-config` returned the configured public redirect origin.
- The database and data-protection key survived management-container replacement.

---

## Outstanding evidence

- A green delivery run and verified GHCR images.
- A verified dev and prod deployment and an operator recovery path.
- Domain routing, TLS and the real redirect host.
- Provider secrets and Google/Stripe callback configuration.
- Database backups, restore verification, health monitoring and burst-load results.

A local build or a marketing page in source does not establish a live deployment.
Preserve existing printed redirect URLs when changing domains.
