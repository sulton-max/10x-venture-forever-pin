# ForeverPin

Dynamic QR codes, barcodes, and programmable links.

**Pin it once. It points forever.**

## Status

- Implemented: code management, styling, SVG/PNG export, accounts, billing integration, and contextual routing.
- Active: the newest [version track](engineering/planning/version-track/) folder.
- Remaining: dynamic-content delivery, mode/copy UX, validation guarantees, and launch verification.
- Vue migration is a separate lane; this application still uses React.

---

## Development

Requires .NET 10, Node 24.11+, pnpm, and PostgreSQL. Full backend tests use disposable Docker containers.

```bash
nvm use
bash engineering/scripts/verify.sh all
```

The verifier also accepts `frontend`, `backend`, or `unit`.
It selects the installed `.nvmrc` runtime when available.

```bash
cd engineering/codebase/forever-pin.backend-services
dotnet run --project ForeverPin.Api
dotnet run --project ForeverPin.Redirect.Api
```

Run each host in its own terminal. Frontend development:

```bash
pnpm -C engineering/codebase/forever-pin.frontend-services dev
```

| Service | HTTPS | HTTP |
|---|---|---|
| Management API | 7020 | 7021 |
| Redirect API | 7022 | 7023 |
| Frontend dev | 7024 | opt-out on 7024 |

Frontend builds into the management API's `wwwroot`.
Dev certificates apply only to the dev server; builds and tests never install certificates.

---

## Configuration

- Both hosts: `DB_CONNECTION` or `DatabaseSettings:ConnectionString`.
- Both hosts: `Deployment:TrustedProxies` (IPs) and `Deployment:TrustedNetworks` (CIDR) for the ingress.
- Redirect host: `IP_LOCATION_DIRECTORY` holds the monthly DB-IP Lite database; the image uses `/data/geo`.
- Management API: `ApiSettings:RedirectBaseUrl` controls printed dynamic links.
- Google: frontend `VITE_GOOGLE_CLIENT_ID` and backend `Auth:Google:ClientId`.
- Stripe: [billing configuration](engineering/architecture/billing.md).
- Local overrides: ignored `appsettings.Local.json` and `.env.local`.

The existing physical database remains `smartqr` to retain local data.
[Rename compatibility](engineering/operations/rebrand.md) records intentional legacy identifiers.

---

## Project map

- [Version track](engineering/planning/version-track/): releases; the newest folder is the active version.
- [Architecture](engineering/architecture/architecture.md): implemented contracts.
- [Backlog](engineering/planning/backlog.md): every unbuilt item and the feature index.
- [Development](engineering/development/development.md): verification and permissions.
- [Product](product/product.md): positioning and pricing boundaries.
