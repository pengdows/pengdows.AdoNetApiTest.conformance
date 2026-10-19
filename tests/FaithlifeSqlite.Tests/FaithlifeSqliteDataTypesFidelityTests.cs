using pengdows.AdoNetApiTest.conformance;

namespace FaithlifeSqlite.Tests;

public sealed class FaithlifeSqliteDataTypesFidelityTests : DataTypesFidelityTestBase<FaithlifeSqliteDbFactoryFixture>
{
	public FaithlifeSqliteDataTypesFidelityTests(FaithlifeSqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
