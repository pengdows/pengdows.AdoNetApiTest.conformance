using pengdows.AdoNetApiTest.conformance;

namespace MySqlData.Tests;

public sealed class MySqlDataParameterTests : ParameterTestBase<MySqlDataDbFactoryFixture>
{
	public MySqlDataParameterTests(MySqlDataDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
