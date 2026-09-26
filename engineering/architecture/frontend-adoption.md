# ForeverPin frontend source audit and comparison candidate

*Last updated: 2026-09-26*

## Evidence boundary

- baseline: `ae04e2b940bf15f4461fe980e9f10fd11c839086`; repository was clean when this audit started
- repository: `workbench/ventures/10x-venture-forever-pin`
- frontend at that commit: `engineering/codebase/forever-pin.frontend-services`; source references below are relative to this frontend unless prefixed `backend`
- scope: bootstrap, routing, identity, dashboard, create/edit/copy builder, all ten content control groups, rule controls, design controls, preview, billing, marketing composition, theme, fonts, and animation lifecycle
- source inspection and color arithmetic only; no frontend edits, tests, runtime changes, index operations, or browser interactions performed by this audit lane
- baseline mobile Content and desktop Design screenshots were subsequently inspected from `foreverpin-visual-20260926`; parent measured document width 606px at a 390px viewport, confirming horizontal overflow
- computed theme mapping, keyboard behavior, real backend responses, and after screenshots remain the parent lane's verification
- the root lane may move the frontend after this audit; source locations remain reproducible through `git show ae04e2b:<path>`
- published adoption candidate: `@wow-two-beta/ui-vue@0.0.7`; public tarball verified separately, with 72 exports and all 140 export targets present

## Decision

Preserve the current two-tab builder and improve correctness while porting it to Vue. Compare the captured baseline against one restrained candidate: clearer text and surfaces, aligned page gutters, fewer redundant controls, and a usable narrow-screen preview. Do not revive the older three-tab design or replace the joined rule editor with a different information architecture.

The existing design document is partly historical: it specifies three tabs, an older nested-condition contract, 400/500-only type, and an older palette mapping, while current source implements two tabs and one condition per rule. Baseline source plus the user's current comparison request governs the candidate; update the design document only after a visual choice.

## Priority findings

### Correctness to fix during the Vue adoption

1. **Fallback pointers can silently change destination after reordering.**
   - `src/application/codes/createCodeForm.ts:192` renumbers conditional rules sequentially but leaves `DefaultPointer.targetOrder` unchanged.
   - `src/presentation/codes/routing/components/RuleControls.tsx:72` permits arbitrary movement; pointer rows have no visible target explanation or editor at `:128`.
   - backend `ForeverPin.Redirect.Api/Infrastructure/Routing/RoutingService.cs:38` resolves the fallback by the persisted numeric order.
   - example: conditional A order 1, B order 2, pointer target 1; reorder B above A, save, and the unchanged pointer now selects B.
   - repair: use stable editor row identities; map old conditional order to new order at serialization; explicitly handle deletion of a referenced row. Render a readable fallback target. Keep the fallback visually last because backend evaluates it after every conditional regardless of array position.
   - acceptance: reorder/insert/delete preserves the same logical fallback destination or surfaces an explicit invalid-target correction.

2. **Dynamic previews are presented as the downloadable final symbol, but encode a placeholder.**
   - `src/presentation/codes/common/createCode/views/PreviewView.tsx:67` claims the preview is the final asset.
   - backend `ForeverPin.Api/Controllers/CodesController.cs:30` always uses slug `preview` for dynamic preview requests.
   - `CreateCodeScreen.tsx:219` supplies live draft values even after a save; download links at `PreviewView.tsx:106` fetch the last persisted record.
   - repair: label an unsaved dynamic preview as a style sample; after saving show the actual saved image/short URL. Mark later edits unsaved and distinguish draft preview from last-saved downloads.
   - acceptance: decoded saved-preview and downloaded payloads match; changing a draft never implies that the last-saved download contains those edits.

3. **Builder route changes and failed loads leave unsafe stale state.**
   - `CreateCodeScreen.tsx:94` returns immediately when `sourceId` becomes absent without resetting values, `existingCode`, `saved`, `copiedInto`, or errors.
   - the same component handles create/copy/edit; React and Vue can reuse it while route parameters change.
   - after a failed edit/copy GET, loading ends and the ordinary editable form renders with the error below the submit button (`:203`).
   - repair: explicit create/edit/copy state transitions with complete reset, cancellation, and a blocking load-error state with retry/back. On save, establish a fresh dirty baseline.
   - acceptance: edit A → edit B, copied A → blank create, failed load, and back/forward navigation never submit values belonging to another record.

