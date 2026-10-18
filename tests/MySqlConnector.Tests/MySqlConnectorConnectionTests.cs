using pengdows.AdoNetApiTest.conformance;

namespace MySqlConnector.Tests;

public sealed class MySqlConnectorConnectionTests : ConnectionTestBase<MySqlConnectorDbFactoryFixture>
{
	public MySqlConnectorConnectionTests(MySqlConnectorDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
