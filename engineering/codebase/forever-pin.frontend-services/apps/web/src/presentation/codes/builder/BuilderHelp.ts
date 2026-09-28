/** Holds the builder's control explanations, shown behind each control's info button. */
export const BuilderHelp = {
  name: 'Only you see the name. It labels the code in your list and names its downloads.',
  contentType: 'What a scan delivers: a link, text, a contact card, Wi-Fi access, a calendar event and more.',
  mode: 'Dynamic codes hold a short link you can repoint after printing, with routing and analytics. Static codes bake the content into the symbol: they work offline and never change.',
  shortLink: 'The address this code encodes. It never changes; you change where it leads.',
  rules:
    'Rules run top to bottom and the first match wins. A scan that matches no rule follows the catch-all, so every scan lands somewhere.',
  codeType:
    'QR codes scan with any phone camera. Data Matrix, Aztec and PDF417 usually need a scanner app; linear barcodes suit retail and stock.',
  fill: 'A solid color or a gradient for the modules. Phones read contrast, so keep them dark on a light background.',
  colors: 'Pick colors with strong contrast; the builder warns when the code would be hard to scan.',
  background: 'The quiet area around and behind the code. A transparent background only scans on a light surface.',
  body: 'The module shape. Rounded and dotted bodies scan as reliably as squares at normal print sizes.',
  eyes: 'The three corner markers phones lock onto. Keep them clearly visible and distinct from the body.',
  emoji: 'A center emoji covers modules that error correction rebuilds. Keep it small, and test-scan before printing.',
} as const;
