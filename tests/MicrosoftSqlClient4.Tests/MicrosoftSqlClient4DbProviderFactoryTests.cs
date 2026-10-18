using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlClient4.Tests;

public sealed class MicrosoftSqlClient4DbProviderFactoryTests : DbProviderFactoryTestBase<MicrosoftSqlClient4DbFactoryFixture>
{
	public MicrosoftSqlClient4DbProviderFactoryTests(MicrosoftSqlClient4DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
