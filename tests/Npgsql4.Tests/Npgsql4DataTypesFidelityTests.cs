using pengdows.AdoNetApiTest.conformance;

namespace Npgsql4.Tests;

public sealed class Npgsql4DataTypesFidelityTests : DataTypesFidelityTestBase<Npgsql4DbFactoryFixture>
{
	public Npgsql4DataTypesFidelityTests(Npgsql4DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
