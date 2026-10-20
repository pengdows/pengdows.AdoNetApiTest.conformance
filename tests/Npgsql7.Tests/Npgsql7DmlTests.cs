using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Npgsql7.Tests;

public sealed class Npgsql7DmlTests : DmlTestBase<DmlFixture<Npgsql7DbFactoryFixture>>
{
	public Npgsql7DmlTests(DmlFixture<Npgsql7DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
