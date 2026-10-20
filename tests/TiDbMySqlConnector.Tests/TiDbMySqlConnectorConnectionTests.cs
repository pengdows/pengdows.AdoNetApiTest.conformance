using pengdows.AdoNetApiTest.conformance;

namespace TiDbMySqlConnector.Tests;

public sealed class TiDbMySqlConnectorConnectionTests : ConnectionTestBase<TiDbMySqlConnectorDbFactoryFixture>
{
	public TiDbMySqlConnectorConnectionTests(TiDbMySqlConnectorDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
