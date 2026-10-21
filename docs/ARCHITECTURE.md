# Architecture and organization

This repository has four layers. The dependency direction is inward: provider
tests depend on the shared contract suite; the runner consumes their results;
the publication site consumes the runner's matrix.

```text
tests/<Provider>.Tests and providers/*.json
        |
        v
src/pengdows.AdoNetApiTest.conformance.Databases
        |
        v
src/pengdows.AdoNetApiTest.conformance
        |
        v
src/pengdows.AdoNetApiTest.conformance.Runner
        |
        v
conformance-results/matrix.json|matrix.xml -> static site
```

## Shared contract suite

`src/pengdows.AdoNetApiTest.conformance/` contains the reusable ADO.NET
contract fixtures and tests. The shared tests are divided by intent:

- `Required/` contains behavior required by the documented `System.Data.Common`
  contract.
- `Optional/` contains capabilities a provider may decline. A declined
  capability is recorded as `not-implemented`, not skipped.
- `Common/` contains interoperability diagnostics that are useful but do not
  decide contract conformance.
- root-level types define fixtures, capability interfaces, type catalogs, and
  shared test helpers.

Provider projects under `tests/` derive from these bases and supply only the
factory, fixture, dialect, and provider-specific setup needed to execute them.

## Database and fixture infrastructure

`src/pengdows.AdoNetApiTest.conformance.Databases/` supplies the reusable
database boundary:

- `Configuration/` loads provider JSON and creates configured fixtures.
- `Lifecycle/` owns server/container startup and teardown.
- `Sql/` owns SQL-92 setup, value tables, DML setup, and connection helpers.
- the remaining files are provider-family SQL and type definitions.

The configuration-driven providers are generated into `tests/obj/providers/`.
Generated projects are disposable build inputs; provider JSON files remain the
source of truth.

## Runner and reporting

`src/pengdows.AdoNetApiTest.conformance.Runner/` is organized by responsibility:

- `Execution/` parses commands, discovers providers, generates configured
  projects, launches test processes, and records provenance.
- `Matrix/` parses xUnit results, classifies outcomes, builds the cross-provider
  matrix, and serializes JSON/XML results.
- `Certification/` evaluates the full ADO.NET contract and consumer profiles.
- `Site/` renders console summaries and the static certification pages.

The runner must not contain provider behavior. It observes test results and
turns them into evidence; contract assertions belong in the shared suite.

## Change boundaries

- Contract behavior changes belong in the shared suite and require regression
  coverage.
- Provider-specific SQL or capability declarations belong in the provider
  fixture or `providers/*.json`.
- Result interpretation changes belong in `Runner/Matrix/` and must have
  runner tests.
- Publication and historical-result changes belong in `Runner/Site/` and the
  Pages workflow.
- Generated files under `bin/`, `obj/`, and `conformance-results/` are outputs,
  not source changes.
