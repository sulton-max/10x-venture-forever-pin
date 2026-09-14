# End-to-end tests

## Scope

The management and redirect hosts run against disposable PostgreSQL.
Google and Stripe are faked at the provider boundary.

- Identity, ownership, account claiming, and logout.
- Code creation, editing, search, preview, and downloads.
- Billing checkout, portal, webhook, limits, and cancellation.
- Create → scan → edit → scan propagation across both hosts.
- Redirect continuity when the subscription is canceled.

## Run

From the product root:

```bash
bash engineering/scripts/verify.sh backend
```

Docker and test-runner socket access are required.
Results: `engineering/operations/verification.md`.
These tests do not replace real OAuth, Stripe test-mode, or physical-scanner verification.
