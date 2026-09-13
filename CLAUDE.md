# ForeverPin

Dynamic QR / barcode / link platform — programmable routing ("one code, many destinations by context") + "codes never expire." Micro-SaaS portfolio product #002. Brief → `wow-two-ws/ideas/forever-pin-spec.md`.

## Lazy loading

- Open a file only when the task needs it; `.claude/rules/file-references.md` is a lookup table, not a reading list.
- Navigate source via `tree`/`find`/`grep` — `.cs` files aren't indexed.

## Structure

Two top-level layers (per `conventions/development/repo/structure/repo-structure.md`):

- **`product/`** — venture layer: model, pricing, positioning, GTM.
- **`engineering/`** — technical layer: `codebase/` (the .NET + React services) · `architecture/` · `planning/` (incl. `version-track/`) · `development/` · `deployment/` · `operations/` · `research/`.

Backend (`engineering/codebase/forever-pin.backend-services/`) — refs go product → platform, never reverse:

| Project | Role |
|---|---|
| `ForeverPin.Common*` | shared libs — mediator/settings · domain entities · EF Core + SQL migrations |
| `ForeverPin.Platform.*` | SDK-bound infra (mediator/result/config · migrator · E2E harness) → extracts to backend-beta |
| `ForeverPin.Codes` | code generation (QRCoder / ZXing / Svg.Skia) → extracts to backend-beta SDK (`…Beta.Codes`) in v0.6 |
| `ForeverPin.Api` | management API · https **7020** |
| `ForeverPin.Redirect.Api` | redirect hot path · https **7022** |
| `ForeverPin.Tests.{Unit,Integration,E2E,Migrations}` | xUnit — pure-logic units · repo/DB integration · full-API E2E (Testcontainers PG) · migrator engine |

Frontend (`engineering/codebase/forever-pin.frontend-services/`) — React 19 + Vite + Tailwind v4 + `@wow-two-beta/ui`; **pnpm** (not npm — `workspace:` protocol); https dev server via mkcert (even port 7024).

## Conventions

- All code / architecture / ports / docs conventions live in **`wow-two-ws/conventions/`** (index: `conventions.md`) — follow them, never restate here.
- Repo specifics only: single `https` profile per service binding two ports — HTTPS even + HTTP odd (Api `7020`/`7021` · Redirect `7022`/`7023`); TLS upstream in prod. Allocations → `conventions/deployment/hosting/ports.md`.
