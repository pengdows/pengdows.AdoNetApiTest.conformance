using pengdows.AdoNetApiTest.conformance;

namespace DotConnectPostgres.Tests;

public sealed class DotConnectPostgresDbProviderFactoryTests : DbProviderFactoryTestBase<DotConnectPostgresDbFactoryFixture>
{
	public DotConnectPostgresDbProviderFactoryTests(DotConnectPostgresDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
