using pengdows.AdoNetApiTest.conformance;

namespace SqlClient.Tests;

public sealed class SqlClientDataTypesFidelityTests : DataTypesFidelityTestBase<SqlClientDbFactoryFixture>
{
	public SqlClientDataTypesFidelityTests(SqlClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
