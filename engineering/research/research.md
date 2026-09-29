# ForeverPin — Research

Technical research dumps feeding architecture + iteration decisions.

- `design-research/design-research.md` — product UI design source (light + dark, Tailwind v4 `@theme` + `@wow-two-beta/ui` tokens).
- `form-design-research/` — form-design exploration (`ideas.excalidraw`).
- `data-seam/` — Dapper-read / EF-write POC (runnable). Measures the `Attach` snapshot trap (load→attach→mutate) + the jsonb-converter gap. Feeds the backend-SDK read-seam chat and validation § *Phases*.
- `error-ordering/error-ordering.md` — cited answer to the open question in [validation](validation.md): resolve+authorize (`404`/`403`) vs payload validation (`400`/`422`). RFC 9110 / AIP-211 / CWE-203-204 / OWASP + Rails·DRF·Laravel·ASP.NET Core source-verified sequencing, rigour-tagged `[S]`/`[F]`/`[G]`/`[O]`.
- `validation.md` — validation wiring, issues V1–V7, the decided validator shape, field-path fixes, and the entity-gate and service-level placement.
- `validation-patterns.md` — named validation taxonomy and the state-comparison deep dive behind the workspace validation convention.
- `vector-sweep.md` — 2026-07-13 capability map of app versus SDK vectors: adopt, migrate, polish, add, defer.
- `gap-analysis-2026-09-19.md` — 2026-09-19 correctness and release-gap audit, one acceptance criterion per finding.
