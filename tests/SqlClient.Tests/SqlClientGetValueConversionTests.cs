using pengdows.AdoNetApiTest.conformance;

namespace SqlClient.Tests;

public sealed class SqlClientGetValueConversionTests : GetValueConversionTestBase<SqlClientSelectValueFixture>
{
	public SqlClientGetValueConversionTests(SqlClientSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
