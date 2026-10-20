using pengdows.AdoNetApiTest.conformance;

namespace FirebirdClient.Tests;

public sealed class FirebirdClientCommandTests : CommandTestBase<FirebirdClientDbFactoryFixture>
{
	public FirebirdClientCommandTests(FirebirdClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
