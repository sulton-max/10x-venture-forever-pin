# Deployment

*Last updated: 2026-09-25*

## Implemented shape

- The management host serves the built frontend from `wwwroot`.
- The redirect host runs separately and shares PostgreSQL.
- Host startup applies embedded SQL migrations.
- `Dockerfile` builds management and redirect images from one source revision.
- `compose.local.yml` runs both hosts with disposable local PostgreSQL.
- Main pushes verify disposable images; version tags publish verified release bundles.
- The release bundle pins both image digests and its generated Compose hash.
- `/health` checks database readiness on both hosts.
- The management image persists data-protection keys in `/data/keys`.
- The SPA loads public environment settings from `/api/runtime-config`.
- Development ports are listed in the [README](../../README.md).

---

## Settings contract

- The release bundle lists every required setting; DryDock's `transport.py template` prints each service's skeleton.
- `AllowedHosts` lists the public host plus `localhost`: health checks and DryDock smoke probes call `http://localhost:8080`.
- `Deployment:TrustedProxies` is a JSON array of ingress IPs; a plain string trusts no proxy.
- DryDock refuses a deployment that breaks any of these before replacing a container.
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
- Earlier verifier resources were removed; the DryDock pilot retains its own named test volumes.

---

## Outstanding evidence

- A successful tag-triggered workflow and verified GHCR artifacts.
- A verified production deployment and operator recovery path.
- Domain routing, TLS, trusted proxy addresses, and real redirect base URL.
- Provider secrets and Google/Stripe callback configuration.
- Database backups, restore verification, health monitoring, and burst-load results.

A local build or a marketing page in source does not establish a live deployment.
Preserve existing printed redirect URLs when changing domains.

---

## Git and CI policy

- `ci.yml`: every push to `main` calls the shared verifier without package-write permission.
- `verify.yml`: frontend/backend tests, both Linux images, saved-code/SVG/redirect smoke, container replacement.
- `publish-docker-image.yml`: a `v*` tag on a commit already in `main` calls the same verifier with package publication.
- Only a successful verifier produces a complete bundle; assets are attached to a draft before publication.
- Release labels are explicit; deployment uses recorded digests, never `latest`.
- The external actions are pinned to commit SHAs; `actionlint 1.7.12` passes locally.
- No workflow or image publication has run remotely; the local changes remain uncommitted.
- CI requires the intended runtime fixes, `smoke.py`, Dockerfile and workflows to be included in the pushed commit.

The full ownership, registry and retention analysis is in DryDock's
[CI/artifact policy](../../../../wow-two-platform/wow-two-platform.drydock/engineering/planning/ci-artifact-policy.md).

Explicit Linux runtime identifiers removed other platforms' native assets from both images.
Local `linux/arm64` sizes are 443,557,503 bytes for management and 435,977,901 bytes for redirect.
The smaller images passed saved-code ownership, native SVG and redirect checks against the retained pilot database.
Hosted `linux/amd64` evidence remains open.
