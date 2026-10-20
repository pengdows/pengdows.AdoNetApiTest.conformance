using pengdows.AdoNetApiTest.conformance;

namespace OracleManagedDataAccess.Tests;

public sealed class OracleManagedDataAccessGetValueConversionTests : GetValueConversionTestBase<OracleManagedDataAccessSelectValueFixture>
{
	public OracleManagedDataAccessGetValueConversionTests(OracleManagedDataAccessSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
