# pengdows.AdoNetApiTest.conformance

A suite of [xUnit.net](https://xunit.github.io/)-based conformance tests for ADO.NET providers, i.e.,
classes that inherit from `DbConnection`, `DbCommand`, `DbDataReader`, etc.

## Why this exists

This is the successor to [mysql-net/AdoNetApiTest](https://github.com/mysql-net/AdoNetApiTest) (Bradley Grainger,
MIT license), which it was forked from; the original's history is kept in this repository. If you were looking for
that project, this is where it went on.

It is maintained by the [pengdows.crud](https://github.com/pengdows/pengdows.crud) project. pengdows.crud runs on
many databases through their vendors' ADO.NET drivers, and testing it showed that drivers return wrong data in
ways the ADO.NET documentation does not allow (a value truncated, shifted, or read back as a different value). This
suite exists to find those defects independently of any one data-access library, so that each can be reported to
its driver's maintainers and a minimum driver version can be recorded for it.

The premise: test the **documented contract** of `System.Data.Common`, not whether a provider behaves like SQL
Server, and test **every type each provider supports** with boundary values to see what survives a round trip.
See [CONTRACT-COVERAGE.md](src/pengdows.AdoNetApiTest.conformance/CONTRACT-COVERAGE.md) for the classification rules.

## How to Use

The NuGet package is not published yet. Until it is, reference the `src/pengdows.AdoNetApiTest.conformance`
project directly (the library is unsigned).

Create a class that implements `IDbFactoryFixture`: the
`Factory` property must return a `DbProviderFactory` for your ADO.NET implementation,
and the `ConnectionString` property must return a valid connection string that the test
suite can use to connect to a database. See
[this example](https://github.com/pengdows/pengdows.AdoNetApiTest.conformance/blob/main/tests/SqlClient.Tests/SqlClientDbFactoryFixture.cs) for
`System.Data.SqlClient`.

To use the tests that execute SQL statements, you must implement the `ISelectValueFixture`
interface. Examples of doing this can be found in the `tests/*` projects and `pengdows.AdoNetApiTest.conformance.Databases`.
Where SQL-92 is enough, `SelectValueFixture<YourFixture>` and `DmlFixture<YourFixture>` (in the Databases package) supply
the reader and DML tests' statements over your connection; override the column types or binary literal where your
database spells them differently.

Create one public derived class from each `XTestBase<T>` test base class. Add a public
constructor that takes an instance of your `IDbFactoryFixture` implementation and passes it
to the base class.

Nothing is skipped: the shared tests report `not-applicable` or `not-implemented` themselves, based on what your
fixture declares. If a Required test fails, the provider does not meet the documented contract.

## Version History

See the [changelog](CHANGELOG.md). The original project's release notes are in
[UPSTREAM-VERSION-HISTORY.md](UPSTREAM-VERSION-HISTORY.md).

## Running the conformance check

You need the .NET SDK pinned in `global.json`. Providers that talk to a server (SQL Server, PostgreSQL, MySQL, Oracle,
Firebird, TiDB) also need Docker: each of those test projects starts its own server in a container (with [Testcontainers](https://dotnet.testcontainers.org/))
before its first test and removes it, with its data, when the run ends. If the test process is killed, Testcontainers'
reaper removes it. Docker picks the host port, so several providers, or several copies of this repository, can run at
the same time. To test against a server you already have, set the `ConnectionString` environment variable; no container
is started then. SQLite and the other embedded providers need nothing.

From the repository root:

```text
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- run Sqlite
```

`run` builds and runs each named provider's test project (every provider if you name none), then prints one line per
provider and an overall verdict:

```text
Sqlite    FAIL        Required 96/112 (8 failed, 8 not covered)  |  Optional 24 implemented pass, 0 implemented fail, 7 not implemented  |  Common 276 differ  |  Types 499 exact, 0 lossy, 0 wrong type, 0 throws
OVERALL: FAIL
Matrix: conformance-results/matrix.json, conformance-results/matrix.xml
```

`Required 96/112` is passed over total, and the total is the same for every provider: every Required test the shared
library declares, read from the library itself, whichever providers run and whatever areas each has tests for. The
parenthesis says what became of the rest: `failed` tests ran and failed; `not run` tests belong to an area the provider covers but reported no result (a crashed test host, a result
file from an older suite); `not covered` tests are in an area the provider has no tests for at all (such as `Dml` without
a fixture). `passed + failed + not run + not covered` always equals the total, and the same figures are in
`matrix.json` and `matrix.xml`.

The exit code is `0` for PASS, `1` for anything else (FAIL, INCOMPLETE, NOT RUN) and `2` for a usage error, so it works
in a script or CI job.

**A provider passes** when every Required test ran, none failed, and no Optional test failed. A Required test that never
ran is not a pass: a provider with no failure but some Required tests not covered is `INCOMPLETE`, which says the gap is
in its tests rather than in the driver, and it fails the overall verdict like `FAIL`. Nothing is skipped:

| Outcome | Counts as | Meaning |
| --- | --- | --- |
| `pass` | pass | The test ran and passed. |
| `fail` | fail | A Required test failed: the provider does not meet the documented contract. |
| `implemented-pass` | pass | The provider implements this optional capability and the test passed. |
| `implemented-fail` | fail | The provider implements this optional capability and the test failed. |
| `not-implemented` | pass | The provider does not implement this optional capability. |
| `not-applicable` | pass | The check does not apply, such as a data type the database does not have. |
| `different` | advisory | The provider departs from a common expectation the contract does not require. Never fails the provider. |
| `not-run` | fail | No result was reported, so nothing is known. For a **Common** test this is advisory instead (shown as `N not run` on the Common part of the console line): it cannot hide a Required result. |
| `not-covered` | advisory | The provider's test project has no tests for this area at all, so nothing is known about it. Counted in the Required total but not as a failure; a provider with any Required test not covered is `INCOMPLETE`, and the console line says `Coverage N of M areas`. |

A provider whose test project could not run at all (a server that is down, a build that failed, a timeout) is reported as
`NOT RUN` with the reason, and it fails the overall verdict: a provider that could not be tested is never reported as
passing. The provider's own output is kept next to the matrix as `conformance-results/<Provider>.log`.

**The matrix** (`matrix.json` and `matrix.xml`, the same data) has a legend that defines every outcome, one entry per
provider with its verdict and counts, and one entry per test with its area, group, the documentation page it checks, and
each provider's outcome and failure message.

Other commands and options:

```text
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- run                 # every provider
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- run Sqlite DuckDB   # these two
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- run Sqlite --no-build --json out/m.json --xml out/m.xml
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- report out/Sqlite.xml out/DuckDB.xml
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- --help
```

`report` builds the matrix from xUnit result files you already have (`dotnet run --project tests/Sqlite.Tests -- -xml
out/Sqlite.xml`), naming each provider after its file.

To see only the hard result without the runner, filter a provider's tests by group:
`dotnet test tests/Sqlite.Tests --filter "Category=Required|Category=Optional"`.

## Upstream fixes

When a provider fails a Required test, we send its maintainers a fix with a regression test and the documentation
sentence it follows. [docs/UPSTREAM.md](docs/UPSTREAM.md) lists the pull requests and issues, and holds drafts for the
trackers that do not take pull requests.

## Certification site

`site` turns a matrix into a static site: an index of every provider with its certification status, and a page per
provider with the evidence behind it.

```text
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- run --exclude Informix,Ydb
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- site
```

`site` reads `conformance-results/matrix.json` (or the matrix named on the command line) and writes
`conformance-results/site` (`--out` to change it). It needs nothing but the matrix, so a published matrix can be turned
into the same site by anyone.

| Status | Verdict | Meaning |
| --- | --- | --- |
| Certified | PASS | Every Required test ran and passed, and no Optional capability the provider implements failed. |
| Incomplete | INCOMPLETE | Nothing failed, but some Required tests have not run for the provider, so it cannot be certified. |
| Not certified | FAIL | A Required test, or an Optional capability the provider implements, failed; each failure links the documentation it breaks. |
| Not tested | NOT RUN | The provider could not be run, for the reason shown. |

Each provider page shows what was tested, as recorded by its own test process rather than its project file: the driver
assembly and version that loaded, the server's version and container image, the runtime and operating system, and, for
a configured provider, its configuration file and checksum, with the suite's commit. A run on another machine with the
same configuration is the same test. Type fidelity and the comparison of `DataTypes` with the server are shown for every
provider but do not decide its status. The `Pages` workflow runs every provider GitHub's runners can run, builds the
site and publishes it.

## Type fidelity

For every type a database supports, a provider should store a CLR value and give back the same value, as the right CLR
type. `TypeFidelityTestBase<TFixture>` checks that. A provider's fixture supplies a catalog of its native types
(`ITypeFidelityFixture.Types`): the column definition, the `DbType` to bind with, the CLR type to expect, and values
that matter (minimum, maximum, empty, `NULL`, the finest fractional second, Unicode outside the BMP, trailing spaces,
embedded `NUL`, an offset). Each value takes three paths, so a failure says where it happened:

| Path | What it does |
| --- | --- |
| `read` | writes the value with a SQL literal and reads it back through the provider |
| `write` | writes the value with a parameter, then compares how the database itself renders it against the literal's rendering, so the read side of the provider takes no part |
| `roundtrip` | writes with a parameter and reads back through the provider, with extra checks `type` (`GetFieldType` and `GetValue` agree with the expected CLR type), `typed` (`GetFieldValue<T>`) and `where` (`WHERE V = parameter` finds the row) |

A database's own rules are written into the expectation, never reported as a driver bug: a `char(10)` comes back padded
and a `datetime` keeps 3.33 ms steps. Each finding is one of:

| Status | Meaning |
| --- | --- |
| `exact` | The value, and its CLR type, came back unchanged. |
| `lossy` | The type is right but the value changed: truncated, rounded, padded or shifted. |
| `wrong-type` | The value came back as a different CLR type than expected. |
| `throws` | The provider threw instead of returning the value. |
| `not-applicable` | The check does not apply to this type. |

Findings are advisory (the tests are in the Common group) and do not change a provider's PASS or FAIL. The runner
prints `Types N exact, N lossy, N wrong type, N throws` per provider and writes every finding into the matrix, with the
detail that shows what changed.

If the provider lists `DataTypes` among its schema collections (`GetSchema("MetaDataCollections")`), the Optional test
`Advertised_data_types_match_the_catalog` also checks that each type's advertised `DataType` is the CLR type the provider
really returns and that every supported type is listed. Many providers do not offer `DataTypes` (Microsoft.Data.Sqlite,
DuckDB.NET and pengdows.flatfile do not), which is why the catalog, not the metadata, is the source of the types.

Catalogs exist for SQL Server (`SqlServerTypes`, 24 types; Microsoft.Data.SqlClient 2 to 5 and System.Data.SqlClient) and
for both SQLite clients (`SqliteTypes`). SQLite has no column types, only four storage classes and a declared name, so what
a provider returns is its own contract and each client has its own catalog: System.Data.SQLite follows the mapping its
`DataTypes` collection lists, and Microsoft.Data.Sqlite returns only `long`, `double`, `string` or `byte[]` from `GetValue`
and converts in the typed read, so a value there has an expected raw form and an expected typed form. SQLite keeps a REAL in
8 bytes (15 significant digits), so decimals beyond that are the database's limit, not the provider's, and the catalog keeps
decimals inside it.

`PostgreSqlTypes` covers Npgsql 4 to 8 and `OracleTypes` covers ODP.NET. Several of their values come from workarounds
pengdows.crud carries for these drivers: an `inet` with a netmask, an `interval` of months (PostgreSQL counts a month as
30 days in `extract(epoch ...)`, so that is the expectation), a `timestamptz` bound with a non-zero offset, a
`BINARY_DOUBLE` beyond 1e126, and a `LONG` read without the row's key in the select list. A value whose faithful CLR form
is the provider's own type (Npgsql's `NpgsqlInet`) is expected by its text (`ClrText`). If a provider closes the
connection after a rejected value, the finding says so and the next check uses a new connection. A fixture that
implements `ITypeFidelityFixture` and a derived class are all another provider needs; a database that needs
`FROM dual` or no statement terminator implements `ISqlDialectFixture` once on its fixture.

## Providers from a configuration file

A provider can be a JSON file in `providers/` instead of a test project. The runner generates a project from it (in
`tests/obj/providers`, which git ignores) and runs it with the others, so `run` picks it up by its `name` like any
provider. The file is the whole description of what is tested, so a driver's vendor and its users can run the same
file and compare matrices. A minimal file:

```jsonc
{
  "name": "Npgsql9Postgres17",
  "driver": { "package": "Npgsql", "version": "9.0.3", "factory": "Npgsql.NpgsqlFactory, Npgsql" },
  "target": {
    "kind": "docker",
    "image": "postgres:17-alpine",
    "port": 5432,
    "env": { "POSTGRES_USER": "root", "POSTGRES_PASSWORD": "test", "POSTGRES_DB": "conformance" },
    "tmpfs": "/var/lib/postgresql/data"
  },
  "connectionString": "Host=${host};Port=${port};Username=root;Password=test;Database=conformance",
  "dialect": { "renderAsText": "CAST({0} AS text)" },
  "types": { "query": "SELECT DISTINCT format_type(oid, NULL) FROM pg_type WHERE typtype = 'b'" }
}
```

| Key | What it says |
| --- | --- |
| `driver` | The NuGet `package` and `version` (exact or floating), or a `project` to build it from source (a sibling checkout, relative to the repository root); the `DbProviderFactory` type (its `Instance`, or a new one); and any other `packages` the driver needs |
| `target` | `docker`: an `image` started on a Docker-assigned host port for the `port` it listens on, with `env`, `command`, `tmpfs`, `privileged`, `files` put in the container, `waitForLog`, `sidecars` (containers it reaches by alias on a network of their own), `exec` shell commands run in it, and `setup` statements (on `setupConnectionString`); `exec` and `setup` are retried until `readySeconds` runs out, and every container is removed after the run. `cloud`: nothing is started and the connection string names environment variables. `file`: a database under `${tempdir}`, removed after the run. `singleWriter` (true by default for `file`): the database allows one writer at a time, so the test classes run one after another and a test that needs two writers at once is not applicable |
| `requires` | An `os` and environment variables (an opt-in switch, credentials) the provider needs; without them the runner reports it as `NOT RUN` with the reason, without building it |
| `processEnvironment` | Variables the runner sets when it starts the test process, for a driver that reads them before any code runs; `${output}` is the test assembly's folder |
| `connectionString` | With `${host}` and `${port}` (the container's), `${tempdir}` (removed after the run), `${instancedir}` (a folder of each test class's own, for a file database that allows one writer per folder), and `${NAME}` for any environment variable; the `ConnectionString` environment variable overrides it |
| `environment` | Environment variables set for the test process |
| `dialect` | Only where the database differs from SQL-92: `select` (`SELECT {0} FROM dual`), `terminator`, `parameterMarker`, `renderAsText`, `createTable`, `dropTable`, `binaryLiteral` (`0x{0}`, `hextoraw('{0}')`; SQL:1999 `X'{0}'` by default), `returning`, DML that returns rows with `{0}` for the statement (`{0} RETURNING Id`; unset, the RETURNING tests are not applicable), and `valueTypes`, the reader tests' column type per `DbType` (`"Binary": "bytea"`; an empty type leaves one out) |
| `types` | `query`: the server's own list of types; one the provider's `DataTypes` leaves out is a finding. `include`: types to test besides `DataTypes` (name, CLR type, column). `exclude`: types not to test, with the reason |

The fixture waits until the driver can open a connection, so a server that accepts its port before its logins is not a
problem. Where a database does not say how to write something, the defaults are SQL-92 double-quoted identifiers and
single-quoted strings, `;` between statements, and `@name` for parameters (the .NET convention, though SQL-92 writes
`:name`). The files in `providers/` are pengdows.crud's integration matrix: one per database it supports, with the driver
version, image and settings its testbed uses, so both projects test the same pairs. The opt-in ones (Aurora, SAP HANA,
InterBase, Snowflake) and Access (Windows only) report `NOT RUN` until their variables are set. A configured provider runs every test base with Required tests, plus the
generated type-fidelity tests: the DML and reader tests use tables of their own, created with the `createTable` and
`dropTable` statements and filled with each `ValueKind`'s value. A type whose table the server will not create is left
out of the supported types; a value a type cannot hold leaves its row out, so the tests reading it fail.

## Origin and license

This project is a derivative of [mysql-net/AdoNetApiTest](https://github.com/mysql-net/AdoNetApiTest), created by
Bradley Grainger and contributors (Brice Lambson, Shay Rojansky, Yoh Deadfall) under the MIT license. The upstream
commit history is preserved in this repository, and the upstream copyright notice is kept in [LICENSE](LICENSE)
as the MIT license requires. Changes since the fork are Copyright 2025-2026 Pengdows, under the same license.

[MIT](LICENSE)
