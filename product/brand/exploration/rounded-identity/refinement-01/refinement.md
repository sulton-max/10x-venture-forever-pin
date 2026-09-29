# Routed QR gap study — unused

*Last updated: 2026-09-29*

## Status

- The owner clarified that the original gaps must remain unchanged.
- This folder records an unused spacing exploration; none of its generated candidates is selected.
- The selected source remains [`../source/01-routed-qr.png`](../source/01-routed-qr.png).
- The [current design judgment](../exploration.md#current-design-judgment) concerns motif sufficiency, not spacing.

## Historical work

- [x] Owner selected `01 — Routed QR` for iteration.
- [x] Generate a regularized standard candidate and a wider-gap micro candidate.
- [x] Verify the first pass at `16`, `20`, `24` and `32` pixels.
- [x] Preserve the corrected standard candidate and reproducible review sheets as unused evidence.

The three finder eyes and lower-right routing bend remain the selected identity. This historical pass
explored construction and small-size spacing. It does not govern current artwork decisions or reopen
Finder F and Linked Finders. Current assets remain owned by the [brand guide](../../../brand.md).

## Result

The wider-gap **B — Micro spacing** candidate keeps four separate components at every tested size,
including `16px` and `20px`, where the original merges. Its three finder counters remain visually
open. The candidate is nevertheless unused because the owner explicitly retained the original gaps.

The initial A generation tightened the gutters and failed the intended improvement. It is retained
as source evidence, not recommended artwork. A targeted correction is saved as `source/01a-refined-v2.png`;
it is also unused. The comparison sheet shows that corrected A beside the original and B.

## Measurements

Straight gutters are measured at alpha at least `128`, perpendicular to their midbodies, excluding
corner transitions. The maximum visible extent comes from bounds at alpha above `16`. Connectivity
uses four-neighbor foreground pixels at alpha at least `128` after proportional LANCZOS resizing.

| Source | Visible maximum | Vertical gutter | Horizontal gutter | Components at 16 / 20 / 24 / 32px |
|---|---:|---:|---:|---|
| Original 01 | 868px | 34px / 3.92% | 33px / 3.80% | 1 / 1 / 4 / 4 |
| Initial A, superseded | 884px | 29px / 3.28% | 28px / 3.17% | 1 / 1 / 1 / 4 |
| Corrected A, unused | 902px | 37px / 4.10% | 36px / 3.99% | 2 / 2 / 4 / 4 |
| B — Micro spacing | 675px | 47px / 6.96% | 45px / 6.67% | 4 / 4 / 4 / 4 |

Four components are necessary for this segmented mark, but they do not alone establish recognition
or a production minimum. The size sheet also checks visible finder counters, routing bend and
overall balance. The standard gap baseline remains about `1/24` of maximum visible extent; B's
wider gutters were explored as a micro exception and are not selected for use.

## Review and sources

- [Comparison](comparison.png): original, standard candidate and micro candidate at equal maximum extents.
- [Size review](size-review.png): actual-size and magnified samples.
- [Manifest](manifest.json): original hashes, dimensions, transparency, transformations and replay evidence.
- [Prompts](prompts.json): exact built-in `image_gen` requests, preserved alongside generated sources.
- [Initial A](source/01a-refined.png): superseded first pass, retained without modification.
- [Corrected A](source/01a-refined-v2.png): unused correction, retained without modification.
- [B — Micro spacing](source/01b-micro.png): unmodified generated candidate.

The renderer crops and scales the saved pixels without redrawing or recoloring the symbols. Geist
weight `700` is rendered from the licensed font in `../fonts/`. The original and both refinement
sources have genuine alpha channels; their violet fills still contain slight tonal variation and
edge residue. They are raster review sources, not color-exact editable masters. Dark-surface samples
show the native source violet honestly; final palette variants remain an export step.

## Reproduce

Runtime: Python 3, Pillow `12.3.0`, and FreeType support for the copied Geist WOFF2 variable font.
From the repository root:

```sh
python3 product/brand/exploration/rounded-identity/refinement-01/render-review.py
python3 product/brand/exploration/rounded-identity/refinement-01/render-review.py --output-dir /tmp/foreverpin-refinement-review
```

The script reads saved sources only and verifies fresh temporary render hashes. It preserves the
previous exploration and current application assets. The shared composition plan remains one
horizontal identity, one standalone symbol and an app tile carrying that same symbol, with a
tested micro derivative where needed. Final artwork approval precedes production asset replacement.
