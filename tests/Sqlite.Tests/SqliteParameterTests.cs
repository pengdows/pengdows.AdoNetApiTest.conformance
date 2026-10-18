using pengdows.AdoNetApiTest.conformance;

namespace Sqlite.Tests;

public sealed class SqliteParameterTests : ParameterTestBase<SqliteDbFactoryFixture>
{
	public SqliteParameterTests(SqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
