using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Npgsql5.Tests;

public sealed class Npgsql5DmlTests : DmlTestBase<DmlFixture<Npgsql5DbFactoryFixture>>
{
	public Npgsql5DmlTests(DmlFixture<Npgsql5DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
