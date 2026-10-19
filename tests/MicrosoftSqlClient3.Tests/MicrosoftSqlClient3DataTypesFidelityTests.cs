using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlClient3.Tests;

public sealed class MicrosoftSqlClient3DataTypesFidelityTests : DataTypesFidelityTestBase<MicrosoftSqlClient3DbFactoryFixture>
{
	public MicrosoftSqlClient3DataTypesFidelityTests(MicrosoftSqlClient3DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
