using pengdows.AdoNetApiTest.conformance;

namespace MySqlData26.Tests;

public sealed class MySqlData26CommandTests : CommandTestBase<MySqlData26DbFactoryFixture>
{
	public MySqlData26CommandTests(MySqlData26DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
