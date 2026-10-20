using pengdows.AdoNetApiTest.conformance;

namespace TiDbMySqlData.Tests;

public sealed class TiDbMySqlDataCommandTests : CommandTestBase<TiDbMySqlDataDbFactoryFixture>
{
	public TiDbMySqlDataCommandTests(TiDbMySqlDataDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
