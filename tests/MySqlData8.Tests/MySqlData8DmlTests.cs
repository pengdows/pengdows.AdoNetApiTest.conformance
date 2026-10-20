using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MySqlData8.Tests;

public sealed class MySqlData8DmlTests : DmlTestBase<DmlFixture<MySqlData8DbFactoryFixture>>
{
	public MySqlData8DmlTests(DmlFixture<MySqlData8DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
