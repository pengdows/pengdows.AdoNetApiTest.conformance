using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlite.Tests;

public sealed class MicrosoftSqliteDmlTests : DmlTestBase<DmlFixture<MicrosoftSqliteDbFactoryFixture>>
{
	public MicrosoftSqliteDmlTests(DmlFixture<MicrosoftSqliteDbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
