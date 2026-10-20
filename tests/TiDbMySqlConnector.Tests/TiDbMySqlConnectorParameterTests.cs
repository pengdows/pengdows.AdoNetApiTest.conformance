using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace TiDbMySqlConnector.Tests;

public sealed class TiDbMySqlConnectorParameterTests : ParameterTestBase<TiDbMySqlConnectorDbFactoryFixture>
{
	public TiDbMySqlConnectorParameterTests(TiDbMySqlConnectorDbFactoryFixture fixture)
		: base(fixture)
	{
	}

}
