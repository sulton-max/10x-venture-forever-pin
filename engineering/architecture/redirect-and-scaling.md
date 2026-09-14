# Redirect and scaling

*Last updated: 2026-09-13*

## Implemented path

`GET /{slug}` → database lookup → context → rule evaluation → scan enqueue → `302`.

- `DbRedirectCodeRepository` reads PostgreSQL on each scan.
- An edit is visible to the following scan without cache invalidation.
- `CachedRedirectCodeRepository` exists but is not registered.
- `ChannelScanRecorder` queues events without awaiting the database write.
- `ScanFlushBackgroundService` persists batches and updates the scan counter.
- Analytics is best-effort; queue overflow can drop events.
- No matching destination returns `404`.

---

## Planned scaling

- Cache routing configuration with edit invalidation when traffic warrants it.
- Add a Redis read/write path before claiming Redis-backed production routing.
- Activate country lookup through a local geo database.
- Measure burst traffic, unknown-slug traffic, queue loss, and database load.
- Add CDN/TLS and operational evidence before claiming deployment readiness.

Image exports are rendered on demand. Object storage and immutable CDN delivery are future infrastructure.
Customer analytics reporting is separate from scan collection.

---

## Source

Implementation lives under `ForeverPin.Redirect.Api/Infrastructure/`.
Registration lives in `ForeverPin.Redirect.Api/Configurations/HostConfiguration.Extensions.cs`.
Behavior: [routing engine](routing-engine.md). Deferred work: [backlog](../planning/backlog.md).
