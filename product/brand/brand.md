# ForeverPin brand assets

*Last updated: 2026-10-20*

## Approved identity

- The owner approved `01 — Routed QR` on 2026-09-29: three rounded finder eyes plus one routing corner.
- Original gaps and the four-part silhouette are preserved; no extra motif is added.
- [Canonical family](routed-qr/manifest.json) owns the checksummed original, cleaned alpha and derived exports.
- [Review sheet](routed-qr/preview.png) shows horizontal, standalone and app-tile compositions.
- The independent consumer identity keeps violet and uses Nunito Sans lettering; no WoW2 endorsement is used.

## Icons

- `icons/` holds the eight product variants Wheelhouse and other consoles read, named `{form}-{color}-{theme}.png`.
- They copy the Routed QR exports: symbol = `short`, primary horizontal = `full`; no service has its own mark.

## Spacing and minimums

- The shared logo system's separator baseline is `1/24` of maximum visible extent, with `±10%` optical tolerance.
- Original straight gutters measure `33–34px` against `868px` visible extent: approximately `3.8–3.9%`.
- Those values already meet the convention; exporting preserves them instead of widening the gaps.
- Alpha cleanup preserves every original alpha-at-least-128 contour pixel; the manifest records this invariant.
- Standard standalone artwork requires at least `24px` visible extent. The shared navigation slot is `28px`.
- Primary horizontal exports are reviewed at `32px` minimum total image height.
- External clear space is one quarter of visible symbol height; it is not baked into every export.
- `16px` and `20px` reductions merge modules. Small favicons are fallbacks, not approved micro redesigns.
- [Size review](routed-qr/size-review.png) records actual pixels; [mask review](routed-qr/mask-review.png) records tile crops.
- Product-specific clear space, export padding and symbol-to-wordmark spacing are recorded in the export manifest.

## Color and lettering

| Role | Light | Dark |
|---|---|---|
| Symbol / accent | `#7040d7` | `#aa8cff` |
| Wordmark | `#202331` | `#f1f3f8` |
| App-tile glyph | `#ffffff` | `#1d1433` |
| Canvas | `#f4f5f8` | `#14151a` |

- Wordmark: Nunito Sans Variable `900`, lowercase `foreverpin`, tracking `-0.035em`.
- The approved [lettering study](exploration/rounded-identity/typography-01/typography.md) selects `02 Rounded`.
- The app uses the exact exported wordmark mask; ordinary interface text retains Geist Variable.
- The canonical source includes the exact variable font and its SIL Open Font License.
- Light/dark and monochrome versions derive from one alpha mask; internal counters remain transparent.
- App tiles intentionally add an opaque carrier; it is separate from the standalone symbol.

## Masters and reproduction

- Master type: raster. The unmodified approved ImageGen source is preserved in `routed-qr/source/`.
- Cleanup removes faint isolated residue and flattens fill; it does not reposition parts or redesign corners.
- `foreverpin-logo.svg` and `favicon.svg` are self-contained raster wrappers, not editable vector masters.
- Runtime: Python 3, Pillow `12.3.0`, NumPy `2.3.5`, and FreeType support for TrueType and WOFF2 variable fonts.

```sh
python3 engineering/scripts/export-brand-logos.py
python3 engineering/scripts/export-brand-logos.py --output-dir /tmp/foreverpin-brand-replay
python3 engineering/scripts/sync-brand-logos.py
python3 engineering/scripts/sync-brand-logos.py --check
```

- The exporter records source hashes, exact transforms, dimensions, spacing and small-size checks.
- The sync script verifies every active copy and writes [`current/manifest.json`](current/manifest.json).
- Previous Lucide QR assets remain preserved in [`archive/2026-09-28-qr/`](archive/2026-09-28-qr/).
- The [original exploration](exploration/rounded-identity/exploration.md) and [unused gap study](exploration/rounded-identity/refinement-01/refinement.md)
  remain provenance; they do not supply alternate active artwork.

---

## Active files

| Asset | Dimensions | Purpose |
|---|---|---|
| `routed-qr/exports/primary-light.png`, `primary-dark.png` | 982 × 192 | Approved horizontal logo |
| `routed-qr/exports/wordmark-ink.png`, `wordmark-white.png` | 773 × 164 | Lettering components |
| `current/foreverpin-logo.svg` | 1024 × 1024 wrapper | Raster compatibility wrapper |
| `current/foreverpin-logo.png` | 1024 × 1024 | Product logo export |
| `current/foreverpin-google-logo.png` | 120 × 120 | Google consent logo export |
| `current/foreverpin-navbar-logo.png` | 56 × 56 | Transparent standalone symbol |
| `current/foreverpin-navbar-logo@2x.png` | 112 × 112 | Symbol source for the 28px live mask |
| `current/favicon-16.png`, `current/favicon-32.png`, `current/favicon-48.png` | 16 / 32 / 48 | Browser favicons |
| `current/favicon.ico` | 16 / 32 / 48 / 64 / 128 / 256 | Browser fallback |
| `current/apple-touch-icon.png` | 180 × 180 | Device shortcut |

- `BrandGlyph.vue` renders the approved symbol as an alpha mask using the existing `primary` theme token.
- Its colocated symbol and wordmark masks serve marketing headers, footer and app shell through the shared component.
- Compatibility copies remain under `src/assets/brand/`; browser icons remain under `public/`.
- The SVG favicon is replaced together with PNG/ICO files, preserving the existing HTML references.
- The Google consent logo is prepared locally; uploading it to the provider is a separate action.
- Historical Lucide attribution remains with archived assets and previous public license copies.

---

