using pengdows.AdoNetApiTest.conformance;

namespace MySqlData.Tests;

public sealed class MySqlDataTransactionTests : TransactionTestBase<MySqlDataDbFactoryFixture>
{
	public MySqlDataTransactionTests(MySqlDataDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
