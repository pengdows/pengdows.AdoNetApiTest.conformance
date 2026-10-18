using pengdows.AdoNetApiTest.conformance;

namespace Sqlite.Tests;

public sealed class SqliteConnectionStringBuilderTests : ConnectionStringTestBase<SqliteDbFactoryFixture>
{
	public SqliteConnectionStringBuilderTests(SqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
