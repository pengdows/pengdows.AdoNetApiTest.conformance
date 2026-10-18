using pengdows.AdoNetApiTest.conformance;

namespace MySqlConnector.Tests;

public sealed class MySqlConnectorTransactionTests : TransactionTestBase<MySqlConnectorDbFactoryFixture>
{
	public MySqlConnectorTransactionTests(MySqlConnectorDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
