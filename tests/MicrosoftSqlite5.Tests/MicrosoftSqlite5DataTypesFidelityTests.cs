using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite5.Tests;

public sealed class MicrosoftSqlite5DataTypesFidelityTests : DataTypesFidelityTestBase<MicrosoftSqlite5DbFactoryFixture>
{
	public MicrosoftSqlite5DataTypesFidelityTests(MicrosoftSqlite5DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
