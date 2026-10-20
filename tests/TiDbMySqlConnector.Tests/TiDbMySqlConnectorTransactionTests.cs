using pengdows.AdoNetApiTest.conformance;

namespace TiDbMySqlConnector.Tests;

public sealed class TiDbMySqlConnectorTransactionTests : TransactionTestBase<TiDbMySqlConnectorDbFactoryFixture>
{
	public TiDbMySqlConnectorTransactionTests(TiDbMySqlConnectorDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
