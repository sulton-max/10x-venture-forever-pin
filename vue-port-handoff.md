# forever-pin frontend → Vue — handoff

*Last updated: 2026-08-15*

> **Current lane:** application migration is excluded from this work. Another chat owns the SDK update.
> SDK release and symbol-readiness claims below are dated snapshots; verify them before migration.
> **Recorded status:** not started — **gated on one SDK release**. Every symbol the app imports today already
> resolves (§2), but four groups of SDK work ship first: ~~the Google button~~ (done), the dependency
> majors, ~~the meta/OG gap~~ (done), and the components this app hand-rolls. That list is **§2b** — the
> working queue, 11 of 28 ticked.
> This doc is the whole brief — a fresh chat should need nothing else from the session that produced it.
>
> **What this is:** forever-pin is the pilot for a React→Vue move of the wow-two frontend SDK. The SDK port is
> done; rebuilding this app on it is the test of whether Vue is actually better to build with. That question
> — *how it feels to build* — is the deliverable, not just parity.

---

## 1 · The two sides

| | React (today) | Vue (target) |
|---|---|---|
| SDK | `@wow-two-beta/ui@0.0.97` | **`@wow-two-beta/ui-vue@0.0.5`** (npm, published) |
| App | `engineering/codebase/forever-pin.frontend-services` | same path, rebuilt |
| Size | 132 files · 6,667 LOC | — |

The Vue SDK is a complete port: 41 `foundation` modules · 18 primitives · 17 composables · 237 components
across 7 groups · `domain` · `feedback` · `analytics` · `flags` · `auth` · `forms-engine` (+2 adapters) ·
`router` · `query`. Nothing React had is missing. 405 SFCs compile, 1,327 tests pass, all seven component
groups render with zero error boundaries.

**Read before starting** (in the SDK repo,
`workbench/wow-two-sdk-beta/wow-two-sdk-beta.ui/engineering/codebase/wow-two-front-vue-beta-sdk/`):

- `MIGRATION.md` — 705 lines, built by diffing the two packages' source. The reference for every API delta.
- `engineering/pilot-readiness.md` — the pre-flight against *this app's* imports.
- `../../planning/vue-port-track.md` — how the port was built, and 11 house rules with the bugs behind them.

---

## 2 · Pre-flight verdict: unblocked on symbols

