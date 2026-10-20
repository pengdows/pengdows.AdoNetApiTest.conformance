using pengdows.AdoNetApiTest.conformance;

namespace Npgsql5.Tests;

public sealed class Npgsql5DataTypesFidelityTests : DataTypesFidelityTestBase<Npgsql5DbFactoryFixture>
{
	public Npgsql5DataTypesFidelityTests(Npgsql5DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
