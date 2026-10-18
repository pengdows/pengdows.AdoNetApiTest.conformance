using pengdows.AdoNetApiTest.conformance;

namespace MySqlConnector.Tests;

public sealed class MySqlConnectorDbProviderFactoryTests : DbProviderFactoryTestBase<MySqlConnectorDbFactoryFixture>
{
	public MySqlConnectorDbProviderFactoryTests(MySqlConnectorDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
