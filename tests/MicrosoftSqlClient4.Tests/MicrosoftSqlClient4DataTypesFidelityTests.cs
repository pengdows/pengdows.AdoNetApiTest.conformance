using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlClient4.Tests;

public sealed class MicrosoftSqlClient4DataTypesFidelityTests : DataTypesFidelityTestBase<MicrosoftSqlClient4DbFactoryFixture>
{
	public MicrosoftSqlClient4DataTypesFidelityTests(MicrosoftSqlClient4DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
