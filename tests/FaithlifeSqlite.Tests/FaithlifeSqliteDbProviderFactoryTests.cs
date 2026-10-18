using pengdows.AdoNetApiTest.conformance;

namespace FaithlifeSqlite.Tests;

public sealed class FaithlifeSqliteDbProviderFactoryTests : DbProviderFactoryTestBase<FaithlifeSqliteDbFactoryFixture>
{
	public FaithlifeSqliteDbProviderFactoryTests(FaithlifeSqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
