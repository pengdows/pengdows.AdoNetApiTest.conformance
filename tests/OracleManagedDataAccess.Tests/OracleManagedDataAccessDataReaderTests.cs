using pengdows.AdoNetApiTest.conformance;

namespace OracleManagedDataAccess.Tests;

public sealed class OracleManagedDataAccessDataReaderTests : DataReaderTestBase<OracleManagedDataAccessSelectValueFixture>
{
	public OracleManagedDataAccessDataReaderTests(OracleManagedDataAccessSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
