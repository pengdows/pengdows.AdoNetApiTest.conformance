using pengdows.AdoNetApiTest.conformance;

namespace Sqlite.Tests;

public sealed class SqliteDataTypesFidelityTests : DataTypesFidelityTestBase<SqliteDbFactoryFixture>
{
	public SqliteDataTypesFidelityTests(SqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
