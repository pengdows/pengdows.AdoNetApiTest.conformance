using pengdows.AdoNetApiTest.conformance;

namespace Sqlite.Tests;

public sealed class SqliteConnectionTests : ConnectionTestBase<SqliteDbFactoryFixture>
{
	public SqliteConnectionTests(SqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
