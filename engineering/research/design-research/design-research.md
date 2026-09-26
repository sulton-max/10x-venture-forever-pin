# ForeverPin — design system

*Last updated: 2026-09-26*

> Current palette and accepted product frame. Earlier layout explorations remain reference material below.

## Current direction

- The owner accepted the typography, wrapping navigation, aligned gutters, and restrained elevation on September 26.
- The owner confirmed the after palette on September 26; commit `6264a34` is the accepted visual baseline.
- Vue adoption preserves this palette and the accepted frame, with matching screenshots for verification.
- Light mode uses a cool neutral canvas, white navigation/cards, and violet primary actions.
- Dark mode uses graphite canvas/cards, raised controls, and lighter violet actions with dark text.
- Structural dividers stay quiet; input outlines and keyboard focus remain distinct.
- Geist stays the UI font; Geist Mono remains available for literal values.
- QR artwork follows the saved code design; application colors do not recolor the rendered asset.

## Semantic palette

Values map directly to `--color-{name}` in
[the app stylesheet](../../codebase/forever-pin.frontend-services/apps/web/src/bootstrap/index.css).

| Token | Light | Dark |
|---|---|---|
| `background` | `#f4f5f8` | `#14151a` |
| `card` | `#ffffff` | `#1d2028` |
| `card-foreground` | `#202331` | `#f1f3f8` |
| `popover` | `#ffffff` | `#232733` |
| `popover-foreground` | `#202331` | `#f1f3f8` |
| `muted` | `#e9ebf1` | `#2a2e3a` |
| `foreground` | `#202331` | `#f1f3f8` |
| `muted-foreground` | `#586174` | `#aab2c1` |
| `subtle-foreground` | `#626b7c` | `#9aa4b6` |
| `inverse` | `#202331` | `#f1f3f8` |
| `inverse-foreground` | `#f4f5f8` | `#202331` |
| `border` | `#dce0e8` | `#363c49` |
| `border-strong` | `#737e93` | `#8d98ad` |
| `input` | `#7b8597` | `#727b8e` |
| `ring` | `#7040d7` | `#aa8cff` |
| `primary` | `#7040d7` | `#aa8cff` |
| `primary-foreground` | `#ffffff` | `#1d1433` |
| `primary-soft` | `#eee8fc` | `#302744` |
| `primary-soft-foreground` | `#5730af` | `#d0bcff` |
| `accent` | `#0f766e` | `#2dd4bf` |
| `accent-foreground` | `#ffffff` | `#042f2e` |
| `accent-soft` | `#ccfbf1` | `#103a35` |
| `accent-soft-foreground` | `#115e59` | `#5eead4` |
| `destructive` | `#b4233a` | `#fb8795` |
| `destructive-foreground` | `#ffffff` | `#380d18` |
| `destructive-soft` | `#fff0f2` | `#3b222b` |
| `destructive-soft-foreground` | `#a21d33` | `#ffb0ba` |
| `success` | `#167153` | `#72d3ab` |
| `success-foreground` | `#ffffff` | `#0e2d22` |
| `success-soft` | `#e7f5ed` | `#1e352d` |
| `success-soft-foreground` | `#165c45` | `#96dfbd` |
| `warning` | `#925500` | `#efc16c` |
| `warning-foreground` | `#ffffff` | `#32220c` |
| `warning-soft` | `#fff3d6` | `#3b3120` |
| `warning-soft-foreground` | `#7a4900` | `#f4d195` |
| `info` | `#2666a1` | `#8dbff2` |
| `info-foreground` | `#ffffff` | `#122b42` |
| `info-soft` | `#eaf2fc` | `#223246` |
| `info-soft-foreground` | `#225782` | `#b0d3f6` |

## Component mapping

- Navigation uses `bg-card`; public navigation uses `bg-card/95` with its existing blur.
- Cards use `bg-card`; popovers and text inputs use `bg-popover`.
- `border` separates structure; `input` identifies controls; `border-strong` emphasizes hover/open controls.
- `ring` identifies keyboard focus. Severity roles have explicit solid and soft foreground pairs.
- Segmented controls use the SDK's selected `background` pill over a `muted` track.
- Inactive segmented labels use the supported soft variant for readable text, preserving selected behavior.
- The installed React SDK `0.0.97` applies `read-only:bg-muted` to Select buttons too;
  input-border contrast therefore covers both popover and muted surfaces.
- Google sign-in remains provider-rendered; no iframe style overrides are applied.
- Token semantics must be rechecked against Vue `0.0.7` during the pending framework migration.

## Accepted typography and frame

