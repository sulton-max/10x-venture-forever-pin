/** Defines what the builder renders while it opens a code. */
export const CodeBuilderPhase = {
  /** Refers to a source code loading for an edit or a copy. */
  Loading: 'loading',
  /** Refers to a source code that failed to load — retry or go back; nothing editable renders. */
  Failed: 'failed',
  /** Refers to an editable form. */
  Ready: 'ready',
} as const;

export type CodeBuilderPhase = (typeof CodeBuilderPhase)[keyof typeof CodeBuilderPhase];
