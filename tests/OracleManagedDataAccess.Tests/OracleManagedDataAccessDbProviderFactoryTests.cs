using pengdows.AdoNetApiTest.conformance;

namespace OracleManagedDataAccess.Tests;

public sealed class OracleManagedDataAccessDbProviderFactoryTests : DbProviderFactoryTestBase<OracleManagedDataAccessDbFactoryFixture>
{
	public OracleManagedDataAccessDbProviderFactoryTests(OracleManagedDataAccessDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
