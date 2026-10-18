using pengdows.AdoNetApiTest.conformance;

namespace MySqlConnector.Tests;

public sealed class MySqlConnectorConnectionStringBuilderTests : ConnectionStringTestBase<MySqlConnectorDbFactoryFixture>
{
	public MySqlConnectorConnectionStringBuilderTests(MySqlConnectorDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}

