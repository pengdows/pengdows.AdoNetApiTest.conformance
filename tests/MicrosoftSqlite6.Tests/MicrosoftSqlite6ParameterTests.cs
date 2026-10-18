using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite6.Tests;

public sealed class MicrosoftSqlite6ParameterTests : ParameterTestBase<MicrosoftSqlite6DbFactoryFixture>
{
	public MicrosoftSqlite6ParameterTests(MicrosoftSqlite6DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
