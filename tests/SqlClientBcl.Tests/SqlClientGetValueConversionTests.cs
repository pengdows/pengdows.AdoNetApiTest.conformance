using System;
using System.Data.SqlTypes;
using pengdows.AdoNetApiTest.conformance;

namespace SqlClientBcl.Tests;

public sealed class SqlClientGetValueConversionTests : GetValueConversionTestBase<SqlClientSelectValueFixture>
{
	public SqlClientGetValueConversionTests(SqlClientSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
