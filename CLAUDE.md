# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

An xUnit v3 conformance suite for ADO.NET providers (forked from mysql-net/AdoNetApiTest, maintained by pengdows.crud).
It tests the **documented `System.Data.Common` contract**, not "behaves like SQL Server", and round-trips every native
type of each database to find drivers that silently lose or alter data. `README.md` and
`src/pengdows.AdoNetApiTest.conformance/CONTRACT-COVERAGE.md` are the authoritative descriptions of outcomes and
classification rules; `AGENTS.md` holds style and commit conventions.

## Commands

SDK is pinned by `global.json` (.NET 10). Provider projects mostly target `net8.0;net10.0`.

```bash
dotnet build pengdows.AdoNetApiTest.conformance.sln
dotnet test tests/Repository.Tests/Repository.Tests.csproj   # repo guard tests (CI gate)
dotnet test tests/Runner.Tests/Runner.Tests.csproj           # runner tests (CI gate)

# Conformance run: builds each provider project, writes conformance-results/{matrix.json,matrix.xml,<Provider>.log}
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- run Sqlite MicrosoftSqlite8
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- run Sqlite --no-build --timeout 15
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- report out/Sqlite.xml   # from existing xUnit XML
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- run --exclude Informix,Ydb   # all but the named
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- site   # certification site from matrix.json
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- --help

# One provider / one test
dotnet test tests/Sqlite.Tests --filter "Category=Required|Category=Optional"
dotnet test tests/Sqlite.Tests --filter "FullyQualifiedName~SqliteCommandTests.Connection_can_be_unset"

```

The runner defaults to `--config Release`; with `--no-build` it runs whatever Release binaries exist, so after changing
code either let it build or build Release first.

**Database servers come from Testcontainers.** SQL Server, MySQL and PostgreSQL provider projects need only Docker
running: `src/pengdows.AdoNetApiTest.conformance.Databases/DatabaseServer.cs` defines `PostgreSqlServer`, `MySqlServer`
and `SqlServerServer`, xUnit v3 assembly fixtures that each provider project registers in its `*DbFactoryFixture.cs`
(`[assembly: AssemblyFixture(typeof(PostgreSqlServer))]`). Each test process gets its own container on a Docker-assigned
host port, with data on tmpfs, so runs can overlap freely; the fixture builds its connection string from
`Host`/`Port`/`User`/`Password`/`Database`. The container is disposed after the last test, and Ryuk removes it if the
process dies. A new server-backed provider follows the same pattern; never bind a fixed host port.

**Docker cleanup is mandatory.** Every container started for testing must be gone (containers, network and volumes)
when the work ends, pass or fail. For an ad-hoc database the fixtures don't cover (Informix, DB2, a different server
version), prefer Testcontainers; otherwise use `docker run --rm` with a random port (`-p 127.0.0.1::<port>`), `--tmpfs`
for the data directory where the server supports it, and `docker rm -f` it before finishing. Don't touch containers you
didn't start.

Runner exit codes: `0` PASS, `1` FAIL, `2` usage error. CI runs only the embedded providers (`Sqlite MicrosoftSqlite8`)
and publishes the matrix without gating on it, since real providers legitimately fail Required tests.

Connection strings: `DbFactoryTestBase` and `Databases/Utility.cs` prefer the `ConnectionString` environment variable
over the fixture's value, and when it is set the server fixtures start no container. Informix and YDB read `INFORMIX_CONNECTION_STRING` /
`YDB_CONNECTION_STRING`. `DuckDB.Tests` and `FlatFile.Tests` reference sibling checkouts (`../../../DuckDB.NET`,
`../../../pengdows.flatfile`); their `*NuGet.Tests` twins link the same sources against released packages.

## Architecture

- **`src/pengdows.AdoNetApiTest.conformance/`** — the shared library (the NuGet package). Abstract
  `XTestBase<TFixture>` classes (Connection, Command, DataReader, Parameter, Transaction, DbProviderFactory,
  TypeFidelity, ...) all derive from `DbFactoryTestBase<TFixture>`. Each base is a **partial class split by group**:
  `XTestBase.cs` at the root holds helpers, and the tests live in `Required/XTestBase.Required.cs`,
  `Optional/XTestBase.Optional.cs`, `Common/XTestBase.Common.cs`.
  Capabilities are declared by fixture interfaces (`IDbFactoryFixture`, `ISelectValueFixture`, `IDmlFixture`,
  `ITransactionSemanticsFixture`, `ITypeFidelityFixture`, ...). SQL dialect differences go through overridable hooks
  (`SelectSql`, `SelectOneSql`, ...) and `ParameterMarkerDiscovery` (reads `ParameterMarkerFormat`, falls back to `?`)
  — never hard-code `@`, `FROM dual`, batching syntax or a provider's exception subclass in shared tests.
