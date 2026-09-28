import { BarcodeFormat } from '../content';

/** Holds the smallest module a phone camera resolves reliably, per symbology family, in millimetres. */
const MinimumModuleMm: Readonly<Record<BarcodeFormat, number>> = {
  // Linear codes follow the GS1 nominal bar width; every 2D symbology gets a camera-friendly margin above it.
  [BarcodeFormat.Code128]: 0.33,
  [BarcodeFormat.Ean13]: 0.33,
  [BarcodeFormat.UpcA]: 0.33,
  [BarcodeFormat.QrCode]: 0.4,
  [BarcodeFormat.DataMatrix]: 0.4,
  [BarcodeFormat.Pdf417]: 0.4,
  [BarcodeFormat.Aztec]: 0.4,
};

/** Matches the root element of an SVG document. */
const SvgRoot = /<svg\b[^>]*>/;

/** Matches a `viewBox` of four numbers. */
const ViewBox = /\bviewBox="\s*[-\d.]+[\s,]+[-\d.]+[\s,]+([\d.]+)[\s,]+([\d.]+)\s*"/;

/** Rounds a length to at most two decimals for an SVG attribute. */
function millimetres(value: number): string {
  return `${Math.round(value * 100) / 100}mm`;
}

/**
 * Holds the print rules of a scannable code. A rendered SVG measures in modules, quiet zone included, so its
 * `viewBox` width tells how small one module becomes at a printed width.
 */
export const PrintSizing = {
  /** The smallest printed width a phone camera focuses on comfortably, in millimetres. */
  MinimumWidthMm: 20,

  /** The ratio of scanning distance to printed width that scans comfortably. */
  DistanceRatio: 10,

  /** Returns the smallest module the symbology prints reliably at, in millimetres. */
  minimumModuleMm(format: BarcodeFormat): number {
    return MinimumModuleMm[format];
  },

  /** Reads the symbol's width and height in modules from an SVG's `viewBox`; undefined without one. */
  modules(svg: string): { readonly across: number; readonly down: number } | undefined {
    const root = SvgRoot.exec(svg)?.[0];
    const box = root ? ViewBox.exec(root) : null;
    const across = Number(box?.[1]);
    const down = Number(box?.[2]);
    return across > 0 && down > 0 ? { across, down } : undefined;
  },

  /** Returns the printed size of one module. */
  moduleMm(widthMm: number, modulesAcross: number): number {
    return widthMm / modulesAcross;
  },

  /** Returns the smallest printed width that keeps every module resolvable. */
  minimumWidthMm(format: BarcodeFormat, modulesAcross: number): number {
    return Math.max(PrintSizing.MinimumWidthMm, Math.ceil(modulesAcross * MinimumModuleMm[format]));
  },

  /** Returns how far away a code of this width still scans comfortably, in millimetres. */
  scanDistanceMm(widthMm: number): number {
    return widthMm * PrintSizing.DistanceRatio;
  },

  /** Sets the SVG's physical size — the width given, the height in proportion; undefined without a `viewBox`. */
  withPhysicalWidth(svg: string, widthMm: number): string | undefined {
    const root = SvgRoot.exec(svg);
    const modules = PrintSizing.modules(svg);
    if (!root || !modules) return undefined;
    const sized = root[0]
      .replace(/\s(?:width|height)="[^"]*"/g, '')
      .replace(
        /^<svg\b/,
        `<svg width="${millimetres(widthMm)}" height="${millimetres((widthMm * modules.down) / modules.across)}"`,
      );
    return svg.slice(0, root.index) + sized + svg.slice(root.index + root[0].length);
  },
} as const;
