using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite.Tests;

public sealed class MicrosoftSqliteDbProviderFactoryTests : DbProviderFactoryTestBase<MicrosoftSqliteDbFactoryFixture>
{
	public MicrosoftSqliteDbProviderFactoryTests(MicrosoftSqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
