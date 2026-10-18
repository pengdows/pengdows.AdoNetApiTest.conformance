using pengdows.AdoNetApiTest.conformance;

namespace MySqlData8.Tests;

public sealed class MySqlData8ConnectionStringBuilderTests : ConnectionStringTestBase<MySqlData8DbFactoryFixture>
{
	public MySqlData8ConnectionStringBuilderTests(MySqlData8DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
