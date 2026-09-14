# Deployment

*Last updated: 2026-09-13*

## Implemented shape

- The management host serves the built frontend from `wwwroot`.
- The redirect host runs separately and shares PostgreSQL.
- Host startup applies embedded SQL migrations.
- Development ports are listed in the [README](../../README.md).

---

## Outstanding evidence

- A verified production deployment and repeatable deployment configuration.
- Domain routing, TLS, forwarded-header configuration, and real redirect base URL.
- Provider secrets and Google/Stripe callback configuration.
- Database backups, restore verification, health monitoring, and burst-load results.

A local build or a marketing page in source does not establish a live deployment.
Preserve existing printed redirect URLs when changing domains.
