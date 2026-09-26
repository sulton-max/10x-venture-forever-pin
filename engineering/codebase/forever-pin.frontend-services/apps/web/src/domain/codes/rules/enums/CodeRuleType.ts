/** Defines a routing rule's role. */
export const CodeRuleType = {
  /** Refers to a conditional rule evaluated in order. */
  Conditional: "conditional",

  /** Refers to a catch-all serving its own content. */
  Default: "default",

  /** Refers to a catch-all delegating to another rule's content. */
  DefaultPointer: "defaultPointer",
} as const;

export type CodeRuleType = (typeof CodeRuleType)[keyof typeof CodeRuleType];
