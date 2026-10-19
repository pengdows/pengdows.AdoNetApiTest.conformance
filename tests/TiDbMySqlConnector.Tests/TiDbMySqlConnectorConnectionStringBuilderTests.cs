using pengdows.AdoNetApiTest.conformance;

namespace TiDbMySqlConnector.Tests;

public sealed class TiDbMySqlConnectorConnectionStringBuilderTests : ConnectionStringTestBase<TiDbMySqlConnectorDbFactoryFixture>
{
	public TiDbMySqlConnectorConnectionStringBuilderTests(TiDbMySqlConnectorDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}

