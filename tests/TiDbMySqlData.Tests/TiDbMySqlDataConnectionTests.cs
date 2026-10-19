using pengdows.AdoNetApiTest.conformance;

namespace TiDbMySqlData.Tests;

public sealed class TiDbMySqlDataConnectionTests : ConnectionTestBase<TiDbMySqlDataDbFactoryFixture>
{
	public TiDbMySqlDataConnectionTests(TiDbMySqlDataDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
