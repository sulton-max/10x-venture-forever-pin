# Codebase

- `forever-pin.backend-services/`: .NET 10 solution, two API hosts, and four test tiers.
- `forever-pin.frontend-services/`: private pnpm workspace; `apps/web/` owns the product app, tests, and Vite output.
- Workspace commands stay at this root as peer apps are added; shared `packages/` are introduced only when needed.
- [Development](../development/development.md) owns runtime and verification instructions.
