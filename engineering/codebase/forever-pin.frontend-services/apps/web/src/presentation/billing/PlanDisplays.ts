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
    highlights: ['3 dynamic codes', 'Unlimited static codes', '7 days of analytics', 'Codes never expire'],
    rank: 0,
  },
  [Plan.Basic]: {
    name: 'Basic',
    price: '$1',
    cadence: '/mo',
    tagline: 'Billed yearly at $12. For a small business with printed codes.',
    highlights: ['25 dynamic codes', 'A full year of analytics', 'Everything in Free'],
    rank: 1,
  },
  // Not on sale: shown only to an account that already holds one.
  [Plan.Pro]: {
    name: 'Pro',
    price: '—',
    cadence: '',
    tagline: 'A legacy plan.',
    highlights: ['200 dynamic codes', 'A full year of analytics'],
    rank: 2,
  },
  [Plan.Agency]: {
    name: 'Dev / Agency',
    price: '—',
    cadence: '',
    tagline: 'A legacy plan.',
    highlights: ['Unlimited dynamic codes', 'A full year of analytics'],
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
