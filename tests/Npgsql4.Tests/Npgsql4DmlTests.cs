using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Npgsql4.Tests;

public sealed class Npgsql4DmlTests : DmlTestBase<DmlFixture<Npgsql4DbFactoryFixture>>
{
	public Npgsql4DmlTests(DmlFixture<Npgsql4DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
