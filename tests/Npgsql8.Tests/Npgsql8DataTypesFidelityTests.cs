using pengdows.AdoNetApiTest.conformance;

namespace Npgsql8.Tests;

public sealed class Npgsql8DataTypesFidelityTests : DataTypesFidelityTestBase<Npgsql8DbFactoryFixture>
{
	public Npgsql8DataTypesFidelityTests(Npgsql8DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
