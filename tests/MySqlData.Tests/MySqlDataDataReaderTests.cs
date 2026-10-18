using pengdows.AdoNetApiTest.conformance;

namespace MySqlData.Tests;

public sealed class MySqlDataDataReaderTests : DataReaderTestBase<MySqlDataSelectValueFixture>
{
	public MySqlDataDataReaderTests(MySqlDataSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
