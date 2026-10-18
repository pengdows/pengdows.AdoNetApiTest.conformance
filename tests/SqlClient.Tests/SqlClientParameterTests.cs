using pengdows.AdoNetApiTest.conformance;

namespace SqlClient.Tests;

public sealed class SqlClientParameterTests : ParameterTestBase<SqlClientDbFactoryFixture>
{
	public SqlClientParameterTests(SqlClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
