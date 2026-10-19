using pengdows.AdoNetApiTest.conformance;

namespace TiDbMySqlConnector.Tests;

public sealed class TiDbMySqlConnectorDbProviderFactoryTests : DbProviderFactoryTestBase<TiDbMySqlConnectorDbFactoryFixture>
{
	public TiDbMySqlConnectorDbProviderFactoryTests(TiDbMySqlConnectorDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
