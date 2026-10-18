using pengdows.AdoNetApiTest.conformance;

namespace DotConnectMySql.Tests;

public sealed class DotConnectMySqlConnectionTests : ConnectionTestBase<DotConnectMySqlDbFactoryFixture>
{
	public DotConnectMySqlConnectionTests(DotConnectMySqlDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
