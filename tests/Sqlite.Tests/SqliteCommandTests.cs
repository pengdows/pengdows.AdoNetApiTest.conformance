using pengdows.AdoNetApiTest.conformance;

namespace Sqlite.Tests;

public sealed class SqliteCommandTests : CommandTestBase<SqliteDbFactoryFixture>
{
	public SqliteCommandTests(SqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
