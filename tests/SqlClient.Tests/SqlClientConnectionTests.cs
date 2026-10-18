using pengdows.AdoNetApiTest.conformance;

namespace SqlClient.Tests;

public sealed class SqlClientConnectionTests : ConnectionTestBase<SqlClientDbFactoryFixture>
{
	public SqlClientConnectionTests(SqlClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
