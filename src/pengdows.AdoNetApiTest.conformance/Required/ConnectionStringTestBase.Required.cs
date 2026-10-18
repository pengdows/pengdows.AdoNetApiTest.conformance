using System;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public partial class ConnectionStringTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnectionstringbuilder")]
	public void ConnectionString_is_empty_by_default()
	{
		var builder = CreateConnectionStringBuilder();
		Assert.Equal(string.Empty, builder.ConnectionString);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnectionstringbuilder")]
	public virtual void ConnectionString_set_to_null_is_empty_string()
	{
		var builder = CreateConnectionStringBuilder();
		builder.ConnectionString = null;
		Assert.Empty(builder.ConnectionString);
	}
}
