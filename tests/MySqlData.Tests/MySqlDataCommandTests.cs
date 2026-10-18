using pengdows.AdoNetApiTest.conformance;

namespace MySqlData.Tests;

public sealed class MySqlDataCommandTests : CommandTestBase<MySqlDataDbFactoryFixture>
{
	public MySqlDataCommandTests(MySqlDataDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
