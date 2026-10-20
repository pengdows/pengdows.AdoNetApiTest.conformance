using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace TiDbMySqlData.Tests;

public sealed class TiDbMySqlDataDmlTests : DmlTestBase<DmlFixture<TiDbMySqlDataDbFactoryFixture>>
{
	public TiDbMySqlDataDmlTests(DmlFixture<TiDbMySqlDataDbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
