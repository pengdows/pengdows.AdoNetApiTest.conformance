using pengdows.AdoNetApiTest.conformance;

namespace DuckDB.Tests;

public sealed class DuckDBDataTypesFidelityTests : DataTypesFidelityTestBase<DuckDBDbFactoryFixture>
{
	public DuckDBDataTypesFidelityTests(DuckDBDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
