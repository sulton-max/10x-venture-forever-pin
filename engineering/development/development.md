# Development

*Last updated: 2026-09-13*

## Verification

```bash
bash engineering/scripts/verify.sh all
```

- `frontend`: TypeScript, Vitest, and production bundling.
- `unit`: backend pure-logic suite.
- `backend`: all four backend test tiers.
- `all`: frontend and backend.

Node is pinned in `.nvmrc`. The script uses that installed nvm runtime without changing global settings.
Docker must be running for PostgreSQL integration, migration, and end-to-end tests.
Provider boundaries are faked for Google and Stripe; tests do not verify live provider configuration.

---

## Native permissions

- Serial MSBuild and disabled compiler/build servers avoid unnecessary helper processes.
- VSTest still uses local sockets. PostgreSQL test containers use the Docker daemon.
- In a restricted Codex sandbox, request native approval for the verification command.
- Keep one permission request pending at a time; wait for its result before another approved action.
- A dismissed or superseded request establishes no execution result. Reissue the same bounded command.
- No global permission, trust, keychain, or sandbox setting needs to be disabled.

Official boundary: [Codex security](https://learn.chatgpt.com/docs/security).

---

## HTTPS

`mkcert` runs only for frontend development. Builds and tests never invoke certificate installation.
`VITE_HTTPS=false` is the explicit dev-server HTTP opt-out.
Normal browser auth development uses HTTPS and the existing trusted development certificate.

---

## Evidence

Fresh results belong in [verification](../operations/verification.md).
Manual version checks remain unticked until the owner verifies the corresponding user flow.

## Frontend workspace

The private pnpm root is `engineering/codebase/forever-pin.frontend-services/`.
The shipped app owns its source, configuration, tests, and `dist/` under `apps/web/`.
`pnpm dev`, `pnpm typecheck`, `pnpm test`, and `pnpm build` run from the workspace root.
`pnpm run deploy` builds the web app and copies its output into the API host's generated `wwwroot/`.
Docker copies the same app-local output into the management image.
The backend `BuildSpa` target calls `pnpm run deploy`; ordinary frontend builds leave backend output untouched.
Local Vite overrides live in `apps/web/.env.local`; use `apps/web/.env.example` as the starting point.
Peer apps belong under `apps/`; introduce `packages/` only for code shared by multiple apps.
