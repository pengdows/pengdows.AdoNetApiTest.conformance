using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlClient2.Tests;

public sealed class MicrosoftSqlClient2CommandTests : CommandTestBase<MicrosoftSqlClient2DbFactoryFixture>
{
	public MicrosoftSqlClient2CommandTests(MicrosoftSqlClient2DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
