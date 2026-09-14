# Design session — handoff

> Historical design handoff from 2026-08-15. Current work is recorded in [the engineering plan](engineering/planning/planning.md).

*Last updated: 2026-08-15*

> UI/design + SDK-frontend session for **ForeverPin**. Design system locked + adopted; SDK motion
> layer shipped (`@wow-two-beta/ui@0.0.64`) + adopted in the app. This doc is the **leftover tail**
> for a fresh chat. Logic / backend changes stay out of scope (separate chat).

## Status — done this session

- ✅ **Design system locked** — canvas `#D9DDE8` · tinted cards `#EBEEF4` · violet brand `#7C3AED` · teal accent `#0D9488` · Geist / Geist Mono. Dark = neutral charcoal (no cyberpunk).
- ✅ **App adopted** — `@theme` + Geist, tabbed builder (`Destination · Style · Center · Routing`) + sticky preview, dense `RuleBuilder` on SDK `Sortable`, dark mode via `ColorModeProvider`, preview color-lag fix.
- ✅ **SDK extensions shipped + adopted** — `accent` token · `Card` ambient · `Sortable` · `ColorModeProvider` · input `border`/`ring` scale variants (default = prior look) · `ToggleButton` `tooltip` prop (group-safe).
- ✅ **SDK motion layer** — tokens (`--duration-*` / `--ease-*`) + keyframes (fade / pop / slide) + **27 components** on `Presence` + `data-state`; the 12 silently-**dead** phantom animations revived. Published `0.0.64`.
- ✅ **App motion adopted** — builder **tab cross-fade** (`key={tab}` + `motion-safe:animate-(--animate-fade-in)`) + overlays / menus / dialogs / selects auto-animate. Verified: typecheck + build clean; compiled app CSS carries `@keyframes fade-in` + `--animate-fade-in` + `animation:var(--animate-fade-in)`.

## Left — pick up in a new chat

### 1. P3 motion — feedback components (SDK) · optional, small
Appear / dismiss feedback components still static — give them enter/exit + dismiss-collapse. Audit (a few may already animate from the pass):

- `feedback/banner/Banner.tsx` · `feedback/bannerSimple/BannerSimple.tsx`
- `feedback/callout/Callout.tsx`
- `feedback/…/Alert.tsx` · `AlertSimple.tsx`
- `feedback/toastSimple/ToastSimple.tsx` · `feedback/feedbackToasts/FeedbackToasts.tsx`

Pattern: wrap in `<Presence>`; `data-[state=closed]:animate-(--animate-fade-out)` on exit; height-collapse on dismiss (`grid-rows-[1fr]` → `[0fr]`); `motion-safe:` / `motion-reduce:animate-none` guards. Mirror `display/collapsible/Collapsible.tsx`.

### 2. Motion docs (SDK) · optional
Undocumented and worth capturing:
- **Convention** — `data-state` + motion tokens + `Presence` for real exits; `motion-safe`/`motion-reduce` always.
- **Dead-animation finding** — 12 components shipped phantom `animate-in fade-in-0 zoom-in-95` with **no** plugin/keyframes → animations were silently dead. Guard against reintroduction.

Old target `docs/analysis/ui-philosophy/targets.md` is **gone** after the relocation (below) — choose a new home.

### 3. App `design-research.md` Motion section · optional
`forever-pin/platform/research/design-research/design-research.md` has no motion section. Add: token table + fade-through-on-tab rule + "overlays animate via SDK `Presence`" so the app spec is complete.

### 4. User-side · not mine
- **Visual QA in your own browser** — dev server is HTTPS:7024 (mkcert), which blocks the harness preview; I verified via build + compiled-CSS grep only, never a live render.
- **Commit** the uncommitted app + SDK changes (git stays yours).

## Heads-up — SDK UI repo relocated (today, another lane)

Source moved to the conformant layout:

```
wow-two-sdk-beta.ui/src/…
  → wow-two-sdk-beta.ui/engineering/codebase/wow-two-front-beta-sdk/src/presentation/…
```

`0.0.64` was published around the move, so the motion build on npm is intact. A new chat editing SDK source: confirm the relocation has settled, then use the new paths. Groups under `src/presentation/`: `actions · display · feedback · forms · layout · nav · overlays`.

## Pointers

| What | Path |
|---|---|
| Design spec (app) | `forever-pin/platform/research/design-research/design-research.md` |
| Method / convention | `wow-two-ws/conventions/design/research/design-exploration.md` |
| Release log | `forever-pin/platform/versions/v0.5/v0.5.md` (Iteration 6) |
| SDK front src root | `wow-two-sdk-beta.ui/engineering/codebase/wow-two-front-beta-sdk/src/presentation/` |
| App consumes | `@wow-two-beta/ui@0.0.64` (`platform/src/frontend/package.json`) |
