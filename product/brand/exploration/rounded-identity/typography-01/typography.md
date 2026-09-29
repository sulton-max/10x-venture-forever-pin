# ForeverPin lettering study

*Last updated: 2026-09-29*

## Status

- [x] Compare approved WoW2, Wheelhouse and ForeverPin lettering at one review scale.
- [x] Render four ForeverPin treatments on light and dark surfaces with its approved symbol unchanged.
- [x] Select `02 Rounded`: lowercase Nunito Sans `900`, tracking `-0.035em`.
- [x] Rebuild primary, standalone and tile exports and the live app header from the selection.

## Finding

The WoW2 [logo system](../../../../../../../../conventions/design/identity/logo-system.md) requires readable weight
and optical balance but prescribes no common font; ForeverPin adopts that guidance voluntarily as an
independent consumer venture. WoW2 and Wheelhouse use approved raster lettering, not
font specifications. Their names read as compact, heavy, rounded lowercase wordmarks. ForeverPin's active
wordmark is title-case Geist Variable 700 with `-0.025em` tracking; the narrower, lighter lettering makes
its symbol-to-name composition feel different.

This is a visual consistency concern, not a separator problem. All four samples retain the approved
Routed QR silhouette, the original internal gaps, colors, symbol height, and symbol-to-name spacing.

## [Comparison](preview.png)

| Row | Lettering | Assessment |
|---|---|---|
| 00 Previous | `ForeverPin`, Geist 700 | Matched the prior UI, but looked lighter and more formal beside the two approved wordmarks. |
| 01 Dense | `foreverpin`, Geist 900 | Better density and casing; straight terminals still look less rounded. |
| 02 Selected | `foreverpin`, Nunito Sans 900 | Rounded and compact without overwhelming the product symbol. |
| 03 Soft | `foreverpin`, Fredoka 700 | Clear rounded family resemblance, but playful letter shapes weaken the product's utility tone. |

The owner selected `02 Rounded`. Product/UI text continues spelling `ForeverPin` in Geist; the compact
lowercase wordmark is a separate approved brand asset. The [brand guide](../../../brand.md) owns the
resulting family and application integration.

## Sources and replay

- Current [Geist source](../../../routed-qr/source/geist-latin-wght-normal.woff2) and approved
  [Routed QR alpha](../../../routed-qr/source/clean-symbol-alpha.png) are unchanged.
- The WoW2 and Wheelhouse images in `source/` are exact copies of their approved primary artwork.
- [Nunito Sans](https://github.com/google/fonts/tree/main/ofl/nunitosans) and
  [Fredoka](https://github.com/google/fonts/tree/main/ofl/fredoka) were obtained from the official
  Google Fonts repository with their SIL Open Font License files in `source/`.
- The font files and reference images are local inputs; the script needs no network or host-installed font.
- Runtime: Python 3, Pillow with variable TrueType and WOFF2 support.

```sh
python3 product/brand/exploration/rounded-identity/typography-01/render-review.py
```

The renderer updates only this study's `preview.png`. It does not write active assets or application files.
