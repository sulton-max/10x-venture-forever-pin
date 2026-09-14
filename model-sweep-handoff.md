# Handoff — model names + comments sweep (v0.9 Iteration 8)

> Current state: [engineering plan](engineering/planning/planning.md).
> Earlier tree status and test counts below are historical snapshots.

*Last updated: 2026-08-15*

> This chat's lane: **v0.9 Iteration 8** — how the models *read*, not how they're shaped. Two halves: renames + doc passes in `forever-pin`, and the rules they were distilled into, in `wow-two-ws/conventions/`.
> Plan of record: `engineering/planning/version-track/v0.9/v0.9.md` § *Iteration 8*. Research: `model-conventions-research.md` (repo root).
> Sibling lanes have their own handoffs — `engineering/planning/polish-track/handoff.md` (p0.1) · `vue-port-handoff.md` · `product/marketing/handoff.md` · `models-handoff.md` (2026-07-04, FE tail, older lane).

---

## State

- **HEAD `6074732`** *(docs: swept every backend summary through the three gates)*.
- **Verified 2026-08-15:** BE build **0 errors** · `ForeverPin.Tests.Unit` **119 passed** · FE `pnpm typecheck` clean.
- Integration · E2E · Migrations **not run this session** (Docker/Testcontainers) — run them before closing v0.9.
- `forever-pin` tree: this lane holds **nothing uncommitted**. Dirty files belong to other lanes — `vue-port-handoff.md` · `product/marketing/handoff.md` · `engineering/planning/polish-track/handoff.md` (untracked) · `models-handoff.md` (untracked). **Leave them.**
- `wow-two-ws` tree: **35 convention files modified, uncommitted** — this lane's other half. See § *Open — the conventions commit*.

## Landed

- **Sub-step a — model type comments** *(done 2026-08-13)*. 120-char line limit landed as a convention and swept backend-wide: 269 doc blocks compacted across 141 files, all suites green. `documentation.md` gained the **comment anti-pattern catalogue** (nonlocal · too much · over-specification · redundant · mandated · inobvious connection · wrapped-instead-of-cut) and the **fact-routing table** — `<summary>` = consumer guarantee · `<remarks>` = consumer directive · `//` = maintainer rationale · a convention doc = team policy. The **falsifiability test** and the **referent/affordance split** are written into `documentation/summary.md`.
- **Sub-step b — model type names** *(done 2026-08-15)*. All **15** renames shipped, one commit each, suites green after every one: the 10 content models, `CodeContent`, and the 4 rule types → `*ValueObject`. Wire held throughout — `SubtypeRegistry` binds `CodeContentType.Wifi → typeof(…)` explicitly, so no discriminator and no stored jsonb moved. `VCard` + `Calendar` gained `Extensions/{X}ContentExtensions.ToPayload()`.
- **Frontend deliberately stays off the suffix** — zero `ValueObject` hits under `src/`, verified. Rationale in `v0.9.md` § *Why the frontend keeps its own names*: FE types are wire projections, and the divergence is already real (`CodeDto` has no `UserId` / `UpdatedAt` / `StyleJson`, carries a derived `shortUrl`).
- **Iteration 8's checkboxes reconciled 2026-08-15** — sub-step b was still showing *10 of 15* while the renames were committed; the doc now matches the source.

## Open — pick up here

**The conventions commit** *(do first — it's the only thing at risk)*

- `wow-two-ws` holds **35 modified convention files, +738/−308**, dated today. Load-bearing changes: `code-style/models.md` § *Naming* now mandates the `ValueObject` suffix with the frontend-`Dto` carve-out, and each doc's local starter table was replaced by a pointer to the canonical one in `documentation/summary.md`.
- Also dirty there, **not this lane's**: `.claude/hooks/*` · `launch.json` · `settings.json` · `scripts/active.sh` · `conventions/deployment/hosting/ports.md` (staged already) · untracked `vue-sfc.md` · `design-handoff.md` · `remotion-motion-graphics-playbook.md` · `ideas/3d-print-products-analysis.md` · `system/planning/`. Stage the conventions **only**, and confirm ownership of anything ambiguous before staging it.
- Agents stage and print the message; **the human commits** (`guard-git` enforces).

**Sub-step a remainder**

- Apply the falsifiability test to **every type summary** — the test is written, the sweep across non-content types isn't. Worked example lives at `v0.9.md:646`: `WifiContentValueObject` → `Represents the credentials of a Wi-Fi network.`, with payload shape and the escaped-not-trimmed rule moved onto `WifiContentExtensions`.

**Sub-step c — model member comments + names** *(the bulk of what's left)*

- Apply the falsifiability test to the **remaining member summaries** — entities, DTOs, requests, rules. The 10 content models are already reshaped to `Gets the…` with affordance and encoding claims cut.
- **Sweep member names, BE↔FE.** The symmetry rule covers members and optionality, never type names.
- **Suffix the FE wire models `Dto` uniformly** — the split is real: `domain/codes/rules/models/CodeRuleDto.ts` carries it, `domain/codes/content/models/WifiContent.ts` doesn't. Both are wire shapes; uniform `Dto` is the honest read.
- **Reproduce the reported calendar defect** — symptom was never captured; check whether the `Temporal` binding already absorbed it.

## Not this lane's

- **p0.1 polish** — `D8` preview copy · Iter 14 `SelectField` · Iter 15 test naming. Its own handoff.
- **v0.9 Iterations 9–12** — mode lock + copy UX · validation integration sweep · print export · SDK converters. Iteration 10 still carries *Green the backend* unticked; the **build** is green as of today, the **suites** are the outstanding half.
- **Iteration 1's 7 open forks** — resolve-page host (F3) · per-type resolve path (CM16) · `geo` path · preset definition · versioning seam (CM10) · which rule a pointer resolve records. Design decisions, owner's call.
- `IGeoBroker` uses `NoopGeoBroker`; country routing is unimplemented, language routing is wired. A feature gap, tracked elsewhere.

## Conventions a fresh chat must know

- **Commits**: `{type}: {past-tense verb} {what}`, subject only, 50–70 chars, no scope bracket.
- **A rename pass and a doc pass never share a diff** — that's why the 15 renames went one commit each.
- **Agents never commit or push.** Stage one cohesive change, print the message, stop.
- **Multiple chats share this tree.** Never revert, stash, or clean another lane's files; unexpected dirty files → stop and ask.
- **Track cleanup** (`version-track.md`): on iteration done, drop the **steps**, keep the **tasks**; git holds the detail.

## Verify

```bash
cd engineering/codebase/forever-pin.backend-services && dotnet test ForeverPin.Tests.Unit
cd engineering/codebase/forever-pin.frontend-services && pnpm typecheck && pnpm test && pnpm build
```

Integration · E2E · Migrations need Docker; the E2E build also runs `pnpm build`, so it gates the frontend too.
