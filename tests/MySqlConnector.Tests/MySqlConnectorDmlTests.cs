using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MySqlConnector.Tests;

public sealed class MySqlConnectorDmlTests : DmlTestBase<DmlFixture<MySqlConnectorDbFactoryFixture>>
{
	public MySqlConnectorDmlTests(DmlFixture<MySqlConnectorDbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
