# ForeverPin rounded identity exploration

*Last updated: 2026-09-29*

## Status

- [x] Inspect the retained QR logo, current Vue implementation and product palette.
- [x] Generate three distinct symbol concepts using the current logo as a reference.
- [x] Preserve original generation files, full prompts and the licensed Geist font.
- [x] Compose and inspect horizontal, carrier, light/dark and small-size reviews.
- [x] Select one direction with the owner: `01 — Routed QR`.
- [x] Owner approved the original four-part artwork with convention-compliant existing gaps.
- [x] Export the selected primary, standalone and app-tile family; verify target sizes and masks.

This folder preserves the original review concepts. `01 — Routed QR` is approved and supplies the
[canonical family](../../routed-qr/manifest.json); the [brand guide](../../brand.md) owns active usage.
The comparison's "Current QR" column is the archived pre-adoption baseline, preserved for reproducibility.

The owner selected `01 — Routed QR` for iteration on 2026-09-29, then clarified that its existing
gaps must remain unchanged. The owner approved the design without another motif and requested convention compliance.
The [gap study](refinement-01/refinement.md) is retained as unused exploration. The other two concepts
remain exploration history; the direction-selection question is closed.

## Current design judgment

Keep the original four-part composition and its current gaps. The three finder eyes already establish
QR recognition, and the open routing corner adds the programmable-destination idea. No extra pin,
infinity loop, arrowhead or dot is recommended: another motif would compete with the existing reading.
The useful remaining polish is consistent contours and faithful export construction, with the current
spacing and metaphor preserved. The owner has approved this artwork for use.

## Recommendation

**01 — Routed QR** is the strongest continuation of the existing identity. Three finder eyes preserve
the recognizable QR structure, while one routing bend replaces the small modules. That bend suggests
ForeverPin's product promise: a printed reference can persist while its destination changes.

| Direction | What it communicates | Tradeoff / refinement |
|---|---|---|
| Current QR | Familiar scanning category | Generic library glyph; scattered modules compete at small sizes |
| 01 — Routed QR | QR recognition with a changing destination | Regularize matching eyes and resolve micro-size gap merging |
| 02 — Finder F | A name-led mark with a QR-like counter | The closed upper counter makes the proposed F read as P/F |
| 03 — Linked Finders | A connection that lasts | Less immediate QR recognition; the crossing needs wider clearance |

The options hold violet and Geist constant so the review centers on symbol shape. They are visual
proposals, not claims of trademark uniqueness or final small-size conformance.

## Review files

- [Comparison](comparison.png): current identity and three concepts in consistent product placements.
- [Size review](size-review.png): actual small-size samples and magnified raster inspection.
- [Manifest](manifest.json): source dimensions, hashes, transparency and generated review artifacts.
- [Original prompts](prompts.json): exact generation requests and original output identifiers.
- [`source/01-routed-qr.png`](source/01-routed-qr.png)
- [`source/02-finder-f.png`](source/02-finder-f.png)
- [`source/03-linked-finders.png`](source/03-linked-finders.png)

## Construction and product fit

ForeverPin is a consumer venture. This exploration adopts the shared logo system's construction,
composition and export discipline at the owner's request; it does not add ForeverPin to the WoW2
portfolio identity. No `by wow2` endorsement is proposed.

The default identity pairs one symbol with the existing live wordmark `ForeverPin`. A standalone
symbol serves compact controls, and an app tile carries that same symbol on square surfaces.
An extra stacked composition is unnecessary without a real placement requiring it. Version and
environment labels remain ordinary UI text. A micro derivative is warranted only if the selected
standard symbol fails at the intended size.

The desired construction uses deliberate rounded corners, substantial forms and open negative space.
For the intentional separators in Routed QR or Linked Finders, the baseline is about `1/24` of the
greater visible symbol dimension, optically adjusted after actual-size review. This value does not
govern wordmark spacing, counter widths, clear space or export padding.

Routed QR's current straight gutters measure `33–34` source pixels at alpha `128`, against an
`868px` maximum visible extent (bounds at alpha above `16`). This is `3.8–3.9%`, within the
convention's starting tolerance around `4.17%`; the small-size failures below still require review.
Measurements sample the upper vertical gutter at source rows `350`, `400`, `450` and the left
horizontal gutter at source columns `350`, `400`, `450`, away from rounded junctions.

The generated sources are unmodified raster concepts. Their fills approximate the requested
`#7040d7` and include tonal variation and edge artifacts. They are not color-exact vector masters.
The review compositor preserves that evidence: it crops to visible bounds, scales proportionally,
and places the original pixels without redrawing or recoloring the symbols. A light lavender carrier in the
comparison is a placement test, not an approved replacement for the current violet app tile.

## Historical small-size findings

These findings describe the original concept review. The approved family's measured `24px` standalone minimum
and fallback favicon limitations are recorded in the [brand guide](../../brand.md).

The review renders maximum visible extents of `16`, `20`, `24` and `32` pixels. The existing baseline
includes its carrier; the proposed symbols have no baked-in carrier. The figures show native pixels
beside explicitly magnified raster detail.

- Routed QR preserves three open finder counters visually. Its four pieces merge at `16` and `20`
  pixels under a four-neighbor, alpha-at-least-128 connectivity check; they separate at `24` and `32`.
  The final export preserves those gaps and documents the minimum instead of introducing a micro derivative.
- Finder F keeps a strong silhouette but reads as P/F across the reviewed sizes. Its narrow tall
  proportions also give it less area in a square slot than the other two concepts.
- Linked Finders keeps two visible openings, but its crossover distinction weakens at small sizes.
  Its linked-square metaphor is clearer than its QR connection.
- The existing tile remains recognizable as the current product identity, while its smallest modules
  blur together in the smallest samples.

These observations support direction selection; they do not establish approved production minimums.

## Palette and typography

| Role | Current value |
|---|---|
| Light primary / foreground | `#7040d7` / `#ffffff` |
| Dark primary / foreground | `#aa8cff` / `#1d1433` |
| Light canvas / text | `#f4f5f8` / `#202331` |
| Dark canvas / text | `#14151a` / `#f1f3f8` |
| Wordmark | Geist Variable, weight `700`, spelling `ForeverPin` |

Values were checked against the current frontend's `src/bootstrap/index.css` and
`src/presentation/marketing/BrandGlyph.vue`. The review renders the installed Geist font directly;
the AI-generated images contain no lettering. The copied font and its OFL license live in `fonts/`.
The archived reference retains its Lucide license in `../../archive/2026-09-28-qr/lucide-license.txt`.

## Reproduce the review

Runtime: Python 3 with Pillow `12.3.0` and FreeType support for WOFF2 variable fonts.
From the repository root:

```sh
python3 product/brand/exploration/rounded-identity/render-review.py
```

The script reads only the saved sources, archived baseline and licensed font. It does not use the
original ImageGen output directory, a product runtime or a network dependency. Its output directory
option permits an independent replay without modifying the saved review files.

```sh
python3 product/brand/exploration/rounded-identity/render-review.py --output-dir /tmp/foreverpin-logo-review
```

Both review PNGs and the manifest were reproduced byte-for-byte in a separate output directory.

## Selection boundary

Routed QR is approved, with its original gaps preserved within the convention's optical tolerance.
One approved master owns all exports. Final checks cover transparency, exact palette variants, clear space,
primary minimum size, symbols at `16`, `20`, `24` and `32` CSS pixels, and app-tile masks.

Active exports and application integration are recorded in the owning brand guide. This historical review
does not provide alternate production artwork; its unselected concepts and gap studies remain unused.
