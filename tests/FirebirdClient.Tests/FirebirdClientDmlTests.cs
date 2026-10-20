using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace FirebirdClient.Tests;

public sealed class FirebirdClientDmlTests : DmlTestBase<DmlFixture<FirebirdClientDbFactoryFixture>>
{
	public FirebirdClientDmlTests(DmlFixture<FirebirdClientDbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
