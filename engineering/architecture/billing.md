# Billing

*Last updated: 2026-09-13*

## Flow

- Stripe-hosted Checkout creates subscriptions; Customer Portal manages them.
- `BillingController` exposes checkout, portal, webhook, and current billing state.
- Owner-scoped actions use the current user; Stripe webhooks use signature validation.
- `IBillingBroker` separates Stripe transport from application handlers.
- Subscription storage is keyed by user with a unique user index.
- Google and Stripe use deterministic fakes in E2E tests.

---

## Limits

| Plan | Code cap |
|---|---|
| Free | 3 |
| Solo | 25 |
| Pro | 200 |
| Agency | Unlimited |

`PlanLimitsConstants` owns the cap. `CodeCreateCommandHandler` counts all owned codes,
including static codes, before creation and returns `402` when the cap is reached.
The API serializes unlimited as `-1`.

Redirects never read the subscription, so cancellation does not deactivate a printed code.
The free-static proposal in marketing requires a later change to this counting policy.

---

## Configuration

| Setting | Environment override |
|---|---|
| `Billing:SecretKey` | `BILLING_SECRET_KEY` |
| `Billing:WebhookSecret` | `BILLING_WEBHOOK_SECRET` |
| `Billing:SuccessUrl` | `BILLING_SUCCESS_URL` |
| `Billing:CancelUrl` | `BILLING_CANCEL_URL` |
| `Billing:Prices:Solo` | Configuration value |
| `Billing:Prices:Pro` | Configuration value |
| `Billing:Prices:Agency` | Configuration value |

Prices map to Stripe price IDs; display amounts are not authoritative provider configuration.
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
