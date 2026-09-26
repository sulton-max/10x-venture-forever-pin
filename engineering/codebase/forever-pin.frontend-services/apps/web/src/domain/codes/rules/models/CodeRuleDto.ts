import type { ConditionalRuleDto } from "./ConditionalRuleDto";
import type { DefaultPointerRuleDto } from "./DefaultPointerRuleDto";
import type { DefaultRuleDto } from "./DefaultRuleDto";

/** Represents a routing rule; without a default rule, an unmatched scan does not resolve. */
export type CodeRuleDto = ConditionalRuleDto | DefaultPointerRuleDto | DefaultRuleDto;
