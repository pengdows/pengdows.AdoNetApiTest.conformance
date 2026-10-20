using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlite7.Tests;

public sealed class MicrosoftSqlite7DmlTests : DmlTestBase<DmlFixture<MicrosoftSqlite7DbFactoryFixture>>
{
	public MicrosoftSqlite7DmlTests(DmlFixture<MicrosoftSqlite7DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