- Product heading: Geist, 24px, weight600, line-height1.25, letter-spacing-0.02em.
- Header and main share responsive gutters; account navigation wraps on narrow screens.
- Existing builder controls, card geometry, field spacing, and routing behavior are preserved.
- `.surface-soft` retains the accepted neutral elevation; color changes do not introduce colored shadows.

## Evidence and scope

- [Palette comparison](../../../../../system/sessions/frontend-conventions-sweep/foreverpin-palette-20260926/comparison.md)
  contains matched light/dark desktop and light phone captures, plus design/focus/menu checks.
- [Accepted frame comparison](../../../../../system/sessions/frontend-conventions-sweep/foreverpin-visual-20260926/comparison.md)
  preserves the earlier typography, elevation, and overflow baseline.
- [Frontend audit](../../architecture/frontend-adoption.md) owns remaining behavior and Vue migration work.
- These palette checks do not establish complete accessibility or completion of `v0.10`.

---

## Historical exploration — June 2026

The following records earlier layout ideas and SDK assumptions. It is not current implementation guidance.
The current palette, accepted frame, frontend audit, and active version track above take precedence.

## Builder layout (v0.6 — grouped tabs + accordion)

- builder = **3 grouped tabs** `Content · Design · Routing` (SDK `SegmentedControl`) with the **live preview sticky** right. Revised from v0.5's flat tabs (`Destination · Style · …`): as styling + content sections grow (colors/fill · shape & eyes · center · CTA · content-type forms), flat tabs sprawl and each panel scrolls long.
- **Design tab = SDK `Accordion` (`type="single"`)** of sub-sections — `Colors & fill · Shape & eyes · Center · CTA` — one open at a time → caps tab count AND height; scales to any number of styling sections.
- chosen (variant **D**) over flat tabs (A — sprawls) / pure accordion (B — runner-up, simplest + most mobile-friendly) / section-rail (C — desktop-clean, weak on mobile): D keeps a stable 3-tab IA and bounds height via the accordion.
- **Content** — short link (read-only) · name · fallback URL · (v0.7) content-type picker + per-type form.
- **Design** (accordion) — Colors & fill (fg/bg + `Solid / Gradient / Transparent`) · Shape & eyes · Center (emoji; logo later) · (v0.7) CTA caption.
- **Routing** — the dense joined rule list (see *Routing rules layout*).
- one violet **Save** in the card footer; sub-section labels = `text-subtle` 11/500.
- mobile — tabs persist (or collapse to a select); the Design accordion stays; preview becomes a top strip.

## Shape & eyes panel (design-exploration — Axis 1 locked)

- **Axis 1 — body-picker container = Variant D** (locked, user pick): a full-width **Body** grid of `32px` preset cells above a paired **Eyes** block (External + Internal side by side, `w-px` hairline between). Over A (colors-parity strip — crams 7 glyphs on the right), C (all grids — drops the Row language), B (body grid + eye rows — recommended runner-up).
- **Cell** — `ShapeCell` mirrors the colors preset button: `h-8 w-8` (32px) · `rounded-md border` · soft-primary active (`border-primary bg-primary/10 text-primary`) · glyph in `currentColor` (`ModuleSwatch` / `FinderSwatch`), glyph-only; shape name via `title` + `aria-label`.
- **Rhythm** — `Stack gap=0`; Body `py-2 first:pt-0`; Eyes `border-t pt-2` (no bottom pad → accordion `py-3` owns it). Section labels `text-sm text-muted-foreground`, eye sub-labels `text-xs`. 12/12 padding, parity with Colors & fill.
- **Open axes (next)** — 2 eye-pair layout · 3 glyph anatomy (legibility of `Square` / `Classy` / `Classy rounded` at 32px) · 4 dividers + labels · 5 states (non-QR symbology → disable eyes/shape · reduced-motion).

## Components

- **Input** 36–38px; surface bg (light) / sunken (dark); 1px field-border → focus = brand border + 2px brand ring (~30%).
- **Primary button** brand fill, on-brand text, radius 10, weight 500 — **one per screen** (Save). Downloads/secondary = ghost.
- **Ghost/secondary** surface bg + 0.5px border-strong.
- **Condition chip** brand-tint / brand-on-tint, radius 7.
- **State pill** radius 999 — Active = pop-tint / pop-on-tint (+ 6px dot); Off = off-bg / off-text.
- **Rule row** 0.5px border, radius 10, subtle grip handle, drag-to-reorder; "evaluated top-down".
- **Color field** 18px swatch (radius 5) + mono hex.
- **Short link** sunken + lock icon + ghost Copy; permanent/non-editable treatment.

## Routing rule editor

