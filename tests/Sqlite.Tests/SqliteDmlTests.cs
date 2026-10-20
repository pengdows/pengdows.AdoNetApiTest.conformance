using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Sqlite.Tests;

public sealed class SqliteDmlTests : DmlTestBase<DmlFixture<SqliteDbFactoryFixture>>
{
	public SqliteDmlTests(DmlFixture<SqliteDbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
