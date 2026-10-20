using pengdows.AdoNetApiTest.conformance;

namespace FlatFile.Tests;

public sealed class FlatFileDataTypesFidelityTests : DataTypesFidelityTestBase<FlatFileDbFactoryFixture>
{
	public FlatFileDataTypesFidelityTests(FlatFileDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
