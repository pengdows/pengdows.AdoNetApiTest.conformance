using System;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public partial class ConnectionStringTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnectionstringbuilder", Because = "No page says setting ConnectionString to null yields an empty string.")]
	public virtual void ConnectionString_set_to_null_is_empty_string()
	{
		var builder = CreateConnectionStringBuilder();
		builder.ConnectionString = null;
		Assert.Empty(builder.ConnectionString);
	}
}
