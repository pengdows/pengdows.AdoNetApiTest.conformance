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

Create one public derived class from each `XTestBase<T>` test base class. Add a public
constructor that takes an instance of your `IDbFactoryFixture` implementation and passes it
to the base class.

Nothing is skipped: the shared tests report `not-applicable` or `not-implemented` themselves, based on what your
fixture declares. If a Required test fails, the provider does not meet the documented contract.

## Version History

See the [changelog](CHANGELOG.md). The original project's release notes are in
[UPSTREAM-VERSION-HISTORY.md](UPSTREAM-VERSION-HISTORY.md).

## Running the conformance check

You need the .NET SDK pinned in `global.json`. Providers that talk to a server (SQL Server, PostgreSQL, MySQL) need that
server running first; `docker/docker-compose.yml` starts them. SQLite and the other embedded providers need nothing.

From the repository root:

```text
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- run Sqlite
```

`run` builds and runs each named provider's test project (every provider if you name none), then prints one line per
provider and an overall verdict:

```text
Sqlite    FAIL     Required 96/104 (5 failed, 3 waived) + 8 Required not covered  |  Optional 24 implemented pass, 0 implemented fail, 7 not implemented  |  Common 276 differ  |  Types 499 exact, 0 lossy, 0 wrong type, 0 throws
OVERALL: FAIL
Matrix: conformance-results/matrix.json, conformance-results/matrix.xml
```

`Required 146/156` is passed over total. The parenthesis says what became of the rest: `failed` tests ran and failed; `waived` tests failed but carry a waiver (see `deviation`);
`not run` tests belong to an area the provider covers but reported no result (a crashed test host, a result file from
an older suite). `+ N Required not covered` counts Required tests in an area the provider has no tests for at all (such as
`Dml` without a fixture); they are not in the total and do not fail the provider, but they are not passes either.
`passed + failed + not run + waived` always equals the total, and the same three figures are in `matrix.json` and `matrix.xml`.

The exit code is `0` for PASS, `1` for FAIL and `2` for a usage error, so it works in a script or CI job.

**A provider passes** when it has no failed Required test and no failed Optional test. Nothing is skipped:

| Outcome | Counts as | Meaning |
| --- | --- | --- |
| `pass` | pass | The test ran and passed. |
| `fail` | fail | A Required test failed: the provider does not meet the documented contract. |
| `implemented-pass` | pass | The provider implements this optional capability and the test passed. |
| `implemented-fail` | fail | The provider implements this optional capability and the test failed. |
| `not-implemented` | pass | The provider does not implement this optional capability. |
| `not-applicable` | pass | The check does not apply, such as a data type the database does not have. |
| `different` | advisory | The provider departs from a common expectation the contract does not require. Never fails the provider. |
| `deviation` | advisory | A Required test failed, but the test carries a **waiver**: the documentation states the behavior, yet a deviation cannot lose or alter data or change what a statement returns (for example a property only a designer reads). It is reported with its reason, counted in `waived`, and never fails the provider. |
| `not-run` | fail | No result was reported, so nothing is known. For a **Common** test this is advisory instead (shown as `N not run` on the Common part of the console line): it cannot hide a Required result. |
| `not-covered` | advisory | The provider's test project has no tests for this area at all, so nothing is known about it. Not counted against the provider; the console line says `Coverage N of M areas`. |

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
decimals inside it. A fixture that implements `ITypeFidelityFixture` and a derived class are all another provider needs.

## Origin and license

This project is a derivative of [mysql-net/AdoNetApiTest](https://github.com/mysql-net/AdoNetApiTest), created by
Bradley Grainger and contributors (Brice Lambson, Shay Rojansky, Yoh Deadfall) under the MIT license. The upstream
commit history is preserved in this repository, and the upstream copyright notice is kept in [LICENSE](LICENSE)
as the MIT license requires. Changes since the fork are Copyright 2025-2026 Pengdows, under the same license.

[MIT](LICENSE)
