using pengdows.AdoNetApiTest.conformance;

namespace TiDbMySqlData.Tests;

public sealed class TiDbMySqlDataDataReaderTests : DataReaderTestBase<TiDbMySqlDataSelectValueFixture>
{
	public TiDbMySqlDataDataReaderTests(TiDbMySqlDataSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
