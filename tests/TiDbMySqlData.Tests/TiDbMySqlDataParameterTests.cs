using pengdows.AdoNetApiTest.conformance;

namespace TiDbMySqlData.Tests;

public sealed class TiDbMySqlDataParameterTests : ParameterTestBase<TiDbMySqlDataDbFactoryFixture>
{
	public TiDbMySqlDataParameterTests(TiDbMySqlDataDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
