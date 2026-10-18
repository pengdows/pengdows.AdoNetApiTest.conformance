using pengdows.AdoNetApiTest.conformance;

namespace SqlClientBcl.Tests;

public sealed class SqlClientDbProviderFactoryTests : DbProviderFactoryTestBase<SqlClientDbFactoryFixture>
{
	public SqlClientDbProviderFactoryTests(SqlClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