**99 of 99 symbols this app imports resolve from the built package** — runtime and types, across 14 subpaths.
Zero missing. Verified by importing all 99 by package name through the real `exports` map, with a negative
control (an impossible symbol per subpath produced exactly 14 × `TS2305`, so the clean run isn't vacuous).

No *existing* symbol is missing. What the app currently gets from **React-only deps** and from **its own
hand-rolled files** is a different question — that is §2b, and it is where the pre-port SDK work lives.

The app's SDK surface today, for reference (44 import lines, 15 subpaths):

| Subpath | Lines | | Subpath | Lines |
|---|---:|---|---|---:|
| `foundation/utils` | 20 | | `forms-engine` | 4 |
| `presentation/display` | 18 | | `domain/color` | 4 |
| `presentation/layout` | 17 | | `foundation/primitives` | 2 |
| `presentation/forms` | 17 | | `foundation/http` | 2 |
| `presentation/actions` | 17 | | `presentation/overlays` · `foundation/storage` · `forms-engine/tanstack` · `domain/emoji` · `styles.css` | 1 each |
| `presentation/feedback` | 8 | | | |

---

## 2b · SDK ship list — before the port starts

Owner split: everything here is the **SDK chat's** lane (`wow-two-sdk-beta.ui`), except §2b.5, which is the
port's own cleanup and needs no SDK change. Published target: one `@wow-two-beta/ui-vue` release carrying
2b.1–2b.4, then the port repins and starts.

Work one task at a time, top to bottom. `[x]` when the SDK is published with it.

### 2b.1 · Blocks the port — Google Sign-In · **shipped 2026-08-15**

The app's `@react-oauth/google` has no Vue drop-in, **and the SDK did not cover it on either side**:
`ui-vue/auth` ships `CookieStrategy` · `BearerStrategy` · `RedirectStrategy` · `AuthProvider` — no
sign-in button. Hits 2 files (`bootstrap/main.tsx`, `identity/components/GoogleSignInButton.tsx`).

- [x] `foundation/oauth`: `useGoogleIdentity` — script, `initialize`, callback
- [x] `presentation/actions`: `GoogleSignInButton.vue` hosts Google's button
- [x] Emits the ID token; app posts it to `/api/auth/google`
- [x] Client id as a prop; empty renders nothing, app stays guest-only
- [x] Script loads once per document, cached at module scope
- [x] 6 focused dom tests + the actions smoke tiers — 1,335 green

**Landed in `foundation/oauth`, not `auth`.** Two invariants ruled that: `auth`'s own barrel declares
it carries no UI, and ESLint's `boundaries/element-types` forbids `presentation → auth` outright
(`auth` is a standalone top-level layer). The GIS client knows nothing about sessions — it loads a
provider script and surfaces a credential — which is the definition of `foundation`, alongside
`share` / `speech` / `geolocation`. `presentation → foundation` is already a legal edge, so the button
hosts cleanly. `auth/index.ts` carries a pointer to it.

The app therefore imports from **two** subpaths, not one:

```ts
import { GoogleSignInButton } from '@wow-two-beta/ui-vue/presentation/actions';
// and, only if the app drives One Tap itself:
import { useGoogleIdentity } from '@wow-two-beta/ui-vue/foundation/oauth';
```

The emitted `credential` is a signed ID token, **not** a session — post it to `/api/auth/google` and
let the response drive `AuthProvider`. That is the same boundary the React app kept, so
`identity/`'s wiring ports unchanged apart from the import.

### 2b.2 · Dependency majors

`vue-router@5.2.0` is the **official next major** from `vuejs/router`, not a package rename — it folds
file-based routing in as an *optional* Vite plugin. `pinia`, `@pinia/colada`, `vite` and `@vue/compiler-sfc`
are all `optional: true` peers, so adopting v5 drags in **no** state library. It does raise the Vue floor to
`3.5.34`. The SDK's `router/` is 24 files, all on v4 APIs.

- [ ] `vue-router` peer `^4.0.0` → `^5.0.0` — official major, no rename
- [ ] Audit 24 `router/` files against v5; file-routing stays opt-in
- [ ] `vue` peer floor → `^3.5.34`, required by `vue-router@5`
- [ ] `lucide-vue-next` `^0.460.0` → `^1.0.0`; check icon-name drift
- [ ] `@tanstack/vue-form` peer `^1.0.0` → `^1.33.5`
- [ ] Declare the 3 optional `vue-query` peers the React package has

> React-side parallel, out of this port's scope: `lucide-react` is on `^0.460.0` against `1.31.0`, and the
> app pins `react-router-dom@^7.17.0` against the React SDK's `^7.18.1` peer. Neither blocks anything here.

**Router feature parity — checked, no gaps.** The React `router/` module exists because react-router misses
things; the concern was that the Vue port carried the wrapper but not the value. It carried both. Every one
of the React module's 21 exports has a Vue counterpart, and the Vue module adds three more:

| React SDK add-on | Vue | Note |
|---|---|---|
| `RoutePersistence` (last route → storage) | `installRoutePersistence` | Wired by `createAppRouter` by default; persist on, restore off |
| `DocumentTitle` · `DocumentMeta` | `installDocumentTitle` · `installDocumentMeta` | The one real gap is §2b.3, not the wrapper |
| `RouteAnnouncer` (a11y live region) | `RouteAnnouncer.vue` | |
| `NavigationProgress` + `ProgressProvider` | same, split out `NavigationProgressModes` | |
| `PageViewTracker` | `installPageViewTracker` | |
| `definePath` typed paths · `useTypedSearchParams` | same | vue-router 4 types params weakly; still earns its place |
| `requireAuth` · `buildReturnTo` · `useReturnTo` | same | |
| `useBreadcrumbs` · `usePrefetch` · `useNavigationBlocker` | same | Blocker gains a `BlockerState` enum |
| `lazyRoute` · `reloadOnChunkError` | same | |
| `AppErrorBoundary` (react-router `errorElement`) | `AppErrorBoundary.vue` | **Vue has no route error boundary at all** — rebuilt from `onErrorCaptured` + `router.onError` |
| `AppRoot` (the root layout route) | — | Correctly absent: vue-router installs as a plugin, so its contents became router hooks |
| — | `useRouteNavigating` | Replaces react-router's `useNavigation` |
| — | `RouteHandles` · pathless-layout synthesis | vue-router has no pathless layout route |

Shape difference worth knowing: React ships root behaviors as **components** mounted in `AppRoot`; Vue ships
them as **`install*(router)` hooks** that `createAppRouter` registers. Same behavior, idiomatic per side. No
task falls out of this table — the audit against v5 above still covers the module.

### 2b.3 · Meta / SEO gap · **shipped 2026-08-15**

`installDocumentMeta` wrote **`name=` tags only** and read a **static** `handle.meta` off the route record.
The app's `usePageMeta` (12 call sites) also writes `property=` OG tags, and `BlogPostPage` needs a title
derived from `:slug`. Porting as-is would have silently dropped OG tags from every marketing page.

- [x] `installDocumentMeta`: `og:*` keys write `property=`, rest `name=`
- [x] `handle.meta` + `handle.title` accept a `RouteHandleResolver`
- [x] A resolver returning `undefined` falls through to the parent match
- [x] 5 dom tests, incl. the blank-share-card case — 1,340 green

The `og:` prefix **is** the discriminator — no second field to set — because that is the split the Open
Graph spec and Twitter's docs already make (`og:*` is `property`, `twitter:*` is `name`). The resolver
runs on every navigation, so it stays synchronous: params and a module lookup, never a fetch.

```ts
{ path: '/blog/:slug', handle: { title: (route) => postTitle(route.params.slug) } }
```

`installDocumentTitle`, `installPageViewTracker`, and `RouteAnnouncer` all read `handle.title`, so all
three resolve it now — the compiler found the last two.

### 2b.4 · Components the app hand-rolls

**Decided 2026-08-15 — the marketing group goes in, and the trigger changed with it.** Extraction is judged
on *genericness, not consumer count*: a generic surface goes upstream the moment it exists, without waiting
for a second product to want it. Written into
[`frontend/shapes/app/architecture/architecture.md` § SDK extraction](../../../conventions/development/frontend/shapes/app/architecture/architecture.md).
That widened this list past the marketing kit.

The SDK's 7 presentation groups (`actions` · `display` · `feedback` · `forms` · `layout` · `nav` ·
`overlays`) have no marketing surface, so the kit lands as an 8th.

