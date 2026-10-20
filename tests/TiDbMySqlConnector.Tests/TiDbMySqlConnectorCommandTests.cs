using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace TiDbMySqlConnector.Tests;

public sealed class TiDbMySqlConnectorCommandTests : CommandTestBase<TiDbMySqlConnectorDbFactoryFixture>
{
	public TiDbMySqlConnectorCommandTests(TiDbMySqlConnectorDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
