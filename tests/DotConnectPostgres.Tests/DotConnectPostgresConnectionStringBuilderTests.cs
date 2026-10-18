using pengdows.AdoNetApiTest.conformance;

namespace DotConnectPostgres.Tests;

public sealed class DotConnectConnectionStringBuilderTests : ConnectionStringTestBase<DotConnectPostgresDbFactoryFixture>
{
	public DotConnectConnectionStringBuilderTests(DotConnectPostgresDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
