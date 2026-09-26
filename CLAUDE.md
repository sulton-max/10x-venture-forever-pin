# ForeverPin

QR, barcode, and programmable-link product. Brand: **ForeverPin**; domain: `foreverpin.com`.

## Instructions

- Workspace conventions live in `wow-two-ws/conventions/`; repo-specific details follow here.
- `.claude/rules/file-references.md` is a lazy document index.
- Preserve existing edits; coordinate by disjoint file sets in the shared checkout.
- Vue application adoption is active in `v0.10`; backend migration belongs to a separate chat.

## Codebase

- Backend: `engineering/codebase/forever-pin.backend-services/` — .NET 10.
- Frontend: `engineering/codebase/forever-pin.frontend-services/` — React 19, Vite, Tailwind 4, `@wow-two-beta/ui`.
- Namespace: `ForeverPin`; product folder: `10x-venture-forever-pin`.
- Management host: `ForeverPin.Api`, HTTPS/HTTP `7020`/`7021`.
- Redirect host: `ForeverPin.Redirect.Api`, HTTPS/HTTP `7022`/`7023`.
- Frontend dev: HTTPS `7024`; pnpm is the package manager.

## Verification

- `bash engineering/scripts/verify.sh all` runs frontend checks and all backend suites.
- Node version: `.nvmrc`; the verifier selects that installed runtime.
- Full backend tests require Docker and local runner sockets.
- Restricted execution requires native approval for that command; do not weaken global permissions.
- See `engineering/development/development.md` for the approval workflow.

## Planning

- Active release: `engineering/planning/version-track/v0.10/v0.10.md`.
- Remaining tasks: `engineering/planning/planning.md` and the active track.
- Durable content decisions: `engineering/architecture/content-model.md`.
- Rename compatibility: `engineering/operations/rebrand.md`.
