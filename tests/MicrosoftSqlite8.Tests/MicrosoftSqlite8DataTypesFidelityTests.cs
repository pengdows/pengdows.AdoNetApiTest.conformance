using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite8.Tests;

public sealed class MicrosoftSqlite8DataTypesFidelityTests : DataTypesFidelityTestBase<MicrosoftSqlite8DbFactoryFixture>
{
	public MicrosoftSqlite8DataTypesFidelityTests(MicrosoftSqlite8DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
