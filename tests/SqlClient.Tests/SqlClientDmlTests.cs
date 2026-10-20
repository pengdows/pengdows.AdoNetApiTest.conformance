using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace SqlClient.Tests;

public sealed class SqlClientDmlTests : DmlTestBase<DmlFixture<SqlClientDbFactoryFixture>>
{
	public SqlClientDmlTests(DmlFixture<SqlClientDbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
