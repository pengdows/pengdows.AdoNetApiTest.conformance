using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace OracleManagedDataAccess.Tests;

public sealed class OracleManagedDataAccessDmlTests : DmlTestBase<DmlFixture<OracleManagedDataAccessDbFactoryFixture>>
{
	public OracleManagedDataAccessDmlTests(DmlFixture<OracleManagedDataAccessDbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
