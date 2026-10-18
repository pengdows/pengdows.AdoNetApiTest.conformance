## ADO.NET Specification Tests

This package provides base classes that let you test an ADO.NET provider against
the documented `System.Data.Common` API contract. It is not a SQL dialect
compatibility suite and does not assume that all providers expose the same
optional capabilities.

ADO.NET does not have one separate provider specification document, so the
authoritative contract is the .NET API documentation and the referenced ADO.NET
guidance. The complete coverage goal, classification rules, and remaining work
are documented in [CONTRACT-COVERAGE.md](CONTRACT-COVERAGE.md).

Many libraries (such as
[Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient),
[Npgsql](https://www.nuget.org/packages/Npgsql), and [MySqlConnector](https://www.nuget.org/packages/MySqlConnector))
exhibit similar behavior.

### Usage

Create a test project that uses [xunit](https://www.nuget.org/packages/xunit).

Write a class that implements `IDbFactoryFixture`:

```csharp
public class DbFactoryFixture : IDbFactoryFixture
{
	public DbFactoryFixture()
	{
		ConnectionString ="your test connection string";
	}

	public string ConnectionString { get; }
	public DbProviderFactory Factory => YourDbProviderFactory.Instance;
}
```

Parameterised SQL uses `MakeParameterName` to read
`DataSourceInformation.ParameterMarkerFormat` from the provider connection. This
keeps it aligned with the provider contract instead of assuming that the marker is
`@`; providers that do not advertise a named-parameter format use the positional
placeholder `?`.

Simple reader probes use the protected `SelectSql`, `SelectOneSql`, and related
query hooks on `DbFactoryTestBase`. A provider whose SQL dialect requires
`FROM dual`, a system table, or another dummy source should override those
hooks in its provider test project; the shared package does not bake that
dialect assumption into its contract tests.

Then write test classes that inherit from the classes in this package, e.g.,

```csharp
public sealed class ConnectionTests : ConnectionTestBase<DbFactoryFixture>
{
	public ConnectionTests(DbFactoryFixture fixture)
		: base(fixture)
	{
	}

	[Fact(Skip = "Override a method and provide a 'Skip' reason to opt out of a test.")]
	public override void Set_ConnectionString_throws_when_invalid()
	{
	}
}
```

Providers that support output parameters can additionally implement
`IOutputParameterFixture` and derive an `OutputParameterTestBase<TFixture>`.
That opt-in is intentional: the ADO.NET output-parameter contract is shared,
but the SQL command shape and native value configuration are provider-specific.

Providers that support changing the database on an open connection can likewise
implement `IChangeDatabaseFixture` and derive
`ChangeDatabaseTestBase<TFixture>`. The target database and the way the provider
reports the resulting database identity must come from the fixture.

Providers can test local transaction semantics by implementing
`ITransactionSemanticsFixture` and deriving `TransactionSemanticsTestBase<TFixture>`.
This keeps setup and verification SQL provider-specific while enforcing the
shared rollback-on-dispose contract.

Providers that support explicit `System.Transactions` enlistment can implement
`IEnlistmentFixture` and derive `EnlistmentTestBase<TFixture>` to verify that
committed enlisted work is actually committed.

Providers that support `InputOutput` or `ReturnValue` parameters can implement
`IParameterDirectionFixture` and derive `ParameterDirectionTestBase<TFixture>`.
Each direction is tested only when the fixture explicitly advertises support.

Providers can test an explicit isolation level by implementing
`IIsolationLevelFixture` and deriving `IsolationLevelTestBase<TFixture>`.
The ordinary transaction tests intentionally use the provider default.

On `net10.0`, providers that expose `DbProviderFactory.CreateDataSource` can
derive `DbDataSourceTestBase<TFixture>` to cover the modern data-source
connection, open, async-open, and command-creation contract.

### Required, Optional and Common tests

Every shared test belongs to exactly one group, declared by its attribute, listed under the matching folder, and
exposed as the xUnit trait `Category`:

| Group | Attribute | Folder | Meaning | Outcome |
| --- | --- | --- | --- | --- |
| Required | `[RequiredFact]` | `Required/` | The documented `System.Data.Common` contract requires this of every provider that exposes the API. | Failure is a hard conformance failure. |
| Optional | `[OptionalFact]` | `Optional/` | The behavior belongs to a capability a provider may decline. | Skipped when the fixture reports the capability unsupported; when it runs, failure is a hard failure. |
| Common | `[CommonFact]` | `Common/` | Interoperability behavior most providers share that the contract does not require, including the generated `GetValueConversionTestBase` matrix. | Advisory: reported (often as `[SOFT WARNING]`), never part of the hard result. |

Each attribute carries the documentation page it is based on (`Contract = "https://…"`), and `Because` records why a
test sits in its group when that is not obvious. Tests for one API area are partial classes, so
`CommandTestBase<TFixture>` is spread over `Required/`, `Optional/` and `Common/` files and provider projects derive
from it as before.

**The hard pass or failure** for a provider is Required plus Optional:

```text
dotnet test --filter "Category=Required|Category=Optional"
```

A provider conforms when that run has no failures. Common differences are investigated separately:

```text
dotnet test --filter "Category=Common"
```

When invoking the xUnit v3 in-process runner directly, use `-trait "Category=Required" -trait "Category=Optional"`
and `-trait "Category=Common"`.

Common cases still execute; their differences must not be converted into skips or hard failures. The only
conversion-matrix skips are for database types the fixture explicitly reports as unsupported. Required reader
state, ordinal, null, schema, and valid-value behavior is in `Required/DataReaderTestBase.Required.cs`.

`tests/Repository.Tests` enforces this: it fails if any shared test lacks a group attribute, sits in the wrong folder,
names no documentation page, or if a Required test reports a soft warning.
