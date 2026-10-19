using pengdows.AdoNetApiTest.conformance;

namespace OracleManagedDataAccess.Tests;

public sealed class OracleManagedDataAccessConnectionTests : ConnectionTestBase<OracleManagedDataAccessDbFactoryFixture>
{
	public OracleManagedDataAccessConnectionTests(OracleManagedDataAccessDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
