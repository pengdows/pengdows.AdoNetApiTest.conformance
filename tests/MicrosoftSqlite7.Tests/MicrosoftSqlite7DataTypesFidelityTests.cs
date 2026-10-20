using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite7.Tests;

public sealed class MicrosoftSqlite7DataTypesFidelityTests : DataTypesFidelityTestBase<MicrosoftSqlite7DbFactoryFixture>
{
	public MicrosoftSqlite7DataTypesFidelityTests(MicrosoftSqlite7DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
