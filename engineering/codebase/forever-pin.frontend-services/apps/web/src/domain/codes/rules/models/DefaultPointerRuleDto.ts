import type { CodeRuleType } from "../enums/CodeRuleType";

/** Represents a catch-all delegating to another rule's content. */
export interface DefaultPointerRuleDto {
  /** The rule-role discriminator. */
  type: typeof CodeRuleType.DefaultPointer;

  /** The `order` of the rule whose content serves the unmatched scan. */
  targetOrder: number;
}
