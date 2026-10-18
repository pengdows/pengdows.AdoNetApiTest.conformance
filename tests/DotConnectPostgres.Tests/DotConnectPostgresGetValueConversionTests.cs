using pengdows.AdoNetApiTest.conformance;

namespace DotConnectPostgres.Tests;

public class DotConnectPostgresGetValueConversionTests : GetValueConversionTestBase<DotConnectPostgresSelectValueFixture>
{
	public DotConnectPostgresGetValueConversionTests(DotConnectPostgresSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
