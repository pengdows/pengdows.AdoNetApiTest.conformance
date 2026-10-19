using pengdows.AdoNetApiTest.conformance;

namespace OracleManagedDataAccess.Tests;

public sealed class OracleManagedDataAccessCommandTests : CommandTestBase<OracleManagedDataAccessDbFactoryFixture>
{
	public OracleManagedDataAccessCommandTests(OracleManagedDataAccessDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
