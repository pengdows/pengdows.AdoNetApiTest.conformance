using pengdows.AdoNetApiTest.conformance;

namespace FirebirdClient.Tests;

public sealed class FirebirdClientParameterTests : ParameterTestBase<FirebirdClientDbFactoryFixture>
{
	public FirebirdClientParameterTests(FirebirdClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
