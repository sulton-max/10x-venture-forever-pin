/** Defines the subscription tiers. */
export const Plan = {
  /** Refers to the free tier. */
  Free: 'free',
  /** Refers to the paid Basic tier, the one on sale. */
  Basic: 'basic',
  /** Refers to the Pro tier; not on sale. */
  Pro: 'pro',
  /** Refers to the Dev / Agency tier; not on sale. */
  Agency: 'agency',
} as const;

export type Plan = (typeof Plan)[keyof typeof Plan];

/** Lists the paid plans on sale, in upgrade order. */
export const PAID_PLANS: ReadonlyArray<Plan> = [Plan.Basic];

/** Defines a billing checkout request. */
export interface CheckoutRequest {
  /** The paid plan to subscribe to. */
  plan: Plan;
}

/** Represents a hosted billing-session URL. */
export interface SessionUrlDto {
  /** The hosted session URL. */
  url: string;
}

/** Represents a billing plan's usage limits. */
export interface LimitsDto {
  /** The maximum number of owned dynamic codes; `-1` means unlimited. Static codes are unlimited. */
  maxDynamicCodes: number;

  /** How many days back the plan's analytics reach. */
  analyticsDays: number;
}

/** Represents an account's code usage. */
export interface UsageDto {
  /** The number of owned dynamic codes, the ones the plan caps. */
  dynamicCodeCount: number;
}

/** Represents an account's billing snapshot. */
export interface BillingStatus {
  /** The billing plan; `Free` when there is no subscription. */
  plan: Plan;

  /** The lowercase subscription status; `active` for the free plan. */
  status: string;

  /** The billing plan's usage limits. */
  limits: LimitsDto;

  /** The account's code usage. */
  usage: UsageDto;
}
