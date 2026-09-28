import { BarcodeFormat } from '@/domain/codes';

/** Maps each symbology to its display label. */
export const BarcodeFormatDisplays: Readonly<Record<BarcodeFormat, { readonly label: string }>> = {
  [BarcodeFormat.QrCode]: { label: 'QR code' },
  [BarcodeFormat.DataMatrix]: { label: 'Data Matrix' },
  [BarcodeFormat.Pdf417]: { label: 'PDF417' },
  [BarcodeFormat.Aztec]: { label: 'Aztec' },
  [BarcodeFormat.Code128]: { label: 'Code 128' },
  [BarcodeFormat.Ean13]: { label: 'EAN-13' },
  [BarcodeFormat.UpcA]: { label: 'UPC-A' },
};
