using pengdows.AdoNetApiTest.conformance;

namespace SqlClientBcl.Tests;

public sealed class SqlClientCommandTests : CommandTestBase<SqlClientDbFactoryFixture>
{
	public SqlClientCommandTests(SqlClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
