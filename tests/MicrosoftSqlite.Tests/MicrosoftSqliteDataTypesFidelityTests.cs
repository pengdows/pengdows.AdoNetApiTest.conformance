using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite.Tests;

public sealed class MicrosoftSqliteDataTypesFidelityTests : DataTypesFidelityTestBase<MicrosoftSqliteDbFactoryFixture>
{
	public MicrosoftSqliteDataTypesFidelityTests(MicrosoftSqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
