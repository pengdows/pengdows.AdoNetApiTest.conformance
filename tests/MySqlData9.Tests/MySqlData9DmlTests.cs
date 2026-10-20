using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MySqlData9.Tests;

public sealed class MySqlData9DmlTests : DmlTestBase<DmlFixture<MySqlData9DbFactoryFixture>>
{
	public MySqlData9DmlTests(DmlFixture<MySqlData9DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
