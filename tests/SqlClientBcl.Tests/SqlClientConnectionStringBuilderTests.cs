using pengdows.AdoNetApiTest.conformance;
using SqlClientBcl.Tests;

namespace SqlClientBclBcl.Tests;

public sealed class SqlClientConnectionStringBuilderTests : ConnectionStringTestBase<SqlClientDbFactoryFixture>
{
	public SqlClientConnectionStringBuilderTests(SqlClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
