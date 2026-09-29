# Deployment

*Last updated: 2026-09-29*

## Implemented shape

- The management host serves the built frontend from `wwwroot`, behind the SDK secure headers and compression.
- Hashed assets cache for a year as immutable; every document revalidates, so a deploy reaches returning visitors.
- Public pages ship their own documents, so link previews read each page's title and description.
- The redirect host runs separately and shares PostgreSQL.
- Host startup applies embedded SQL migrations.
- `Dockerfile` builds management and redirect images from one source revision.
- `deploy.yml` declares both services for CI, Wheelhouse's `release.py` and the runner.
- `compose.local.yml` runs both hosts with disposable local PostgreSQL.
- Every push builds a candidate bundle; a published `vX.Y.Z` release builds a release bundle.
- A bundle pins each image digest and its generated Compose hash.
- `/health` checks database readiness on both hosts.
- The management image persists data-protection keys in `/data/keys`.
- The SPA loads public environment settings from `/api/runtime-config`.
- Development ports are listed in the [README](../../README.md).

---

## Settings contract

- `deploy.yml` lists every required setting; Wheelhouse's `transport.py template` prints each service's skeleton.
- Both hosts read `DatabaseSettings:ConnectionString` or `DB_CONNECTION`; the bundle previously named a dead key.
- Enabled billing requires `Billing:SuccessUrl`, `CancelUrl` and `PortalReturnUrl`; Production refuses loopback ones.
- `AllowedHosts` lists the public host; the SDK appends `localhost`, so health checks and smoke probes pass.
- `Deployment:TrustedProxies` is a JSON array of ingress IPs; a plain string trusts no proxy.
- `Deployment:TrustedNetworks` is a JSON array of ingress CIDR ranges, such as a Docker network.
- Without either, only loopback forwarding is trusted, so scans behind an ingress record the ingress address.
- The redirect image keeps the monthly DB-IP Lite database in `/data/geo`; a volume avoids re-downloading.
- Wheelhouse refuses a deployment that breaks any of these before replacing a container.
- The local and CI stack runs with `AllowedHosts: "localhost;127.0.0.1"`; the smoke asserts a foreign host gets `400`.

---

## Local verification

- `docker compose config` passed with an explicit test password.
- Both images built from a clean container context.
- The three-service stack reached healthy state.
- Both `/health` endpoints returned HTTP `200`.
- The SPA returned HTML, while an unknown `/api/*` route returned problem JSON.
- `/api/runtime-config` returned the configured public redirect origin.
- Both hosts applied the same ten migrations without errors.
- The database and data-protection key survived management-container replacement.
- The release generator produced a valid digest-pinned Compose model.
- Earlier verifier resources were removed; the Wheelhouse pilot retains its own named test volumes.

---

## Outstanding evidence

- A successful build workflow run and verified GHCR images.
- A verified production deployment and operator recovery path.
- Domain routing, TLS, trusted proxy addresses, and real redirect base URL.
- Provider secrets and Google/Stripe callback configuration.
- Database backups, restore verification, health monitoring, and burst-load results.

A local build or a marketing page in source does not establish a live deployment.
Preserve existing printed redirect URLs when changing domains.

---

## Git and CI policy

- `ci.yml`: every push to `main` runs frontend, browser and backend tests, then smokes both Linux images.
- `publish-docker-image.yml` follows the workspace descriptor convention and Wheelhouse's reference workflow.
- A push builds changed services as `sha-<commit>` and uploads the Actions artifact `bundle-<commit>`.
- A published `vX.Y.Z` release builds changed services as `X.Y.Z` and attaches `foreverpin-release.tar.gz`.
- Wheelhouse's Build action dispatches the workflow for a commit that has no build.
- Unchanged services keep the previous release's images; the base comes from its attached bundle.
- Images publish as `ghcr.io/sulton-max/10x-venture-forever-pin/<service>`.
- The workflow fetches `release.py` from Wheelhouse by git blob and checks its SHA-256.
- A private Wheelhouse needs a `WHEELHOUSE_READ_TOKEN` secret with read access to it.
- Deployment uses recorded digests, never `latest`; external actions are pinned to commit SHAs.

The full ownership, registry and retention analysis is in Wheelhouse's
[CI/artifact policy](../../../../wow-two-platform/wow-two-platform.wheelhouse/engineering/planning/ci-artifact-policy.md).

Explicit Linux runtime identifiers removed other platforms' native assets from both images.
Local `linux/arm64` sizes are 443,557,503 bytes for management and 435,977,901 bytes for redirect.
The smaller images passed saved-code ownership, native SVG and redirect checks against the retained pilot database.
Hosted `linux/amd64` evidence remains open.
