import { Plan } from '@/domain/billing';

/** Defines how a plan is presented in billing; caps mirror the backend plan limits. */
export interface PlanDisplay {
  readonly name: string;
  readonly price: string;
  readonly cadence: string;
  readonly tagline: string;
  readonly highlights: ReadonlyArray<string>;
  /** The upgrade-ladder rank; only higher plans offer an upgrade. */
  readonly rank: number;
}

/** Maps each plan to its billing presentation. */
export const PlanDisplays: Readonly<Record<Plan, PlanDisplay>> = {
  [Plan.Free]: {
    name: 'Free',
    price: '$0',
    cadence: 'forever',
    tagline: 'For tinkerers and first codes.',
    highlights: ['3 dynamic codes', 'Unlimited scans', 'Codes never expire'],
    rank: 0,
  },
  [Plan.Solo]: {
    name: 'Solo',
    price: '$5',
    cadence: '/mo',
    tagline: 'For a small business with physical surfaces.',
    highlights: ['25 codes', 'Smart routing rules', 'Custom domain', 'Full analytics'],
    rank: 1,
  },
  [Plan.Pro]: {
    name: 'Pro',
    price: '$15',
    cadence: '/mo',
    tagline: 'For marketers and power users.',
    highlights: ['200 codes', 'A/B + advanced rules', 'Bulk generation', 'All code & content types'],
    rank: 2,
  },
  [Plan.Agency]: {
    name: 'Dev / Agency',
    price: '$39',
    cadence: '/mo',
    tagline: 'For developers and agencies.',
    highlights: ['Unlimited codes', 'REST API + keys', 'White-label', 'Client workspaces'],
    rank: 3,
  },
};

/** Maps the provider's subscription status to a label and whether it is in good standing. */
export function subscriptionStatusDisplay(status: string): { readonly label: string; readonly isHealthy: boolean } {
  switch (status) {
    case 'active':
      return { label: 'Active', isHealthy: true };
    case 'trialing':
      return { label: 'Trial', isHealthy: true };
    case 'past_due':
      return { label: 'Payment overdue', isHealthy: false };
    case 'unpaid':
      return { label: 'Unpaid', isHealthy: false };
    case 'canceled':
      return { label: 'Cancelled', isHealthy: false };
    case 'incomplete':
    case 'incomplete_expired':
      return { label: 'Payment incomplete', isHealthy: false };
    case 'paused':
      return { label: 'Paused', isHealthy: false };
    default:
      return { label: status || 'Unknown', isHealthy: false };
  }
}
