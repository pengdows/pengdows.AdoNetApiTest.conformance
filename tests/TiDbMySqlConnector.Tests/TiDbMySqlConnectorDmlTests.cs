using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace TiDbMySqlConnector.Tests;

public sealed class TiDbMySqlConnectorDmlTests : DmlTestBase<DmlFixture<TiDbMySqlConnectorDbFactoryFixture>>
{
	public TiDbMySqlConnectorDmlTests(DmlFixture<TiDbMySqlConnectorDbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
