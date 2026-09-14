# Architecture

*Last updated: 2026-09-13*

## Runtime

- .NET 10 management API serves the React frontend and authenticated application endpoints.
- A separate redirect host serves dynamic codes from PostgreSQL.
- Product layers: domain, application, infrastructure, persistence, and shared libraries.
- Generic rendering, migrations, identity helpers, and test infrastructure come from the backend SDK.
- SQL files own schema changes; EF Core maps the schema.

---

## Contracts

- [Content model](content-model.md): mode, rule roles, payloads, and unresolved delivery decisions.
- [Routing engine](routing-engine.md): condition evaluation and fallback behavior.
- [Redirect and scaling](redirect-and-scaling.md): database path and asynchronous scan collection.
- [Code generation](code-generation.md): preview and downloads through the SDK renderer.
- [Billing](billing.md): provider boundaries and creation-time limits.
- [Frontend](frontend.md): React layers, forms, routing, and the separate Vue lane.

Historical rationale: [decision record](decisions.md).
