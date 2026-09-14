# Frontend

*Last updated: 2026-09-13*

## Stack and layers

- React 19, TypeScript, Vite, Tailwind 4, and `@wow-two-beta/ui`.
- `src/domain`: content, rule, identity, and billing models.
- `src/application`: form mapping and application operations.
- `src/integration`: API requests and transport.
- `src/presentation`: content/design/routing controls, screens, and marketing.
- `src/bootstrap`: app routes, layouts, startup, and theme.

---

## Behavior

- Marketing routes render independently of the management API.
- Application routes manage identity, codes, builder, and billing.
- Create/edit forms carry explicit mode and content-bearing rules.
- Update serialization omits mode; edit-state validation still needs the stored-mode constraint.
- Type switches reseed rule content.
- Copy starts a prefilled create flow; the single target-mode dialog remains unbuilt.

---

## Tooling

- Node 24.11+; pnpm only.
- `pnpm typecheck`, `pnpm test`, and `pnpm build` are independent checks.
- Only dev serving initializes mkcert.
- Production output: `../forever-pin.backend-services/ForeverPin.Api/wwwroot`.

---

## Vue lane

The application is still React. [Vue handoff](../../vue-port-handoff.md) retains migration API deltas.
Another chat owns the SDK update; the current lane excludes application migration.
Verify the published SDK before consuming it.
