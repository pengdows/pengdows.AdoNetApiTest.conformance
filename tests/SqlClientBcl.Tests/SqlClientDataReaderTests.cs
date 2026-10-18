using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace SqlClientBcl.Tests;

public sealed class SqlClientDataReaderTests : DataReaderTestBase<SqlClientSelectValueFixture>
{
	public SqlClientDataReaderTests(SqlClientSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
