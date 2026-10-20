# Billing

*Last updated: 2026-10-20*

## Flow

- Stripe-hosted Checkout creates subscriptions; Customer Portal manages them.
- `BillingController` exposes checkout, portal, webhook, and current billing state.
- Owner-scoped actions use the current user; Stripe webhooks use signature validation.
- `IBillingBroker` separates Stripe transport from application handlers.
- Subscription storage is keyed by user with a unique user index.
- Google and Stripe use deterministic fakes in E2E tests.

---

## Limits

| Plan | On sale | Dynamic codes | Analytics reach |
|---|---|---|---|
| Free | yes | 3 | 7 days |
| Basic | yes, $12/year | 25 | 366 days |
| Pro | no | 200 | 366 days |
| Agency | no | Unlimited | 366 days |

- `PlanLimits` owns the caps; static codes are unlimited on every plan.
- `CodeCreateCommandHandler` counts owned dynamic codes before a dynamic create and returns `402` at the cap.
- `PlanLimits.EffectivePlan` entitles a subscription's plan while it is active, trialing or past due; else Free.
- A lapsed owner keeps every code and can edit it; only new dynamic codes past Free's 3 are refused.
- The analytics query cuts a longer window to the plan's reach and returns `maxDays`.
- Checkout sells Basic only; Pro and Agency stay for later and are refused at checkout.
- The API serializes unlimited as `-1`.

Redirects never read the subscription, so cancellation does not deactivate a printed code.

---

## Configuration

| Setting | Environment override |
|---|---|
| `Billing:SecretKey` | `BILLING_SECRET_KEY` |
| `Billing:WebhookSecret` | `BILLING_WEBHOOK_SECRET` |
| `Billing:SuccessUrl` | `BILLING_SUCCESS_URL` |
| `Billing:CancelUrl` | `BILLING_CANCEL_URL` |
| `Billing:PortalReturnUrl` | `BILLING_PORTAL_RETURN_URL` |
| `Billing:Prices:Basic` | `BILLING_PRICE_BASIC` |

- `Billing:Prices:Basic` is a Stripe recurring price of $12 per year; the app shows it as $1 a month.
- Prices map to Stripe price IDs; display amounts are not authoritative provider configuration.
Return URLs land on `/app/billing`: Checkout adds `?status=success` or `?status=cancelled`, the portal adds nothing.
Defaults return to the single-host origin `https://localhost:7020`; Development returns to the Vite origin `https://localhost:7024`.
The SPA treats `success` as provisional and re-reads the snapshot until the webhook confirms a paid plan.
Use ignored local overrides or environment configuration for secrets.

---

## Verification and open work

- Automated E2E covers checkout, portal, webhooks, limits, and cancellation behavior with a fake broker.
- Real Stripe test-mode verification remains required.
- Cross-account guest-subscription merge semantics remain unresolved.
- Marketing's $12/year experiment is not the current $5/$15/$39 display model.
- Confirm return URLs and provider callbacks against the actual deployed origin.

Source: `ForeverPin.Application/Billing/`, `ForeverPin.Infrastructure/Billing/`,
`ForeverPin.Persistence/`, and `ForeverPin.Api/Controllers/BillingController.cs`.
Paths are relative to the backend service folder.
