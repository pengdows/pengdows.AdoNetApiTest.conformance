using pengdows.AdoNetApiTest.conformance;

namespace Npgsql6.Tests;

public sealed class Npgsql6DataTypesFidelityTests : DataTypesFidelityTestBase<Npgsql6DbFactoryFixture>
{
	public Npgsql6DataTypesFidelityTests(Npgsql6DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
