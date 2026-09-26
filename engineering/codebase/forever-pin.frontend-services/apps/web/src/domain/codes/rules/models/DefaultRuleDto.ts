import type { CodeContent } from "../../content/models";
import type { CodeRuleType } from "../enums/CodeRuleType";

/** Represents a catch-all serving its own content. */
export interface DefaultRuleDto {
  /** The rule-role discriminator. */
  type: typeof CodeRuleType.Default;

  /** The content served when no conditional rule matches. */
  content: CodeContent;
}
