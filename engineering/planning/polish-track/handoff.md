# Handoff — forever-pin polish (p0.1), fresh chat

> Current state: [engineering plan](../planning.md).
> Earlier tree status and test counts below are historical snapshots.

*Last updated: 2026-08-15*

> Continuity doc for resuming p0.1 in a new chat. Plans of record: `polish-track/{p0.1,p0.2,p0.3}.md`. Conventions: `wow-two-ws/conventions/development/`.
> The prior chat filled its context; this captures state + the unblocked queue.

## Where we are

- **HEAD `6074732`**, tree **clean** (only dirty file is `vue-port-handoff.md` — another chat's, don't touch).
- **FE typecheck green** (verified this session). BE build not re-verified — baseline it first.
- **The v0.9 models sweep LANDED** — CM15 rule-carries-content is committed: rules are a polymorphic `CodeRule` list (`ConditionalRule` / `DefaultRule` / `DefaultPointerRule`) in jsonb (`010-rules-jsonb`); `CodeEntity.Content` dissolved into `rules[].content`; explicit `Mode` + `ContentType` fields.
- That sweep unblocked p0.1's remaining items (they were parked while its changeset was staged/red).

## Landed this session (all committed or in HEAD)

- **R4** (`0b33136`) — dropped vestigial expiry `NeverExpires`/`ExpiresAt`; migration `008`.
- **R1** (`f3dc03f`) — dropped `CodeType`; `BarcodeFormat` is the sole symbology; migration `009`. **Fixed a live preview bug**: non-QR formats had rendered as `Code128` (preview never sent `barcodeFormat`). Locked by 2 E2E tests.
- **Fallback-copy purge — 4 of 5 sites** (in HEAD): `CodeListQuery` · `ICodeRepository` · `CodeCreateCommandValidator` · `codes.ts`, all name-only now.
- **M1** — content-spec system deleted (by the sweep, not a p0.1 chunk): `IContentTypeSpec` · `ContentTypes` · `MobileAppLinkContentSpec` · `ContentProjection` · `ContentValidation` · `MobileAppLinkContentValidator`.
- **Planning reconciled** — `p0.1.md` Iter 12 compacted (steps dropped per the track convention) + defects D1–D8 reconciled against the sweep; `p0.2` Iter 1 + `p0.3` Iter 3 marked superseded/done; `validation.md` written (V1–V7); `D4` handed to `v0.9.md` step 13.

## p0.1 remaining — now unblocked, ordered

1. **D8 last site — `PreviewView.tsx:15`** stale "dynamic fallback" copy. **Re-verify first**: the preview contract changed (`CodePreviewApiRequest` now carries `mode` + `rules`, no `value`/`content`), so the wording may be stale in a *new* way. Reword to the current contract.
2. **Iter 14 — resolve `SelectField`** (`presentation/codes/content/components/fields.tsx`). One consumer (`WifiControls`). The SDK `Select` already takes an `options` prop while the wrapper *also* maps `<Select.Item>` children — redundant. If `options` alone renders the list, collapse to `<Field><Select options/></Field>` → inline + delete the wrapper; else keep it. FE green, so verifiable now.
3. **Iter 15 — test naming** (`ForeverPin.Tests.Unit` + `Tests.Integration` only; E2E + Migrations already conform). Form: `{Method}_{When}_{Assertion}`. The sweep rewrote these test bodies — confirm names against the settled files.

## Not p0.1's — leave alone

- **Content-model / validation** — owned by the v0.9 lane + a separate validation chat. `validation.md` shows presentation-validation work (P1 done 2026-07-28) by another chat. Don't touch.
- **R2** (`slug`) → v0.9 (CM13 dynamic-only slug). **R5** (`scanCount`) → deferred, keep the counter.
- **`IGeoBroker` stub** (`Country` never matches; language is wired) — a feature gap, its own task, not polish.
- Deferred-not-trimmed: center logo · file upload · local files — unbuilt.

## Conventions a fresh chat MUST know

- **Commits** (`repo/version-control/git.md`): `{type}: {past-tense verb} {what}`. No scope bracket. Past tense.
- **Agents never commit/push** — stage + print the message, the human commits. `guard-git` enforces.
- **Track cleanup** (`version-track.md`): on iteration done, drop the **steps**, keep the **tasks** (one-liners); git holds step detail. Iter 12 is the worked example.
- **Agentic workflow**: multiple chats edit one tree. Never revert/stash another lane's changes; found unexpected dirty files → stop and ask. Stay in your file lane.

## Verify

- BE: `dotnet test` per suite — `ForeverPin.Tests.{Unit,Integration,Migrations,E2E}` (E2E build also runs `pnpm build`).
- FE (`forever-pin.frontend-services/`): `pnpm typecheck · test · build`.
- Last check this session: FE typecheck green; full suites not re-run post-sweep — run them to baseline.
