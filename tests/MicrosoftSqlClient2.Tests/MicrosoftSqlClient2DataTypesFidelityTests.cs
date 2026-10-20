using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlClient2.Tests;

public sealed class MicrosoftSqlClient2DataTypesFidelityTests : DataTypesFidelityTestBase<MicrosoftSqlClient2DbFactoryFixture>
{
	public MicrosoftSqlClient2DataTypesFidelityTests(MicrosoftSqlClient2DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
