# ForeverPin rebrand

*Last updated: 2026-09-13*

## Names

| Surface | Name |
|---|---|
| Product | ForeverPin |
| Domain | foreverpin.com |
| Product folder | 10x-venture-forever-pin |
| Promo folder | 10x-venture-forever-pin-promo |
| .NET namespace/project prefix | ForeverPin |
| Backend solution | forever-pin.backend-services |
| Frontend folder | forever-pin.frontend-services |
| Auth cookie | foreverpin-auth |

Source, project references, UI copy, metadata, example domains, promo composition IDs,
and workspace launch references use the new names.
Changing the auth-cookie name requires an existing signed-in browser to authenticate again.

The `10x-venture-` prefix classifies repository folders.
The brand, domain, package names, and source namespaces remain product-specific.

---

## Compatibility

- Physical database `smartqr` is retained in both default connection strings to avoid creating an empty replacement database.
- Applied SQL migration bytes are unchanged; historical comments remain to preserve migration checksums.
- SDK-generated SVG identifier `sqr-fg` stays in assertions because it belongs to the current SDK output.
- Recorded SDK theme identifiers remain unchanged until their owning SDK lane changes them.
- Original rendered promo outputs are preserved in the local rename backup.
- The main branded promo video and title still were rebuilt. The landing page was visually checked.
- The corrected hero still and 210-frame MP4 were exported under native approval. The still was visually checked: domain labels and routing rows fit their cards.
- Existing printed URLs and external deployment settings were not rewritten.

A future database/domain move requires a data and redirect continuity plan.

---

## Repository

Personal account: `sulton-max`.
The owner renamed GitHub to `sulton-max/10x-venture-forever-pin` on 2026-09-13; the screenshot confirms success.
Both local fetch and push URLs match `https://github.com/sulton-max/10x-venture-forever-pin.git`, verified on 2026-09-13.

No push was performed. Changes are being handed over as staged batches; see [staging batches](staging-batches.md).

---

## Verification

[Full results](verification.md): frontend checks and all four backend suites pass.
[Current plan](../planning/planning.md): the current release remains open.
