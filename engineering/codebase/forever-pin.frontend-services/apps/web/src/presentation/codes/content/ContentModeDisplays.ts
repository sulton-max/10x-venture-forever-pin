import { ContentMode } from '@/domain/codes';

/** Defines the display of a content mode — the choice is permanent, so the copy states what each costs. */
export interface ContentModeDisplay {
  /** The short label for the mode picker and the code card's chip. */
  readonly label: string;
  /** The one-line trade-off shown under the picker. */
  readonly note: string;
  /** The chip treatment on the code card. */
  readonly badge: 'outline' | 'brand';
}

/** Maps each content mode to its label, trade-off note and chip treatment. */
export const ContentModeDisplays: Readonly<Record<ContentMode, ContentModeDisplay>> = {
  [ContentMode.Static]: {
    label: 'Static',
    note: 'The code carries the content itself — it works offline, and changing the content makes a different code.',
    // Outline, not neutral — a neutral fill matches the card surface and stops reading as a chip.
    badge: 'outline',
  },
  [ContentMode.Dynamic]: {
    label: 'Dynamic',
    note: 'The code carries a short link — edit the content any time without reprinting.',
    badge: 'brand',
  },
};
