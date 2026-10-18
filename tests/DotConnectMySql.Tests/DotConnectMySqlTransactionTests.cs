using pengdows.AdoNetApiTest.conformance;

namespace DotConnectMySql.Tests;

public sealed class DotConnectMySqlTransactionTests : TransactionTestBase<DotConnectMySqlDbFactoryFixture>
{
	public DotConnectMySqlTransactionTests(DotConnectMySqlDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
