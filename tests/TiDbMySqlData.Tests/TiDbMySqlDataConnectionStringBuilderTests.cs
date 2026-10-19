using pengdows.AdoNetApiTest.conformance;

namespace TiDbMySqlData.Tests;

public sealed class TiDbMySqlDataConnectionStringBuilderTests : ConnectionStringTestBase<TiDbMySqlDataDbFactoryFixture>
{
	public TiDbMySqlDataConnectionStringBuilderTests(TiDbMySqlDataDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
