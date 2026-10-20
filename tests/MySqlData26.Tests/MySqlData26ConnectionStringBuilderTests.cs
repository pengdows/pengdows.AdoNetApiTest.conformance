using pengdows.AdoNetApiTest.conformance;

namespace MySqlData26.Tests;

public sealed class MySqlData26ConnectionStringBuilderTests : ConnectionStringTestBase<MySqlData26DbFactoryFixture>
{
	public MySqlData26ConnectionStringBuilderTests(MySqlData26DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
