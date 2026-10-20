using pengdows.AdoNetApiTest.conformance;

namespace FirebirdClient.Tests;

public sealed class FirebirdClientDataReaderTests : DataReaderTestBase<FirebirdClientSelectValueFixture>
{
	public FirebirdClientDataReaderTests(FirebirdClientSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
