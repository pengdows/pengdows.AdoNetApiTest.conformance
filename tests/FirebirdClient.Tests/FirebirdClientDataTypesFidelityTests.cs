using pengdows.AdoNetApiTest.conformance;

namespace FirebirdClient.Tests;

public sealed class FirebirdClientDataTypesFidelityTests : DataTypesFidelityTestBase<FirebirdClientDbFactoryFixture>
{
	public FirebirdClientDataTypesFidelityTests(FirebirdClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
