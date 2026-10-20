using pengdows.AdoNetApiTest.conformance;

namespace FirebirdClient.Tests;

public sealed class FirebirdClientGetValueConversionTests : GetValueConversionTestBase<FirebirdClientSelectValueFixture>
{
	public FirebirdClientGetValueConversionTests(FirebirdClientSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
