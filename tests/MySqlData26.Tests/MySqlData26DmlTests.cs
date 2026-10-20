using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MySqlData26.Tests;

public sealed class MySqlData26DmlTests : DmlTestBase<DmlFixture<MySqlData26DbFactoryFixture>>
{
	public MySqlData26DmlTests(DmlFixture<MySqlData26DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
