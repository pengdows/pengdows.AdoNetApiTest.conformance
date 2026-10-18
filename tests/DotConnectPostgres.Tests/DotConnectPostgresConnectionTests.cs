using pengdows.AdoNetApiTest.conformance;

namespace DotConnectPostgres.Tests;

public sealed class DotConnectPostgresConnectionTests : ConnectionTestBase<DotConnectPostgresDbFactoryFixture>
{
	public DotConnectPostgresConnectionTests(DotConnectPostgresDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
