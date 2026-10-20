using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite6.Tests;

public sealed class MicrosoftSqlite6DataTypesFidelityTests : DataTypesFidelityTestBase<MicrosoftSqlite6DbFactoryFixture>
{
	public MicrosoftSqlite6DataTypesFidelityTests(MicrosoftSqlite6DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