- **inline-expanding** — `+ Add rule` expands the editor in place (not a modal); keeps context, fast multi-rule entry.
- **rule = N conditions → 1 destination**; match toggle **All (AND, default) / Any (OR)** — segmented control, violet active.
- **condition = `field → operator → value`**; value control adapts: select · multiselect chips (`brand-tint`) · range · number.
- condition types — `country/region` (is · is one of) · `device/OS` (iOS·Android·Desktop) · `language` (locale) · `time window` (between·before·after) · `day of week` (weekday·weekend) · `scan count` (first N·after N).
- **editor-active treatment** — `1.5px #A78BFA` (brand-400) border, radius `12`; "New rule" label in `brand-on-tint`.
- **destination** — "Send to" + mono URL input (link icon) + helper line.
- footer — Cancel (ghost, muted) + Add rule (violet); per-condition remove `×`; `+ Add condition` = violet text-button.
- reorder via drag handle; **first match wins**, else fallback URL.
- backend contract — `{ match: 'all' | 'any', conditions: [{ field, op, value }], destination }`.

## Routing rules layout

- **joined dense list** (locked; V1 separated cards / V2 timeline rail considered) — all rules in one bordered container, shared `0.5px` dividers, no gaps. scales to many rules with little height.
- row columns — `[grip] [num] [condition chips +N] [→] [destination mono] [status] [chevron]`.
- **number = priority**; **drag grip** reorders (`Accordion` has no native reorder — add handles); first match wins, else fallback.
- **expand-in-place** — chevron opens the rule's editor inline within the container (inset `#18181B` body); `type="single"` open + collapsible keeps height bounded.
- **status pill** = neutral chip + dot, **click-to-toggle** On/Off without expanding.
- `+ Add rule` = the trailing row inside the container → appends + auto-expands + scrolls in.
- expanded editor reuses *Routing rule editor* (match All/Any · condition rows · Send to · Remove).
- unmatched → fallback shown as a muted trailing line under the container (fallback field lives in Details).
- reuses `Accordion` (`type="single"`) + `StepCard` number language from `@wow-two-beta/ui`.

## Usage rules

- Teal is **rationed** — active rule / success / most-scanned only. Never decorative.
- One violet CTA per screen. Red strictly destructive (delete rule).
- **Dark tone** — neutral charcoal surfaces (no violet cast); chips neutral; violet on the primary action only.
- **Status = chip + dot (both themes)** — neutral chip + teal dot (on) / `#52525B` dot (off); never a colored pill. Condition chips: violet-tint in light, neutral in dark.
- Mono for every short-link, hex, destination URL.
- QR preview tile = white in both themes.
- **Sentence case** everywhere.

## SDK mapping & gaps

- target lib — `@wow-two-beta/ui` (Tailwind v4 `@theme`, 24 semantic tokens + `.dark`, `surfaceVariants` on `Card`). map via the app's `src/index.css`.
- **token overrides** — `@theme` (light) + `.dark`: `background`→canvas, `card`→surface, `muted`→sunken, `foreground`/`border`/`input`/`ring`, `primary`→violet, + **new `accent`** (teal pop) `#0D9488` / soft `#CCFBF1` / soft-fg `#115E59` (dark `#2DD4BF` / `#103A35` / `#5EEAD4`). Tailwind v4 auto-emits `bg-accent` etc.
- dark surfaces ≈ SDK zinc defaults (`#09090b`/`#18181b`/`#27272a`); nudge to `#121214`/`#1C1C1F`/`#2A2A2E`.
- **use as-is** — `Button` · `UrlInput` · `TextInput` · `Select` · `ColorField`/`ColorPicker` · `SegmentedControl` · `MultiSelect`/`TagsInput` · `Tag`/`Badge` · `Status`/`NotificationDot` · `StepCard` · `Accordion` · `EmptyState` · `Breadcrumb` · `CopyButton` · `FormField`/`FormErrorMessage` · `Skeleton` · `Toast` · `Switch` · `Tooltip`.
- **extend SDK** — `accent` token family (+ raw scale) · `Card` `sheen`/`glow` gradient variant · brand→violet override · `Accordion` drag-reorder.
- **build** — `Sortable` list primitive (SDK, generic) · `RuleBuilder` field→op→value + match toggle (forever-pin first → extract per `dev-cycle`) · QR preview (product, `qrcode.react`).
- status = compose `Badge` (neutral) + `NotificationDot`/dot — never a colored pill.

## Iterate next (then map to SDK)

1. Empty state — "no rules yet → every scan goes to fallback".
2. Add-rule editor (inline row vs modal) — condition builder.
3. Validation / error states (bad URL, dup name) + loading/skeleton.
4. Mobile single-column (preview collapses to top).
5. Extend system → `CodesListScreen`, `CreateCodeScreen`, `BillingScreen`, `LoginScreen`.
6. Map tokens → `@wow-two-beta/ui` `@theme` overrides + apply.
