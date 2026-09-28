import {
  CellsGlyph,
  DotsGlyph,
  FrameGlyph,
  HorizontalBarsGlyph,
  VerticalBarsGlyph,
} from '@wow-two-beta/ui-vue/presentation/display';
import type { Component } from 'vue';

import { FinderShape, ModuleShape } from '@/domain/codes';

/** Defines a shape option's label and the glyph that previews it. */
export interface ShapeDisplay {
  /** The display label. */
  readonly label: string;
  /** The preview glyph component. */
  readonly glyph: Component;
  /** The glyph's props. */
  readonly glyphProps: Readonly<Record<string, unknown>>;
}

/** Maps each module shape to its label and preview. */
export const ModuleShapeDisplays: Readonly<Record<ModuleShape, ShapeDisplay>> = {
  [ModuleShape.Square]: { label: 'Square', glyph: CellsGlyph, glyphProps: { cornerRx: 0 } },
  [ModuleShape.Rounded]: { label: 'Rounded', glyph: CellsGlyph, glyphProps: { cornerRx: 1.6 } },
  [ModuleShape.Dots]: { label: 'Dots', glyph: DotsGlyph, glyphProps: {} },
  [ModuleShape.Classy]: { label: 'Classy', glyph: CellsGlyph, glyphProps: { cornerRx: 0.8 } },
  [ModuleShape.ClassyRounded]: { label: 'Classy rounded', glyph: CellsGlyph, glyphProps: { cornerRx: 2.2 } },
  [ModuleShape.VerticalBars]: { label: 'Vertical bars', glyph: VerticalBarsGlyph, glyphProps: {} },
  [ModuleShape.HorizontalBars]: { label: 'Horizontal bars', glyph: HorizontalBarsGlyph, glyphProps: {} },
};

/** Maps each finder shape to its label and frame preview; the pupil preview adds `isDot`. */
export const FinderShapeDisplays: Readonly<Record<FinderShape, ShapeDisplay>> = {
  [FinderShape.Square]: { label: 'Square', glyph: FrameGlyph, glyphProps: { frameRx: 0, pupilRoundness: 0 } },
  [FinderShape.Rounded]: { label: 'Rounded', glyph: FrameGlyph, glyphProps: { frameRx: 4, pupilRoundness: 0.2 } },
  [FinderShape.Circle]: { label: 'Circle', glyph: FrameGlyph, glyphProps: { frameRx: 11, pupilRoundness: 0.5 } },
};