4. **Search and mutations can overwrite each other with stale results.**
   - `CodesListScreen.tsx:61` accepts every list response; debouncing at `:74` cancels only timers, not issued requests.
   - `busyId` at `:58` tracks one operation; starting operations on different rows permits the first completion to clear the second row's busy state.
   - stale list responses can restore deleted rows or old active states.
   - repair: SDK query ownership/cancellation and request-generation guards; per-record mutation state; invalidate or reconcile current-query data after mutations.
   - acceptance: slow old query cannot replace a newer query; deleting during a refresh cannot resurrect a row; independent row operations remain disabled until their own completion.

5. **Billing renders claims unrelated to verified subscription state.**
   - `BillingScreen.tsx:147` declares the subscription active solely because `?status=success` is present.
   - `:180` labels every non-Free plan active, ignoring `BillingStatus.status`.
   - on an initial load failure, defaults at `:130` render Free, 0/0 usage, a limit warning, and upgrade actions beneath the error.
   - repair: separate pending confirmation, verified subscription, and unavailable states. Show backend status; refresh with a bounded retry after Checkout return. Do not substitute a fabricated Free snapshot when billing could not load.
   - acceptance: arbitrary success query, delayed webhook, past-due/cancelled subscription, and failed GET remain truthful.

6. **Int64 scan counts still cross a native-number boundary.**
   - frontend `src/domain/codes/common/models/CodeDto.ts:34`: `scanCount: number`.
   - backend `ForeverPin.Application/Codes/Core/Models/CodeDto.cs:36`: `long ScanCount`.
   - integration `readData` uses legacy SDK `parseJson`; the Vue migration must explicitly select the new lossless transport codec and DTO decoding.
   - repair: use the published SDK's lossless JSON and Int64 representation at the boundary and formatter. Do not add a safe-integer product cap.
   - acceptance: `9007199254740993` and Int64 bounds round-trip and display exactly; ordinary count formatting remains readable.

7. **Identity network failures are treated as anonymous; account changes do not invalidate product state.**
   - `AppLayout.tsx:42` converts every identity error into the guest/login gate.
   - header Google sign-in only replaces `me` at `:89`; existing code/billing screens receive no identity-change signal.
   - `handleSignOut` at `:54` has no busy/error path; header Google failures have no `onError` consumer.
   - repair: use Vue auth ownership with distinct resolving/anonymous/authenticated/error states; invalidate identity-bound queries on guest conversion/logout. Retain deep-link destination through the gate.
   - acceptance: transient API outage offers retry without creating a new guest; conversion refreshes owned codes; sign-out failure stays visible.

### Form and interaction improvements

8. **Preview freshness is delayed until the next debounce fires.** `QrPreview.tsx:58` aborts an old request inside the future timer, while effect cleanup at `:84` only clears the timer. An old response can therefore commit after inputs changed and before the next request starts. Invalidate immediately on input change and gate responses by generation. Preserve the last image only as explicitly stale while rendering.

9. **Static edit permits rules it cannot persist.** `ContentView.tsx:49` displays Dynamic when multiple rules exist; `RuleControls` has no persisted-mode input and always offers another rule. Update serialization omits mode correctly, so the backend can reject the resulting static/multiple-rule request. Preserve the existing immutable mode and disable the incompatible action with inline guidance. This is already an open product follow-up, not a newly discovered backend defect.

10. **Type changes discard authored content without a recovery path.** `ContentView.tsx:75` immediately replaces every rule's content with empty values. Preserve per-type drafts or confirm only when authored values would be lost. A content-type switch should never require retyping merely to inspect another type.

11. **Geometry inputs turn a cleared value into an actual coordinate.** `GeoControls.tsx:16` and `:26` use `valueAsNumber || 0`; blank/invalid edits become 0. Retain an empty/invalid draft until validation. Latitude/longitude bounds are real domain limits and remain appropriate; they are not a workaround for integer precision.

12. **Saved and dirty state lack a dependable user signal.** A save leaves the form editable while success/download actions remain visible; "Create another" only clears the saved panel. Preserve intentional duplicate creation, but distinguish save completion, unsaved edits, and the start of a new item. Add route-leave protection only for an actual dirty draft, with explicit discard/save behavior.

13. **Content validation is intentionally coarse.** The schema validates shapes and enums, not content semantics. Ten content groups expose only `value/onChange`, so server leaf errors are collapsed to the object path. The historical checklist records this as deliberate. Vue adoption should bind leaf paths and touched/blur/error state through the SDK field API, focus the first invalid field, and retain backend authority. Add only useful immediate validation matching the current backend contract.

