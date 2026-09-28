# Frontend

*Last updated: 2026-09-28*

## Stack and layers

- Vue 3, TypeScript, Vite, Tailwind 4, and `@wow-two-beta/ui-vue@0.0.7`.
- `src/domain`: content, rule, style, identity, and billing models; contrast checks.
- `src/application`: form schema and request mapping, builder lifecycle, session gate, queries and mutations.
- `src/integration`: API endpoints, zod wire codecs, the antiforgery fetch, and the session bridge.
- `src/presentation`: marketing pages, the codes list and builder, billing, and the sign-in gate.
- `src/bootstrap`: entry, root providers, layouts, and the route table.
- ESLint enforces layer and slice boundaries; `presentation/common` and `integration/common` import no slice.

---

## Behavior

- Marketing routes never call the API; the session is checked only when `/app` opens.
- The sign-in gate renders in place, so a deep link survives sign-in.
- A failed session check offers a retry instead of creating a new guest.
- A new guest or account clears private queries through the SDK session bridge.
- Each list search owns a cache entry; row commands track their own progress.
- Opening another code starts a fresh builder session; a failed load blocks editing.
- Content leaves bind at their own paths, so server field errors land on their controls.
- Conditionals reorder by drag or keyboard; the catch-all stays last and a pointer names its target.
- A content-type switch keeps what was typed for each type.
- Barcodes render plain, so their builder hides style controls and keeps the style.
- The preview is an image of the server's SVG; a dynamic draft is labelled a style sample.
- A save shows the saved image and downloads apart from the draft preview; later edits read as unsaved.
- Leaving a dirty builder asks first.
- A Checkout return stays provisional until the billing snapshot confirms a paid plan.

---

## Tooling

- Node 24.11+; pnpm only.
- `pnpm typecheck`, `pnpm test`, and `pnpm build` are independent checks; typecheck covers tests and SFCs.
- `pnpm --filter @foreverpin/web lint` runs ESLint, including the boundary rule.
- Only dev serving initializes mkcert; `VITE_HTTPS=false` serves HTTP.
- Production output: `../forever-pin.backend-services/ForeverPin.Api/wwwroot`.

---

## SDK bridges

- `integration/common/AntiforgeryFetch.ts` echoes the SPA antiforgery token until the SDK client offers it.
- `integration/common/models/ApiWarningDto.ts` reads envelope warnings until the SDK client decodes them.
- Modals use a plain cancel button: the published `AlertModalCancel` renders as a corner close icon.
