using pengdows.AdoNetApiTest.conformance;

namespace SqlClientBcl.Tests;

public sealed class SqlClientParameterTests : ParameterTestBase<SqlClientDbFactoryFixture>
{
	public SqlClientParameterTests(SqlClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
