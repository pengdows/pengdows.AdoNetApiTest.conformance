using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlite5.Tests;

public sealed class MicrosoftSqlite5DmlTests : DmlTestBase<DmlFixture<MicrosoftSqlite5DbFactoryFixture>>
{
	public MicrosoftSqlite5DmlTests(DmlFixture<MicrosoftSqlite5DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
