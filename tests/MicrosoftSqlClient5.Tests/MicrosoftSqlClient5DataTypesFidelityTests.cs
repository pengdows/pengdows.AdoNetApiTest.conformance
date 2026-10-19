using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlClient5.Tests;

public sealed class MicrosoftSqlClient5DataTypesFidelityTests : DataTypesFidelityTestBase<MicrosoftSqlClient5DbFactoryFixture>
{
	public MicrosoftSqlClient5DataTypesFidelityTests(MicrosoftSqlClient5DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
