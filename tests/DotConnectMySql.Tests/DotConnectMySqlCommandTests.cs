using pengdows.AdoNetApiTest.conformance;

namespace DotConnectMySql.Tests;

public sealed class DotConnectMySqlCommandTests : CommandTestBase<DotConnectMySqlDbFactoryFixture>
{
	public DotConnectMySqlCommandTests(DotConnectMySqlDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
