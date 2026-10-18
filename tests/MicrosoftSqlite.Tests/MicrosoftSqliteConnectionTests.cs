using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite.Tests;

public sealed class MicrosoftSqliteConnectionTests : ConnectionTestBase<MicrosoftSqliteDbFactoryFixture>
{
	public MicrosoftSqliteConnectionTests(MicrosoftSqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
