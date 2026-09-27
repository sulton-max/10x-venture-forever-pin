# Migration tests

The suite applies ForeverPin's embedded SQL resources to disposable PostgreSQL and verifies
that every mapped product table can be read through the current EF model. It checks resource
coverage, product indexes, migration history, and idempotent startup by a second host.

Generic migrator drift, rollback, concurrency, and failure recovery belong to the SDK's
migration suite. Product host startup is also exercised by the E2E suite.

Run from the product root:

```sh
bash engineering/scripts/verify.sh backend
```

Docker and local test-runner socket access are required.
