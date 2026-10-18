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
project directly.

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

As necessary, override failing test methods to customize/skip them for your provider.

## Version History

See [Version History](VersionHistory.md).

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
Sqlite    FAIL     Required 146/154  |  Optional 18 implemented pass, 0 implemented fail, 6 not implemented  |  Common 275 differ
OVERALL: FAIL
Matrix: conformance-results/matrix.json, conformance-results/matrix.xml
```

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
| `not-run` | fail | No result was reported, so nothing is known. |

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

## License

[MIT](LICENSE)
