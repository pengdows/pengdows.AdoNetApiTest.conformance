using pengdows.AdoNetApiTest.conformance;

namespace Sqlite.Tests;

public class SqliteGetValueConversionTests : GetValueConversionTestBase<SqliteSelectValueFixture>
{
	public SqliteGetValueConversionTests(SqliteSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
