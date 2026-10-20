using pengdows.AdoNetApiTest.conformance;

namespace FirebirdClient.Tests;

public sealed class FirebirdClientConnectionStringBuilderTests : ConnectionStringTestBase<FirebirdClientDbFactoryFixture>
{
	public FirebirdClientConnectionStringBuilderTests(FirebirdClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
