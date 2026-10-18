using pengdows.AdoNetApiTest.conformance;

namespace SqlClient.Tests;

public sealed class SqlClientDbProviderFactoryTests : DbProviderFactoryTestBase<SqlClientDbFactoryFixture>
{
	public SqlClientDbProviderFactoryTests(SqlClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
