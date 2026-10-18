using pengdows.AdoNetApiTest.conformance;

namespace Sqlite.Tests;

public sealed class SqliteDataReaderTests : DataReaderTestBase<SqliteSelectValueFixture>
{
	public SqliteDataReaderTests(SqliteSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
