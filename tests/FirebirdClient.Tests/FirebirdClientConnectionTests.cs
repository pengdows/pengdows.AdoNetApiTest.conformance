using pengdows.AdoNetApiTest.conformance;

namespace FirebirdClient.Tests;

public sealed class FirebirdClientConnectionTests : ConnectionTestBase<FirebirdClientDbFactoryFixture>
{
	public FirebirdClientConnectionTests(FirebirdClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
