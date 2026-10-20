using pengdows.AdoNetApiTest.conformance;

namespace TiDbMySqlData.Tests;

public sealed class TiDbMySqlDataTransactionTests : TransactionTestBase<TiDbMySqlDataDbFactoryFixture>
{
	public TiDbMySqlDataTransactionTests(TiDbMySqlDataDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
