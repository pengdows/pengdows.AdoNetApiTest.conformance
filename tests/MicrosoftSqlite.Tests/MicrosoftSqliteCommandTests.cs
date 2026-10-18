using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite.Tests;

public sealed class MicrosoftSqliteCommandTests : CommandTestBase<MicrosoftSqliteDbFactoryFixture>
{
	public MicrosoftSqliteCommandTests(MicrosoftSqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
