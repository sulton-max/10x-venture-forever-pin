/** Represents one color stop of a code's foreground gradient. */
export interface CodeGradientStopDto {
  /** The stop color, `#RRGGBB`. */
  color: string;

  /** The stop position along the gradient, `0..1`. */
  offset: number;
}

/** Represents a code's foreground gradient — a linear or radial projection over its stops. */
export type CodeGradientDto =
  | {
      /** The linear projection discriminator. */
      type: 'linear';
      /** The direction in degrees; `0` is left to right. */
      angle: number;
      /** The ordered color stops. */
      stops: ReadonlyArray<CodeGradientStopDto>;
    }
  | {
      /** The radial projection discriminator. */
      type: 'radial';
      /** The extent as a fraction of the canvas half-size. */
      radius: number;
      /** The ordered color stops. */
      stops: ReadonlyArray<CodeGradientStopDto>;
    };
