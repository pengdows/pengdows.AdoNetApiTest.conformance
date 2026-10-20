using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MySqlData.Tests;

public sealed class MySqlDataDmlTests : DmlTestBase<DmlFixture<MySqlDataDbFactoryFixture>>
{
	public MySqlDataDmlTests(DmlFixture<MySqlDataDbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
