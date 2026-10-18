using pengdows.AdoNetApiTest.conformance;

namespace MySqlData.Tests;

public sealed class MySqlDataConnectionStringBuilderTests : ConnectionStringTestBase<MySqlDataDbFactoryFixture>
{
	public MySqlDataConnectionStringBuilderTests(MySqlDataDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
