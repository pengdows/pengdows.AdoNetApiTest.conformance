using pengdows.AdoNetApiTest.conformance;

namespace DotConnectMySql.Tests;

public sealed class DotConnectMySqlDbProviderFactoryTests : DbProviderFactoryTestBase<DotConnectMySqlDbFactoryFixture>
{
	public DotConnectMySqlDbProviderFactoryTests(DotConnectMySqlDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
