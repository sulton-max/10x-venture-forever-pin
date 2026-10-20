import { ArrowDown, ArrowDownLeft, ArrowDownRight, ArrowRight } from 'lucide-vue-next';
import { GradientType } from '@wow-two-beta/fe/domain/color';
import { RadiusGlyph } from '@wow-two-beta/fe/presentation/display';
import type { Component } from 'vue';

/** Holds the preset glyph size in px. */
export const PresetIconSize = 16;

/** Defines one selectable projection preset. */
export interface GradientPreset {
  /** The applied value — an angle in degrees, or a radius extent. */
  readonly value: number;
  /** The tile's accessible label. */
  readonly label: string;
  /** The preview glyph component. */
  readonly glyph: Component;
  /** The glyph's props. */
  readonly glyphProps: Readonly<Record<string, unknown>>;
}

/** Defines one projection row — a gradient type's presets and the group's accessible name. */
export interface GradientPresetRow {
  /** The gradient type the row drives; the row is enabled only for it. */
  readonly type: GradientType;
  /** The group's accessible name. */
  readonly label: string;
  /** The presets. */
  readonly presets: ReadonlyArray<GradientPreset>;
}

/** Lists the projection rows, one per gradient type, in display order. */
export const GradientPresetRows: ReadonlyArray<GradientPresetRow> = [
  {
    type: GradientType.Linear,
    label: 'Gradient angle',
    presets: [
      { value: 0, label: 'Left to right (0°)', glyph: ArrowRight, glyphProps: { size: PresetIconSize } },
      { value: 45, label: 'Diagonal down-right (45°)', glyph: ArrowDownRight, glyphProps: { size: PresetIconSize } },
      { value: 90, label: 'Top to bottom (90°)', glyph: ArrowDown, glyphProps: { size: PresetIconSize } },
      { value: 135, label: 'Diagonal down-left (135°)', glyph: ArrowDownLeft, glyphProps: { size: PresetIconSize } },
    ],
  },
  {
    type: GradientType.Radial,
    label: 'Gradient radius',
    presets: [
      { value: 0.4, label: 'Tight radius', glyph: RadiusGlyph, glyphProps: { extent: 0.4 } },
      { value: 0.6, label: 'Compact radius', glyph: RadiusGlyph, glyphProps: { extent: 0.6 } },
      { value: 0.8, label: 'Wide radius', glyph: RadiusGlyph, glyphProps: { extent: 0.8 } },
      { value: 1, label: 'Full radius', glyph: RadiusGlyph, glyphProps: { extent: 1 } },
    ],
  },
];
