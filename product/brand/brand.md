# ForeverPin brand assets

*Last updated: 2026-09-28*

## Logo

- The owner retained the existing QR logo on 2026-09-28.
- `Logo` retains its original Lucide `QrCode` glyph, theme-aware violet tile, and live Geist wordmark.
- Active export source: `current/foreverpin-logo.svg`; it copies that existing mark's geometry.
- Active PNG, favicon, and OAuth exports live in `current/` and mirror into the frontend.
- The generated pin/infinity proposal is retained below as unused provenance.

---

## Active files

| Asset | Dimensions | Purpose |
|---|---|---|
| `current/foreverpin-logo.svg` | Scalable | Existing QR mark export source |
| `current/foreverpin-logo.png` | 1024 × 1024 | Product logo export |
| `current/foreverpin-google-logo.png` | 120 × 120 | Google consent logo export |
| `current/foreverpin-navbar-logo.png` | 56 × 56 | Smaller logo export |
| `current/foreverpin-navbar-logo@2x.png` | 112 × 112 | Higher density logo export |
| `current/favicon-16.png`, `current/favicon-32.png`, `current/favicon-48.png` | 16 / 32 / 48 | Browser favicons |
| `current/favicon.ico` | 16 / 32 / 48 / 64 / 128 / 256 | Browser fallback |
| `current/apple-touch-icon.png` | 180 × 180 | Device shortcut |

- Frontend `Logo` remains the original theme-aware React/Lucide component.
- Frontend raster copies are available under `src/assets/brand/`; favicons are under `public/`.
- The light palette (`#7040d7` tile, white glyph) supplies the static icon colors.
- `current/lucide-license.txt` preserves the source glyph's license; a frontend public copy accompanies exports.

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

- Frontend full logo and navbar copies live in `engineering/codebase/forever-pin.frontend-services/apps/web/src/assets/brand/`.
- Frontend stable favicon copies live in `engineering/codebase/forever-pin.frontend-services/apps/web/public/`.
- Frontend copies use the retained QR mark; the generated proposal files above remain unused.
- QR glyph source: `lucide-react` version `0.460.0`, ISC license.

---

## Generated proposal verification (2026-09-27)

- `pnpm typecheck`: passed using the repository's Node `24.11.0` runtime.
- `pnpm build`: passed; the existing bundle-size warning remains.
- PNG dimensions and alpha transparency were checked across every canonical export.
- Proposal exports matched the frontend copies when initially generated; active copies now use the QR mark.
- Production HTML resolves `%BASE_URL%` to the configured base.
- The `16px`, `28px`, `32px`, and `256px` mark was inspected on light and dark product surfaces.
- Browser interaction and live Google sign-in remain outside these asset checks.

---

## Generation provenance

- Mode: built-in `image_gen`, `transparent_background=true`.
- Initial output: `exec-c704ecc3-6603-4860-ba65-ab2c33d2b966.png`.
- Final output: `exec-22d6b968-8def-4c2f-a2a7-2ebbd52bcff3.png`.
- Tool output directory: `/Users/max/.codex/generated_images/01a0e42f-30d5-7943-b667-1a6d30016cc8/`.
- The canonical source is copied into this product folder; frontend usage has no dependency on the tool directory.
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
