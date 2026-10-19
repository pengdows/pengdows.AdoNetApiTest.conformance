using pengdows.AdoNetApiTest.conformance;

namespace OracleManagedDataAccess.Tests;

public sealed class OracleManagedDataAccessDataTypesFidelityTests : DataTypesFidelityTestBase<OracleManagedDataAccessDbFactoryFixture>
{
	public OracleManagedDataAccessDataTypesFidelityTests(OracleManagedDataAccessDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
