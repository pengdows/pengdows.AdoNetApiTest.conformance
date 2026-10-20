using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Npgsql6.Tests;

public sealed class Npgsql6DmlTests : DmlTestBase<DmlFixture<Npgsql6DbFactoryFixture>>
{
	public Npgsql6DmlTests(DmlFixture<Npgsql6DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
