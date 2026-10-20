using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlite6.Tests;

public sealed class MicrosoftSqlite6DmlTests : DmlTestBase<DmlFixture<MicrosoftSqlite6DbFactoryFixture>>
{
	public MicrosoftSqlite6DmlTests(DmlFixture<MicrosoftSqlite6DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
