using pengdows.AdoNetApiTest.conformance;

namespace OracleManagedDataAccess.Tests;

public sealed class OracleManagedDataAccessConnectionStringBuilderTests : ConnectionStringTestBase<OracleManagedDataAccessDbFactoryFixture>
{
	public OracleManagedDataAccessConnectionStringBuilderTests(OracleManagedDataAccessDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
