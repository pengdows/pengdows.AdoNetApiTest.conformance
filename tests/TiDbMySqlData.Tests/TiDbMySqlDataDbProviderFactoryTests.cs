using pengdows.AdoNetApiTest.conformance;

namespace TiDbMySqlData.Tests;

public sealed class TiDbMySqlDataDbProviderFactoryTests : DbProviderFactoryTestBase<TiDbMySqlDataDbFactoryFixture>
{
	public TiDbMySqlDataDbProviderFactoryTests(TiDbMySqlDataDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
