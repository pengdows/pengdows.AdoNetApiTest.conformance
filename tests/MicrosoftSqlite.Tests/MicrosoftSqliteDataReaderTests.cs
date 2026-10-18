using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite.Tests;

public sealed class MicrosoftSqliteDataReaderTests : DataReaderTestBase<MicrosoftSqliteSelectValueFixture>
{
	public MicrosoftSqliteDataReaderTests(MicrosoftSqliteSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
