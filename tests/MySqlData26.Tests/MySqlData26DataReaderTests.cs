using pengdows.AdoNetApiTest.conformance;

namespace MySqlData26.Tests;

public sealed class MySqlData26DataReaderTests : DataReaderTestBase<MySqlData26SelectValueFixture>
{
	public MySqlData26DataReaderTests(MySqlData26SelectValueFixture fixture)
		: base(fixture)
	{
	}
}
