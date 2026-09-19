# Deployment

*Last updated: 2026-09-19*

## Implemented shape

- The management host serves the built frontend from `wwwroot`.
- The redirect host runs separately and shares PostgreSQL.
- Host startup applies embedded SQL migrations.
- `Dockerfile` builds management and redirect images from one source revision.
- `compose.local.yml` runs both hosts with disposable local PostgreSQL.
- Published releases build and push both images through GitHub Actions.
- The release bundle pins both image digests and its generated Compose hash.
- `/health` checks database readiness on both hosts.
- The management image persists data-protection keys in `/data/keys`.
- The SPA loads public environment settings from `/api/runtime-config`.
- Development ports are listed in the [README](../../README.md).

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
- The disposable containers, network, database, and key volumes were removed.

---

## Outstanding evidence

- A successful published-release workflow and verified GHCR artifacts.
- A verified production deployment and operator recovery path.
- Domain routing, TLS, trusted proxy addresses, and real redirect base URL.
- Provider secrets and Google/Stripe callback configuration.
- Database backups, restore verification, health monitoring, and burst-load results.

A local build or a marketing page in source does not establish a live deployment.
Preserve existing printed redirect URLs when changing domains.
