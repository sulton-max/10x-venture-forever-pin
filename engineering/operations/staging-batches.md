# Staging batches

*Last updated: 2026-09-14*

## Scope

The product repository is `sulton-max/10x-venture-forever-pin`.
The user commits each staged batch. No commit or push is authorized by this handover.
The workspace is a separate repository with unrelated concurrent changes.

## Batches

| Batch | State | Scope | Suggested commit subject |
|---|---|---|---|
| 1 | Committed: `da0b2f3` | Product source, namespaces, assemblies, UI branding, cookie, filenames, and naming-only documentation/path updates | `refactor: renamed Smart QR source and branding to ForeverPin` |
| 2 | Committed: `b4bac23` | `.nvmrc`, verification script, frontend Node requirement, dev-only certificate initialization | `fix: isolated dev certificates and standardized verification` |
| 3 | Staged | Current architecture, compact release tracks, task analysis, refreshed handoffs, operations evidence | `docs: compacted release plans and clarified remaining work` |
| 4 | Pending, workspace Git | Only this session's launcher, registry, IDE, and cross-repository path-reference changes | `chore: updated workspace references for the ForeverPin rename` |

## Batch 1 analysis

- Commit `da0b2f3` contains the naming changes projected from its parent; it excludes later documentation rewrites.
- There are 403 intended path moves and 38 changes at existing paths.
- Git reported 398 moves, five delete/add pairs, and 38 modifications. The delete/add pairs are short or heavily renamed files, not lost files.
- `git show --name-status da0b2f3` provides the historical path inventory; Git owns that record.
- The auth cookie becomes `foreverpin-auth`; existing signed-in browsers must authenticate again.
- Physical application database names and all 20 applied SQL migration files retain their existing bytes.
- The data-seam research sample uses its renamed disposable database default.
- Source code matches the tested working tree except the intentionally deferred Vite/Node changes and pre-existing backend whitespace.

## Batch 2 analysis

- Committed paths: `.nvmrc`, `engineering/scripts/verify.sh`, frontend `package.json`, and frontend `vite.config.ts`.
- The verifier selects the installed Node 24.11 runtime; backend-only checks do not require Node.
- Builds and unit tests skip development certificate installation; HTTPS remains enabled for normal dev serving.
- Fresh validation from the renamed root: shell syntax, frontend typecheck, four tests, and production build passed.
- Backend tests were not repeated for this batch; the previous full working-tree run passed 208 tests.
- The existing npm lockfile predates current dependencies. pnpm is the active package manager; npm lockfile reconciliation is outside this staged change.

## Batch 3 analysis

- Current architecture, release plans, product state, handoffs, and execution evidence form one documentation batch.
- All 36 open tasks in the pre-rename working-tree snapshot were retained; backend verification alone was closed using executed results.
- The active version stays v0.9. Its manual user-flow checks remain unticked.
- The Vue handoff retains migration deltas but identifies SDK readiness as historical; theme guidance preserves the SDK's actual `smart-qr` ID.
- The historical batch-1 file listing is available from Git instead of duplicated in a committed text file.
- No source code, SDK implementation, or workspace index change belongs to this batch.
- Checked all 65 Markdown files for relative links: no broken links. Documentation whitespace checks pass.

## Batch 1 validation

- The staged path set matches the explicit batch manifest.
- `git diff --cached --check` passes.
- Staged SQL bytes match HEAD.
- The renamed working tree previously passed 208 backend tests, four frontend tests, typecheck, and production build.
- Those test counts describe the working tree with the runtime fixes present; an isolated export of this staged tree was not rerun.

## Remaining boundaries

- Existing whitespace in `HostConfiguration.cs` stays unstaged.
- The documentation batch includes the existing handoffs and preserves their historical evidence.
- No workspace index changes were made by this staging operation.
- The sibling promo source has no independent Git repository and is ignored by the workspace. Committing the product will not preserve the promo source; its repository placement must be decided separately.
- The corrected hero still and MP4 were exported under native approval; the still was visually checked.
- The owner completed the GitHub rename and both local remote URLs; see [rebrand](rebrand.md).

## Continuation

Batch 3 is staged: 54 documentation files. After the owner commits it, review the workspace reference batch separately.
Preserve unrelated staged files if another chat uses the index between handovers.