14. **Design controls do not follow symbology capabilities.** `DesignView.tsx:25` allows every format but always offers QR eyes, module shape, and center emoji. Explain or disable irrelevant controls for non-QR formats; retain values when switching back. Quiet-zone/ECC/logo fields exist in the model but are not exposed; do not introduce those product features merely because the model supports them.

15. **Gradient toggling resets the user's custom gradient.** `FillControls.tsx:52` seeds a new two-stop gradient every time Gradient is chosen. Preserve a last-used gradient draft; use the SDK gradient model and picker without losing the current concise preset rows. QR contrast guidance remains advisory and product-owned; do not equate a WCAG text ratio with proven scanner success.

16. **Content and static-code language is misleading in several places.** Mobile-app help at `ContentTypeDescriptor.ts:21` promises two-store routing while the form edits one store/URL; delete confirmation says a printed static code stops resolving (`CodesListScreen.tsx:263`); the edit heading promises all printed codes keep working (`CreateCodeScreen.tsx:157`). Explain static versus dynamic behavior at those decisions; do not expand the content model to match outdated copy.

## Visual and responsive evidence

### Source-derived findings; browser validation pending

- **Muted text is too close to several surfaces for small text.** Calculated from current literal tokens using sRGB relative luminance: `#6e7188` on canvas `#d9dde8` = **3.53:1**, on card `#ebeef4` = **4.12:1**; subtle `#9b9fb5` on card = **2.25:1**. Dark subtle text `#6e6e76` on card `#1c1c1f` = **3.36:1**. The dark primary pair `#f5f3ff` on `#8b5cf6` = **3.86:1**. These are token-pair calculations, not a browser accessibility verdict; applied roles and computed CSS still need inspection.
- **Geist Mono is loaded but never used by product markup.** `index.css:38` defines it; no source element has `font-mono`. Short URLs, code destinations, hex values, and identifiers should receive the selected numeric/mono treatment; otherwise drop the extra font. Retain Geist as the primary face for the comparison.
- **Desktop builder proportions and gutters diverge.** Builder uses equal columns at `lg`, 24px gaps/padding (`CreateCodeScreen.tsx:165`), while the older spec calls for more editor width. App navbar and main have mismatched horizontal padding acknowledged at `AppLayout.tsx:62`. The solution is a shared content container, not independent per-screen margin patches.
- **Narrow layouts have explicit pressure points.** App header has a long nonwrapping account/action row (`AppLayout.tsx:74`); marketing hides its nav below `sm` without a replacement (`MarketingLayout.tsx:28`); builder preview is after the entire editor on mobile; shape eyes remain paired even when narrow (`ShapeControls.tsx:64`); gradient rows use a fixed side selector plus tiles (`FillControls.tsx:118`). Verify long names and error strings at 360/390px before choosing a breakpoint.
- **Elevation is not governed by one role.** `.surface-soft` provides three shadows at `index.css:72`; cards do not explicitly select the flat variant requested by its comment. Dark removes the shadow, and the defined `.surface-sheen`/`.surface-glow` classes are unused. Hero uses a separate colored `shadow-xl`. Inspect the SDK's effective default, then choose one product surface/elevation mapping.
- **Marketing developer controls ship unconditionally.** `HeroSim.tsx:56` renders tuning sliders on the public landing page without a development gate. Remove them from the public comparison; keep tuning in development tooling.
- **Canvas work can be reduced without redesign.** `HeroCanvas.tsx` repeatedly calls `getBoundingClientRect` through `viewport()` within each frame, bakes duplicate URL textures, and only resizes on width changes (`:449`). Cache dimensions in ResizeObserver, track height too, bake distinct payloads once, and catch texture-generation failure. Preserve reduced-motion, visibility pauses, cleanup, and the current visual mode.
- **Public pages are coupled to product availability and bundle size.** `main.tsx:12` waits for runtime config before rendering any route; failure blanks marketing as well as the product. `App.tsx` eagerly imports all screens; blog index eagerly imports article bodies. Mount public routes independently, lazy-load product/billing/blog bodies, and resolve product configuration where needed.
- **Marketing promises exceed visible app capabilities.** Current data advertises custom domains, full analytics, A/B, bulk generation, API keys, white-label, client workspaces, and one-click export. The inspected frontend exposes none of those screens. Keep the surface during migration, but label or remove unshipped claims according to the current release scope rather than treating them as established capability.
- **Metadata and navigation need route ownership.** `meta.ts:10` leaves the prior description if a new route has none; app screens do not set titles. `ScrollToTop` resets every pathname and has no back-navigation/hash restoration. Move both to the Vue router integration and verify direct links, back navigation, and unknown paths.

