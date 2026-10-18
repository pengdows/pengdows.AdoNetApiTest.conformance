using pengdows.AdoNetApiTest.conformance;

namespace SqlClient.Tests;

public sealed class SqlClientCommandTests : CommandTestBase<SqlClientDbFactoryFixture>
{
	public SqlClientCommandTests(SqlClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
