using pengdows.AdoNetApiTest.conformance;

namespace FirebirdClient.Tests;

public sealed class FirebirdClientDbProviderFactoryTests : DbProviderFactoryTestBase<FirebirdClientDbFactoryFixture>
{
	public FirebirdClientDbProviderFactoryTests(FirebirdClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
