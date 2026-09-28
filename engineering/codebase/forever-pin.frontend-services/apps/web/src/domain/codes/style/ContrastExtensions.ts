import type { CodeStyleDto } from './models/CodeStyleDto';

/** Holds the sRGB channel value at or below which the WCAG luminance curve stays linear. */
const LinearChannelThreshold = 0.03928;

/** Holds the contrast ratio below which a code likely will not scan at all. */
const ContrastLow = 3;

/** Holds the contrast ratio below which a scan should be tested before printing. */
const ContrastWarn = 4.5;

/** Defines how readable a style's colors are for a scanner; advisory only, never a proof of scanning. */
export const ContrastVerdict = {
  /** Refers to a transparent background whose surface decides the contrast. */
  Transparent: 'transparent',
  /** Refers to a foreground lighter than the background, which many scanners reject. */
  Inverted: 'inverted',
  /** Refers to contrast so low the code likely will not scan. */
  Low: 'low',
  /** Refers to contrast worth testing before printing. */
  Marginal: 'marginal',
  /** Refers to comfortable contrast. */
  Good: 'good',
} as const;

export type ContrastVerdict = (typeof ContrastVerdict)[keyof typeof ContrastVerdict];

/** Extends code styles with WCAG-based contrast checks between the foreground and the background. */
export const ContrastExtensions = {
  /** Computes the relative luminance of a `#RRGGBB` color, from `0` (black) to `1` (white). */
  luminance(hex: string): number {
    const digits = hex.replace('#', '');
    if (digits.length !== 6) return 0;
    const channel = (start: number) => {
      const value = parseInt(digits.slice(start, start + 2), 16) / 255;
      return value <= LinearChannelThreshold ? value / 12.92 : Math.pow((value + 0.055) / 1.055, 2.4);
    };
    return 0.2126 * channel(0) + 0.7152 * channel(2) + 0.0722 * channel(4);
  },

  /** Computes the contrast ratio of two `#RRGGBB` colors, from `1` (identical) to `21`. */
  ratio(left: string, right: string): number {
    const a = ContrastExtensions.luminance(left);
    const b = ContrastExtensions.luminance(right);
    return (Math.max(a, b) + 0.05) / (Math.min(a, b) + 0.05);
  },

  /** Assesses a style against its worst foreground color — every gradient stop, or the solid color. */
  assess(style: CodeStyleDto): { readonly verdict: ContrastVerdict; readonly ratio: number } {
    if (style.transparentBackground) return { verdict: ContrastVerdict.Transparent, ratio: 0 };
    const foregrounds = style.gradient ? style.gradient.stops.map((stop) => stop.color) : [style.foregroundColor];
    const ratio = Math.min(...foregrounds.map((color) => ContrastExtensions.ratio(color, style.backgroundColor)));
    const background = ContrastExtensions.luminance(style.backgroundColor);
    const verdict = foregrounds.some((color) => ContrastExtensions.luminance(color) > background)
      ? ContrastVerdict.Inverted
      : ratio < ContrastLow
        ? ContrastVerdict.Low
        : ratio < ContrastWarn
          ? ContrastVerdict.Marginal
          : ContrastVerdict.Good;
    return { verdict, ratio };
  },
} as const;
