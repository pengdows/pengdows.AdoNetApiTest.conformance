using pengdows.AdoNetApiTest.conformance;

namespace Npgsql7.Tests;

public sealed class Npgsql7DataTypesFidelityTests : DataTypesFidelityTestBase<Npgsql7DbFactoryFixture>
{
	public Npgsql7DataTypesFidelityTests(Npgsql7DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
