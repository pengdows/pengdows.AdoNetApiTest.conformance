using pengdows.AdoNetApiTest.conformance;

namespace DotConnectMySql.Tests;

public sealed class DotConnectConnectionStringBuilderTests : ConnectionStringTestBase<DotConnectMySqlDbFactoryFixture>
{
	public DotConnectConnectionStringBuilderTests(DotConnectMySqlDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
