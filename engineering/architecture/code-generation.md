# Code generation

*Last updated: 2026-09-13*

## Flow

- `CodePayloadMapper` selects the baked content or dynamic short link from the code's mode.
- Product image services pass payload, symbology, style, and format to the SDK renderer.
- Preview and downloaded images use server rendering.
- SVG and PNG exports are available for the supported QR/barcode flows.
- Shape, gradient, transparency, and emoji controls round-trip through the style contract.

The engine was extracted in v0.6; there is no local `ForeverPin.Codes` project.

---

## Remaining work

- Print sizing guidance and the output-fidelity contract remain in v0.9.
- File upload, logo controls, frames, PDF, and animated output remain in the backlog.
- Do not infer builder support from a capability present only in the SDK renderer.

Contracts: [content model](content-model.md). Tasks: [v0.9](../planning/version-track/v0.9/v0.9.md).