- [ ] `ColorModeToggle` into `presentation/actions` — 21 app lines
- [ ] Marketing group: `Section` `SectionHeading` `FeatureCard` `StepCard`
- [ ] Marketing group: `PricingCards` `ComparisonTable` `FaqList` `CtaBand`
- [ ] Marketing group: `BlogCard` + typed post registry
- [ ] `domain/color`: WCAG luminance + contrast ratio — 109 app lines
- [ ] `feedback`: `ContrastCallout` on top of that math, tone-driven
- [ ] `domain/emoji`: emoji picker control — SDK owns the data, not the UI
- [ ] `forms`: gradient + solid fill picker (`FillControls` 164 lines)
- [ ] `forms`: gradient preset strip — `GradientPresets`, 60 lines
- [ ] Audit `SelectField` in `fields.tsx` against the SDK's own field set

> `ContrastCallout` hand-rolls the WCAG relative-luminance curve in a product while the SDK is separately
> failing its own contrast audits (§2b.6). One implementation, in `domain/color`, serves both.

Staying app-side — these encode **what forever-pin is**: `Logo` · `RoutingDemo` · `HeroVisual` · `HeroCanvas` ·
`HeroSim` · `QrPreview` · `RuleControls` · `ShapeControls` (QR module / finder-eye shapes) · the per-content
`*Controls.tsx` set. The last one is a **revisit-after-port** call: vCard / WiFi / geo / calendar capture is
generic in principle, but the shapes are tangled with forever-pin's content registry today.

### 2b.5 · App-side, no SDK work

- [ ] Delete `ScrollToTop`; `createAppRouter` has `scrollRestoration`
- [ ] Delete `usePageMeta` once route-`meta` covers all 12 sites
- [ ] Swap `qrcode.react` → `qrcode` in `kit.tsx` — 1 file

