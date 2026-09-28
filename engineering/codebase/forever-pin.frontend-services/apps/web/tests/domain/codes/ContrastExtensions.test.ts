import { describe, expect, it } from 'vitest';

import { ContrastExtensions, ContrastVerdict, defaultCodeStyle, type CodeStyleDto } from '@/domain/codes';

function style(patch: Partial<CodeStyleDto>): CodeStyleDto {
  return { ...structuredClone(defaultCodeStyle), gradient: undefined, ...patch };
}

describe('ContrastExtensions', () => {
  it('rates black on white as the maximum ratio', () => {
    expect(ContrastExtensions.ratio('#000000', '#ffffff')).toBeCloseTo(21, 5);
  });

  it('flags a lighter foreground as inverted before judging the ratio', () => {
    expect(ContrastExtensions.assess(style({ foregroundColor: '#ffffff', backgroundColor: '#000000' })).verdict).toBe(
      ContrastVerdict.Inverted,
    );
  });

  it('judges a gradient by its weakest stop', () => {
    const assessment = ContrastExtensions.assess(
      style({
        gradient: {
          type: 'linear',
          angle: 0,
          stops: [
            { color: '#000000', offset: 0 },
            { color: '#bbbbbb', offset: 1 },
          ],
        },
      }),
    );
    expect(assessment.verdict).toBe(ContrastVerdict.Low);
  });

  it('defers to the surface for a transparent background', () => {
    expect(ContrastExtensions.assess(style({ transparentBackground: true })).verdict).toBe(ContrastVerdict.Transparent);
  });
});
