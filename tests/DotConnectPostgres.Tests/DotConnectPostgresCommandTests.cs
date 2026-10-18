using pengdows.AdoNetApiTest.conformance;

namespace DotConnectPostgres.Tests;

public sealed class DotConnectPostgresCommandTests : CommandTestBase<DotConnectPostgresDbFactoryFixture>
{
	public DotConnectPostgresCommandTests(DotConnectPostgresDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