### 2b.6 · Package hygiene, carried open

- [x] Repo `package.json` at `0.0.4`; npm at `0.0.5` — resynced
- [ ] Sourcemaps ~49% of the tarball — ship or strip
- [ ] `glass/warning` light mode 7.21 → 3.99 — keep or revert

---

## 3 · The work, by count

Measured against `src/**/*.tsx` as of this doc:

| Action | Sites | Fails how |
|---|---:|---|
| `onChange=` → `@value-change` / `v-model` | **47** | **silently** — see §4 |
| `className=` → `class=` | 225 | loudly, at compile |
| `<X.Y>` dot-access → flat import | 30 | loudly |
| `form.Subscribe` → reactive read | 14 | loudly |
| React-only deps | 20 files | loudly |

Layer sizes — the port is overwhelmingly a presentation-layer job:

| Layer | Files | LOC | Nature |
|---|---:|---:|---|
| `presentation` | 62 | 5,090 | the rewrite |
| `domain` | 49 | 759 | pure TS, copies verbatim |
| `bootstrap` | 5 | 292 | app wiring, `app.use(router)` |
| `integration` | 12 | 292 | API clients, copies verbatim |
| `application` | 2 | 229 | mostly `createCodeForm.ts` |

`src/presentation/` splits into `codes` (the builder, densest), `identity`, `marketing`, `billing`, `common`.

---

## 4 · The one trap that fails silently

**`onChange` — 47 sites.**

React's `onChange` fires **per keystroke**. The Vue SDK's inputs do not declare an `onChange` prop, so it
falls through as the **native DOM `change` event**, which fires **on blur**. A mechanical
`onChange` → `@change` rename **compiles clean and breaks every controlled field** — the value only updates
when focus leaves.

Proven by DOM mount: typing `a` into a field bound with `@change` fired nothing; `@value-change` fired
`value-change:a`.

Correct forms:

```vue
<TextInput v-model="name" />                          <!-- preferred -->
<TextInput :value="name" @value-change="name = $event" />
```

The payload is **the value**, not a DOM event — no `e.target.value`.

Everything else in §3 fails at compile or in an obvious way. This is the only one that will look fine and
be wrong.

---

## 5 · API deltas that matter here

Full list in `MIGRATION.md`. The ones this app hits:

**`forms-engine` — the biggest ergonomic change.** React's `<form.Subscribe selector={…}>{v => …}</form.Subscribe>`
render-prop is **gone**. Reading a value in a template *is* the subscription:

```vue
<!-- was 30 lines of 4-deep nested form.Subscribe in CreateCodeScreen.tsx -->
<PreviewView
  :preview-mode="form.values.mode"
  :preview-rules="form.values.rules"
  :preview-barcode-format="form.values.barcodeFormat"
  :preview-style="form.values.style"
/>
<Alert v-if="form.state.submitError" :message="form.state.submitError.message" />
<Button :is-loading="form.state.isSubmitting" @click="form.handleSubmit">Save</Button>
```

`form.useFormState(selector, isEqual?)` survives only for a composite slice needing a custom `isEqual`.
`<form.Field name="x" v-slot="f">` gives a writable `f.value`, so `v-model="f.value"` works.
**Do not destructure `form.state`** — its members are live getters; destructuring snapshots them.

**Controlled props.** Both spellings work, React's name wins:
`props.value !== undefined ? props.value : props.modelValue`. `v-model:open` works on every stateful root
alongside `:is-open` + `@open-change`.

**Compound dot-access is gone** for 31 of 33 components — `<Card.Header>` → `<CardHeader>`, imported flat.
Only `SpeedDial` and `Toolbar` keep the namespace.

**Node props became slots** where they were structural (`icon`, `actions`, `header`, `footer`). Scalar ones
(`label`, `title`) stayed props typed `string | number` **and** gained a same-named slot; the prop is the
discriminator.

**Composables take `MaybeRefOrGetter`** — pass a getter (`() => props.x`) to keep reactivity, not a snapshot.

---

## 6 · Dependency swaps