## Small comparison candidate

### Immediate trial boundary

The first comparison is limited to `src/bootstrap/index.css` and `src/bootstrap/AppLayout.tsx` after the root lane relocates the frontend. Adjust token contrast, product typography/elevation, shared gutters, and header wrapping there. Keep builder components, topology, form semantics, and Vue migration untouched in this trial so the visual choice remains attributable. The wider improvements below are separate follow-up candidates, not one inseparable patch.

### Hold these constant

- Geist primary font, violet brand, neutral charcoal dark mode, restrained teal status dots
- Content and Design tabs; current design accordion; Body grid and paired Eyes controls
- joined ordered rule editor, catch-all semantics, one primary save/create action
- server-owned code rendering and current supported content types
- marketing section order and actual authored content, except developer-only controls and demonstrably inaccurate status text

### Candidate changes

1. **Surfaces and type:** retain lavender canvas and tinted cards; darken meaningful muted text enough for its actual size, move low-contrast subtle color to decorative roles, and separate violet primary fill from violet links/focus if necessary. Keep product body/field text readable at 14px, helpers 12–13px, product heading about 24px/600; do not copy the marketing headline scale into the builder. Treat exact values as candidates until screenshots and computed contrast are checked.
2. **Elevation:** use one light card shadow, e.g. `0 1px 2px rgb(28 29 38 / .04), 0 6px 18px rgb(28 29 38 / .04)`, with an explicit SDK surface variant. Use a neutral dark border/surface separation instead of colored glow. Reserve stronger elevation for popovers/modals.
3. **Builder frame:** unify header/main gutters; give the editor approximately `minmax(0,1.25fr)` and preview `minmax(18rem,.8fr)` where both fit. Preserve the sticky preview. At narrower widths, use a compact preview summary with an explicit expand action before the editor, avoiding a second competing primary action. Determine the actual collapse point from content fit, not the old spec's 720px number.
4. **Controls:** preserve current grouped design density; align label/value baselines and control heights. Show the selected shape name alongside glyph groups when needed. Use wrapping tile grids and a single-column Eyes fallback at narrow widths. Replace nested interactive toggle/swatch composition with independent accessible controls.
5. **Product state:** header has a compact account menu on narrow screens, dashboard retains visible results during background refresh, and save/preview/error states occupy stable reserved space. This changes clarity, not the product's visual identity.

Tradeoff: this candidate prioritizes predictable editing and readable contrast over the current extremely soft, low-contrast appearance. The baseline remains available for comparison; the user chooses before this becomes the locked design.

## Port boundary

| Area | Reuse | Replace / adapt |
|---|---|---|
| Domain | enums, discriminated content/rule models, empty-content factories, default styles, billing/identity contracts | `scanCount` exact type; update SDK gradient imports; explicit Temporal and JSON boundary decoding |
| Application | schemas and request-mapper intent | fix pointer remapping; immutable clone/draft semantics; leaf error mapping; extract pure rule transitions from JSX |
| Integration | endpoint paths, credentialed-cookie behavior, problem-details semantics | SDK Vue HTTP client/codec; typed response validation; cancellation; identity-bound query ownership |
| Forms | current field names and content-specific layout | replace React render props and TanStack hook glue with Vue form slots/composables; keep adapter choice behind `form.ts` |
| Controls | product labels, presets, shape glyph geometry, condition displays | Vue `SegmentedPicker`, `ControlGroupField`, `OptionTilePicker`, `SortableGroup`, color/emoji/date controls from published SDK; verify current prop contracts |
| Identity | guest → registered account flow and backend token exchange | Vue auth/cookie strategy and Google sign-in component; central session ownership/error handling |
| Routing | public/product route hierarchy and edit/copy URL contracts | Vue Router lazy routes, typed params/query parsing, metadata/scroll behavior, dirty-draft navigation |
| Preview | product renderer request and debounce intent | Vue lifecycle + immediate cancellation/generation ownership; accessible loading/stale/error states; safe SVG embedding policy |
| Marketing | pricing/feature/article data and section order | split icon/component descriptors from data; port JSX article bodies to Vue; lazy route bodies |
| Hero | QR textures and physics as framework-neutral functions | Vue canvas lifecycle wrapper; size/visibility observers; no public tuning panel; deterministic comparison setup |

