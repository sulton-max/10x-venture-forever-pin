import { describe, expect, it } from 'vitest';

import { BarcodeFormat, PrintSizing } from '@/domain/codes';

const Qr =
  '<?xml version="1.0"?>\n<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 37 37" shape-rendering="crispEdges" width="740" height="740"><rect width="37" height="37"/></svg>';

describe('PrintSizing', () => {
  it('reads the symbol size in modules from the root viewBox', () => {
    expect(PrintSizing.modules(Qr)).toEqual({ across: 37, down: 37 });
    expect(PrintSizing.modules('<svg width="10" height="10"></svg>')).toBeUndefined();
  });

  it('keeps small symbols at the camera minimum and grows dense ones by module size', () => {
    expect(PrintSizing.minimumWidthMm(BarcodeFormat.QrCode, 37)).toBe(20);
    expect(PrintSizing.minimumWidthMm(BarcodeFormat.QrCode, 89)).toBe(36);
    expect(PrintSizing.minimumWidthMm(BarcodeFormat.Ean13, 113)).toBe(38);
  });

  it('relates a printed width to its module size and comfortable scanning distance', () => {
    expect(PrintSizing.moduleMm(37, 37)).toBe(1);
    expect(PrintSizing.scanDistanceMm(30)).toBe(300);
  });

  it('sets a physical width and a proportional height, replacing the pixel size', () => {
    const sized = PrintSizing.withPhysicalWidth(Qr.replace('0 0 37 37', '0 0 40 20'), 50);

    expect(sized).toContain('<svg width="50mm" height="25mm" xmlns=');
    expect(sized).not.toContain('width="740"');
    expect(sized).toContain('<rect width="37" height="37"/>');
  });
});
