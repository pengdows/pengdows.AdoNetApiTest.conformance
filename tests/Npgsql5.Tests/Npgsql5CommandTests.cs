using pengdows.AdoNetApiTest.conformance;

namespace Npgsql5.Tests;

public sealed class Npgsql5CommandTests : CommandTestBase<Npgsql5DbFactoryFixture>
{
	public Npgsql5CommandTests(Npgsql5DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
