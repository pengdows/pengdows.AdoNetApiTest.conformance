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

## How to Build a Comparison Chart

Clone this repo.

Run the required database servers locally:

```
cd docker
docker compose up
```

Run the `pengdows.AdoNetApiTest.conformance.Runner` program:

1. In Visual Studio 2017, set it as the Start Up Project and press `Ctrl+F5` to launch, or
2. From a command prompt: `dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner/pengdows.AdoNetApiTest.conformance.Runner.csproj`

Note that some DB providers require the full .NET Framework 4.7; this program has only been tested on Windows (not with mono).

The program will run all providers' test suites, then open the results in your default browser.

## License

[MIT](LICENSE)
