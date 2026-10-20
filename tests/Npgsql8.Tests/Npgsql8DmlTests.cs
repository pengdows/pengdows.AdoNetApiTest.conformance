using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Npgsql8.Tests;

public sealed class Npgsql8DmlTests : DmlTestBase<DmlFixture<Npgsql8DbFactoryFixture>>
{
	public Npgsql8DmlTests(DmlFixture<Npgsql8DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