| React | Vue | Note |
|---|---|---|
| `react` / `react-dom` | `vue` ^3.5 | |
| `react-router-dom` ^7 | `vue-router` ^4 | `createAppRouter` survives; `element:` → `component:` per route, and `app.use(router)` replaces `<RouterProvider>` |
| `@tanstack/react-form` | `@tanstack/vue-form` | via the SDK's `forms-engine/tanstack` — the app should not import it directly |
| `lucide-react` | `lucide-vue-next` | 1:1 |
| `@react-oauth/google` | **no Vue equivalent** — 2 files | see below |
| `qrcode.react` | **React-only** — 1 file | `qrcode` (already a dep) renders to canvas/SVG directly |
| `zod`, `temporal-polyfill`, `qrcode`, `@fontsource-variable/*` | unchanged | framework-agnostic |

`@react-oauth/google` and `qrcode.react` are the only two with no drop-in. **Both are decided** (2026-08-15):

- Google Sign-In → **into the SDK**, shipped: `GoogleSignInButton` in `presentation/actions` over
  `useGoogleIdentity` in `foundation/oauth`. It was a gap the React side had too → §2b.1.
- `qrcode.react` → **app-side**, swapped for `qrcode` (already a dep) in `kit.tsx`. QR rendering is forever-pin's
  own business logic and does not belong in the UI SDK → §2b.5.

---

## 7 · Theme

The local Vue SDK still declares the theme ID `smart-qr` in
`src/foundation/themes/constants/Authored.ts`; the product rebrand does not rename that SDK contract.
Its CSS emitter uses `.theme-{id}`, so that ID maps to `theme-smart-qr`.
Verify the published package's theme ID and `themes.css` export when starting the migration.

The React app currently defines its theme in `src/bootstrap/index.css`.
Compare those tokens and rendered light/dark screens against the chosen Vue theme;
visual parity must be checked rather than inferred from the theme's historical validation status.

---

## 8 · Known SDK caveats

Fixed but worth knowing, because they shape what "correct" looks like:

- `Presence` (every overlay) was rAF-gated with no fallback; it now races the double rAF against a 32 ms
  timer. In a browser tab that is backgrounded, overlays still work.
- `soft` / `outline` tones were repointed to `-soft-foreground`. On non-white surfaces `success` and
  `warning` still measure ~4.3 and ~3.7 — under AA 4.5. That residual is a **theme-token ceiling**, open.
- `glass/warning` in **light** mode regressed 7.21 → 3.99 as a deliberate trade for cross-mode consistency.
  Open.
- `DateField` / `TimeField` / `DateTimeField` no longer use native browser pickers; `native` is the opt-in.
- The Claude browser pane runs pages with `document.hidden === true`, so `requestAnimationFrame` never
  fires there. **Verify visually in a real browser**, not the pane.

---

## 9 · Suggested order

0. **The SDK release carrying §2b.1–2b.4 lands, and the app pins it.** The port does not start on
   `0.0.5` — `identity/` and every marketing page depend on what that release adds.
1. ~~Decide the two dep swaps~~ — done, §6.
2. `domain/` + `integration/` — pure TS, copies with no changes. Cheap confidence. **Runnable during the
   SDK wait**, since neither layer imports the SDK.
3. `bootstrap/` — `createApp`, `app.use(router)`, theme class, providers.
4. `presentation/common/` — the shared shell and layout, then everything else composes.
5. `presentation/codes/` — the builder. `CreateCodeScreen` is the densest file and the best early read on
   whether the `form.Subscribe` collapse actually pays off.
6. `identity/`, `marketing/`, `billing/`.
7. Run it, walk every screen in a **real browser**, compare against the React app side by side.

The SDK is ours and beta-forever. A missing export or a wrong type is a fix in the SDK plus a version bump,
not a workaround here — that is the standing rule, and it applies during this port.

---

## 10 · Judging the pilot

The port's whole purpose is a fluency bet, not a parity exercise. Worth writing down as you go:

- Where did Vue take fewer lines, and where more?
- Did the `form.Subscribe` collapse feel as good as it reads?
- What did `v-model` simplify that `value` + `onChange` made noisy?
- What did you miss from React?

If it goes well, forever-pin stays on Vue and the rest of the products follow. If not, the React SDK is
untouched and still published.