## Adoption verification (2026-09-29)

- All 34 canonical family files, including the manifest, replay byte-for-byte in an independent directory.
- All 23 active artwork copies match their canonical exports; the sync check verifies source hashes before copying.
- Source contour and original gap checks pass. Circle and squircle tile masks clip zero symbol pixels.
- Frontend formatting, lint, type checks and production build pass using Node `24.11.0`.
- Unit tests: 35 passed across 10 files.
- Marketing and accessibility browser tests: 29 passed, one desktop-inapplicable mobile-menu test skipped.
- [Integration review](integration-review/results.json) verifies loaded masks, palette and home-link accessibility
  in light/dark themes at desktop and phone viewports; four adjacent screenshots preserve the visual evidence.
- The live wordmark mask uses `94 × 20px`; the icon uses `28 × 28px`. Both retain the intended theme colors.
- Runtime checks cover the marketing header and footer. The authenticated app shell imports the same component
  and passes compilation; authenticated navigation was not exercised in this brand review.
- Provider uploads and production deployment are outside this local asset and application verification.

---

## Generated proposal files (unused)

| Asset | Dimensions | Purpose |
|---|---|---|
| `source/foreverpin-generated-source.png` | 1254 × 1254 | Unmodified final generated source |
| `logos/foreverpin-logo.png` | 1024 × 1024 | Rejected generated proposal |
| `logos/foreverpin-google-logo.png` | 120 × 120 | Unused generated proposal |
| `logos/foreverpin-navbar-logo.png` | 56 × 56 | Navbar, rendered at 28 CSS pixels |
| `logos/foreverpin-navbar-logo@2x.png` | 112 × 112 | High density navbar |
| `favicon/favicon-16.png` | 16 × 16 | Small browser favicon |
| `favicon/favicon-32.png` | 32 × 32 | Browser favicon |
| `favicon/favicon-48.png` | 48 × 48 | Larger browser favicon |
| `favicon/favicon.ico` | 16 / 32 / 48 / 64 / 128 / 256 | Multi-resolution browser favicon |
| `favicon/apple-touch-icon.png` | 180 × 180 | Device shortcut icon |

- These files describe the rejected pin/infinity proposal and are never synchronized into the current app.
- The previous QR baseline used `lucide-react` version `0.460.0`, ISC license; its exports are archived.

---

## Historical rejected-proposal verification (2026-09-27)

- `pnpm typecheck`: passed using the repository's Node `24.11.0` runtime.
- `pnpm build`: passed; the existing bundle-size warning remains.
- PNG dimensions and alpha transparency were checked across every canonical export.
- Proposal exports matched frontend copies only at that historical point; active copies now use Routed QR.
- Production HTML resolves `%BASE_URL%` to the configured base.
- The `16px`, `28px`, `32px`, and `256px` mark was inspected on light and dark product surfaces.
- Browser interaction and live Google sign-in remain outside these asset checks.

---

## Historical rejected-proposal provenance

The following records concern the unused pin/infinity proposal, not the approved Routed QR family.
Approved generation provenance is recorded in the original exploration prompts and canonical manifest.

- Mode: built-in `image_gen`, `transparent_background=true`.
- Initial output: `exec-c704ecc3-6603-4860-ba65-ab2c33d2b966.png`.
- Final output: `exec-22d6b968-8def-4c2f-a2a7-2ebbd52bcff3.png`.
- Tool output directory: `/Users/max/.codex/generated_images/01a0e42f-30d5-7943-b667-1a6d30016cc8/`.
- The rejected source remains in this product folder for provenance; it has no active frontend usage.
- The generated fill approximates the requested violet; it is not a color-exact vector master.

### Initial prompt

```text
Use case: logo-brand
Asset type: ForeverPin product logo symbol, source for navbar logo and favicon
Primary request: Create an original polished minimal logo symbol for ForeverPin, a product for permanent programmable links and QR codes. A simple upright pin silhouette integrated with a horizontal infinity loop, communicating a destination that lasts forever. Make the form memorable and beautifully balanced, with generous negative space and thick clean geometry that remains unmistakable at 16px. One compact symbol only, no wordmark.
Scene/backdrop: actual fully transparent background, no canvas, no tile, no circle backdrop.
Style/medium: flat vector-like graphic, crisp edges, subtle optical balance, professional software brand identity, no textures.
Composition/framing: centered single symbol occupying about 85% of a square canvas; upright pin with rounded two-lobed top and one pointed bottom; infinity expressed clearly through negative space in its top. Keep a simple silhouette rather than combining several separate objects.
Color palette: uniform flat violet #7040d7, matching the existing product primary token.
Text: none.
Constraints: transparent outside and inside the negative-space cutouts, only one flat ink color, thick contours, no thin details, no tiny QR pattern, no strokes outside the symbol, no mockup, no shadows, no gradients, no glow, no 3D, no slogan, no letters, no watermark.
```

### Refinement prompt

```text
Use case: precise-object-edit
Asset type: final ForeverPin logo symbol
Change only the fill finish and tiny edge defects of this existing generated logo. Preserve the upright pin/infinity silhouette, its composition, shape, proportions, size, cutout holes and centered placement exactly. Make every solid interior pixel uniform opaque flat violet #7040d7, with only antialiasing at boundaries. Eliminate all grain, mottling, tonal variation, gradients, highlights and stray purple pixels in the transparent holes or outside the mark. Refine the center crossing into a clean crisp union without a dotted seam. Maintain real alpha transparency in every outside and cutout area. No new design elements, no letters, no background, no shadows, no glow, no stroke. Final production-quality flat graphic.
```
