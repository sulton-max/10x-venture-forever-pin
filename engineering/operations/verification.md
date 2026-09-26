# Verification

*Last updated: 2026-09-19*

## Executed

Baseline: `bash engineering/scripts/verify.sh all` under native execution approval.
The deployment lane reran all backend suites after adding five URL-routing cases; the table includes that result.

| Check | Result |
|---|---|
| Frontend TypeScript | Passed |
| Frontend unit tests | 4 passed |
| Frontend production build | Passed |
| Backend unit tests | 124 passed |
| Backend integration tests | 18 passed |
| Backend migration tests | 10 passed |
| Backend HTTP E2E tests | 61 passed |

Backend total: 213 passed, zero failed or skipped. The suites used the renamed assemblies.
Integration and HTTP tests used disposable PostgreSQL containers.

---

## Local deployment

- `docker compose config` passed with an explicit test password.
- Management and redirect images built from the final source tree.
- PostgreSQL, management, and redirect reached healthy state.
- Both `/health` endpoints returned HTTP `200` after database checks.
- The SPA returned HTML; an unknown `/api/*` route returned problem JSON.
- `/api/runtime-config` returned the configured public redirect origin.
- The database and data-protection key survived management-container replacement.
- The release generator produced valid JSON and a valid Compose model.
- Earlier verifier resources were removed; the separate DryDock pilot retains its named test volumes.

---

## Warnings and boundaries

- Dependency restore reports vulnerability warnings, including transitive SDK dependencies.
- The frontend bundle reports a large-chunk warning.
- Container publishing reports the same dependency vulnerability warnings.
- The main/tag workflows pass `actionlint 1.7.12`; hosted execution remains unverified.
- Linux-specific ARM images passed native SVG, redirect and persistence checks; hosted x64 remains unverified.
- Google and Stripe are faked in automated E2E.
- Real OAuth, real Stripe test mode, production deployment, and physical print/scanner checks remain unverified.
- Automated results do not close the owner's manual release checks.

---

## Permission resolution

The sandbox's socket restrictions blocked the original backend runner.
Native approval ran the same existing unit suite successfully before the rename.
The renamed full suite also completed under native approval.
No global sandbox or keychain restrictions were disabled.
The frontend config now initializes certificates only for dev serving.

---

## Rename checks

- Frontend typecheck, four tests, and production build passed again after the final verifier edit.
- All previously tracked product files exist at their mapped new paths.
- All 20 SQL migration files match their original committed bytes.
- Relative Markdown links resolve; Git whitespace checks pass.
- The workspace launcher resolves the renamed backend solution and frontend folder.
- The landing page and promo title were visually checked with the ForeverPin wordmark.
- The corrected hero still and 210-frame MP4 were exported successfully under native approval.
- The still was visually checked: domain labels and routing rows fit their cards.
- Output: sibling promo folder `out/hero.png` and `out/forever-pin-hero.mp4`.

No known execution blocker remains for the current test, render, or staging workflow.
Restricted operations still require native approval; no global permissions were weakened.
Production/provider configuration and manual release checks remain outside this verification.
