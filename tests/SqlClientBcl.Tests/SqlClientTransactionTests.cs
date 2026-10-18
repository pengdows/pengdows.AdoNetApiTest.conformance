using pengdows.AdoNetApiTest.conformance;

namespace SqlClientBcl.Tests;

public sealed class SqlClientTransactionTests : TransactionTestBase<SqlClientDbFactoryFixture>
{
	public SqlClientTransactionTests(SqlClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