The published `0.0.7` tarball already exposes the listed Vue form and layout families. QR rendering remains ForeverPin-owned; no SDK QR component or React compatibility layer is required. CSS selector/token compatibility must be checked against the published Vue stylesheet instead of mechanically copying the legacy React import and `@source` path.

## Regression acceptance and comparison capture

### Logic acceptance

- create static and dynamic codes; edit immutable mode; copy both ways only when allowed
- ten content types round-trip, including empty optional values, Unicode, punctuation, calendar local time, and invalid geometry drafts
- rule reorder/add/remove preserves conditions, fallback identity, field errors, and focus; keyboard reorder works
- no stale search, preview, load, mutation, or identity response can commit after its owner changes
- save errors map to the correct field; dirty state and route leave are accurate; retry preserves user work
- guest creation, Google exchange failure, guest conversion, logout, session expiration, and config failure render distinct recoverable states
- billing return is provisional until server-confirmed; provider redirect and public URLs retain current boundaries
- Int64 scan count and Temporal values use explicit codecs; styling/color/emoji shape data round-trips unchanged
- static saved image matches encoded content; dynamic saved image encodes the saved short URL; draft/sample versus saved asset remains explicit

### Required visual frames

- identical seeded content, viewport, theme, browser scale, and scroll position before/after
- desktop about 1440×1000 and narrow phone 390×844; add 360px and 200% text/zoom checks for pressure points
- builder Content single default, multiple rules with fallback pointer, validation errors, loading/error/saved/dirty states
- Design Colors, Shape/Eyes, Center; QR versus non-QR; long content and open picker/popover
- dashboard empty/populated/search/mutation/delete dialog; anonymous/guest/registered header; billing unavailable/confirmed/pending return
- landing header/hero, pricing comparison overflow, and one long article in light and dark
- use fixed hero seed or reduced-motion for still comparisons, without claiming that proves animated performance
- verify keyboard focus, error announcements, no document horizontal overflow, and actual computed contrast alongside screenshots

Only `tests/domain/codes/content/operations.test.ts` exists in the inspected frontend; its four cases cover factory/catalog behavior. Backend tests do not establish the frontend interaction acceptance above. Port verification needs focused unit tests for pure transitions plus browser tests for real user journeys; duplicating SDK component tests is unnecessary.

## Entry links

- [Builder](../codebase/forever-pin.frontend-services/apps/web/src/presentation/codes/common/createCode/screens/CreateCodeScreen.tsx)
- [Request mappings](../codebase/forever-pin.frontend-services/apps/web/src/application/codes/createCodeForm.ts)
- [Rule editor](../codebase/forever-pin.frontend-services/apps/web/src/presentation/codes/routing/components/RuleControls.tsx)
- [Preview](../codebase/forever-pin.frontend-services/apps/web/src/presentation/codes/common/createCode/components/QrPreview.tsx)
- [Theme](../codebase/forever-pin.frontend-services/apps/web/src/bootstrap/index.css)
- [Historical design spec](../research/design-research/design-research.md)
- [Dashboard](../codebase/forever-pin.frontend-services/apps/web/src/presentation/codes/common/listCodes/screens/CodesListScreen.tsx)
- [Billing](../codebase/forever-pin.frontend-services/apps/web/src/presentation/billing/screens/BillingScreen.tsx)

## Captured comparison — 2026-09-26

The workspace relocation is committed as `132e14c`. The application is still React; Vue implementation is pending.
The uncommitted two-file trial changes `AppLayout.tsx` and `index.css` only.
It preserves builder controls and behavior while improving contrast, shared gutters, header wrapping, and elevation.

- Root typecheck and production build pass.
- At a 390px viewport, document width falls from 604px to the 375px client width.
- At a 360px viewport, the candidate fits the 345px client width.
- Browser-computed product heading is Geist 24px/600; light helper text is rgb(84,89,111).
- These checks establish this visual candidate, not full frontend accessibility or Vue migration acceptance.
- Before/after captures and evidence: [comparison](../../../../../system/sessions/frontend-conventions-sweep/foreverpin-visual-20260926/comparison.md).

The owner's visual choice remains open. Existing `v0.11` feature tasks remain in that track;
this analysis does not silently pull their implementation back into `v0.10`.
