/** Represents one article's listing metadata — what the index, the router and link previews read. */
export interface PostMeta {
  /** The URL slug. */
  readonly slug: string;
  /** The article title. */
  readonly title: string;
  /** The listing and meta description. */
  readonly description: string;
  /** The publication date as ISO `YYYY-MM-DD`. */
  readonly date: string;
  /** The estimated reading time in minutes. */
  readonly readingMinutes: number;
  /** The category label shown as a pill. */
  readonly tag: string;
}

/**
 * Lists every article's metadata, with no body attached. The build reads it too, to write each article's
 * document head, so it stays plain data: no component imports.
 */
export const PostCatalog: ReadonlyArray<PostMeta> = [
  {
    slug: 'why-qr-codes-should-never-expire',
    title: 'Why your QR codes should never expire',
    description:
      'Print 10,000 flyers, then watch the vendor brick your code when you downgrade. Here\'s why "hostage codes" are the QR industry\'s original sin — and what to look for instead.',
    date: '2026-06-12',
    readingMinutes: 6,
    tag: 'Why',
  },
  {
    slug: 'smart-routing-one-code-many-destinations',
    title: 'One QR code, many destinations: how smart routing works',
    description:
      "Send iPhone users to the App Store, German visitors to your German store, and lunch-time scanners to the lunch menu — all from a single printed code. Here's how rule-based QR routing actually works.",
    date: '2026-06-11',
    readingMinutes: 6,
    tag: 'How-to',
  },
  {
    slug: 'qr-best-practices-that-scan',
    title: 'QR code best practices: sizes, error correction, logos and contrast that actually scan',
    description:
      "A QR code that looks great but won't scan is worthless. Here are the practical rules — quiet zone, error correction, minimum size, contrast, and logos — that keep codes readable from a phone.",
    date: '2026-06-10',
    readingMinutes: 7,
    tag: 'How-to',
  },
  {
    slug: 'dynamic-vs-static-qr-codes',
    title: 'Dynamic vs static QR codes: which should you use?',
    description:
      "Static codes are free and permanent but can't be edited. Dynamic codes are editable and trackable but depend on a provider. Here's the plain-English difference and how to choose.",
    date: '2026-06-09',
    readingMinutes: 5,
    tag: 'Basics',
  },
];
