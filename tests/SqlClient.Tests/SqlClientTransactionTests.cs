using pengdows.AdoNetApiTest.conformance;

namespace SqlClient.Tests;

public sealed class SqlClientTransactionTests : TransactionTestBase<SqlClientDbFactoryFixture>
{
	public SqlClientTransactionTests(SqlClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
