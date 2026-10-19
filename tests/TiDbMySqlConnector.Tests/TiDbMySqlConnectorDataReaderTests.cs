using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace TiDbMySqlConnector.Tests;

public sealed class TiDbMySqlConnectorDataReaderTests : DataReaderTestBase<TiDbMySqlConnectorSelectValueFixture>
{
	public TiDbMySqlConnectorDataReaderTests(TiDbMySqlConnectorSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
