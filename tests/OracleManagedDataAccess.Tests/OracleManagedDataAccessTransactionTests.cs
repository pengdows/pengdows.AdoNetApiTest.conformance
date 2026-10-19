using pengdows.AdoNetApiTest.conformance;

namespace OracleManagedDataAccess.Tests;

public sealed class OracleManagedDataAccessTransactionTests : TransactionTestBase<OracleManagedDataAccessDbFactoryFixture>
{
	public OracleManagedDataAccessTransactionTests(OracleManagedDataAccessDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