- **`src/pengdows.AdoNetApiTest.conformance.Databases/`** — per-database setup/SQL helpers and type catalogs
  (`SqlServerTypes`, `SqliteTypes`) used by provider fixtures.
- **`src/pengdows.AdoNetApiTest.conformance.Runner/`** — the `run`/`report` CLI. `ProviderDiscovery` treats every
  `tests/X.Tests/X.Tests.csproj` (except Repository/Runner) as provider `X`; `ProviderRunner` executes it with
  `dotnet run -- -xml`; `Classifier` maps xUnit results + traits + attachments to outcomes; `Matrix*`/`ConsoleReport`
  produce the output. Adding a provider = adding a folder; no registration needed.
- **`tests/<Provider>.Tests/`** — a fixture plus one thin derived class per test base. Provider-specific overrides
  belong here, not in the shared library.
- **`providers/*.json`** — a provider as a configuration (driver package and factory, docker/cloud/file target,
  connection string, SQL-92 deviations, types to test). `ConfiguredProjects` in the runner generates a project for each in
  `tests/obj/providers` (gitignored); `ConfiguredProvider`/`ConfiguredServer`/`ConfiguredFixture` in the Databases
  project read it at run time. Prefer a configuration for a new driver version or server; a hand-written project only
  when it needs custom code (a hand-written type catalog, non-SQL-92 DDL). `Sql92Dml` and `SelectValueTables` build the
  DML and reader tests' statements for configurations and for the `DmlFixture<T>`/`SelectValueFixture<T>` wrappers that
  hand-written projects use. `Repository.Tests/ProviderCoverageTests` fails when a provider project leaves out a test
  base with Required tests, since that provider could only ever be INCOMPLETE.
- **Single writer** — SQLite, DuckDB, Access and pengdows.flatfile allow one writer at a time. Their fixtures implement
  `ISingleWriterFixture` and their projects set `[assembly: CollectionBehavior(DisableTestParallelization = true)]`
  (a `file` config target does both unless `"singleWriter": false`); a test that needs two writers at once checks the
  marker and reports not applicable. `ProviderCoverageTests` keeps the hand-written projects in step.
- **Type fidelity** — `FidelityTestBase` is the shared runner; `TypeFidelityTestBase` takes a hand-written catalog
  (`SqlServerTypes`, `PostgreSqlTypes`, `OracleTypes`, ...) and `DataTypesFidelityTestBase` generates one from the
  provider's `GetSchema("DataTypes")` (plus `IServerTypesFixture` extras). Where a provider does not advertise SQL syntax,
  assume SQL-92 quoting, `;`, and `@name` parameters.

### How outcomes flow

Group attributes `[RequiredFact]`, `[OptionalFact]`, `[CommonFact]` (`GroupAttributes.cs`) emit `Category`,
`Contract`, `Because` and `Waiver` traits. Tests never skip; instead they call `ConformanceOutcome.NotImplemented /
NotApplicable / Different(reason)` (and `Finding(...)` for type fidelity), which writes a `[CONFORMANCE outcome]` line
and an xUnit attachment that the runner's `Classifier` reads. A Required test with `Waiver = "..."` that fails becomes
an advisory `deviation`. `SoftWarning.Report` is for Common tests only.

## Rules enforced by `tests/Repository.Tests` (they will fail the build)

- Every shared test has exactly one group attribute and lives in the folder named after its group.
- Every shared test sets `Contract = "https://..."` (the generated `Common/GetValueConversionTestBase.g.cs` is exempt).
  Required belongs only to behavior a quoted sentence on a learn.microsoft.com page states.
- No skips anywhere (`Assert.Skip`, `Skip =`, `SkippableFact`, `SkipException`, ...), in `src/` or provider projects.
- Use `Assert.ThrowsAny`/`ThrowsAnyAsync`, never `Assert.Throws` — a documented exception's subclass must pass.
- Waivers: Required only, a reason of 20+ characters, and still a learn.microsoft.com `Contract`.
- Required files never call `SoftWarning.Report`.
- The old upstream project names (listed in `tests/Repository.Tests/RenameTests.cs`) appear only in history/credit files;
  `LICENSE` keeps the upstream notice; docs don't describe skips or soft warnings.
- `Runner.Tests/ReadmeTests.cs` requires the README to have a table row for every outcome and fidelity status and to
  show the runner's commands, so adding an outcome, status or command means updating the README too. New shared tests should also be reflected in `CONTRACT-COVERAGE.md`.
