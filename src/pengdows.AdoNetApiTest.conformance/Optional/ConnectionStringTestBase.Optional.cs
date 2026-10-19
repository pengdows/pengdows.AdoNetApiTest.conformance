using System;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public partial class ConnectionStringTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnectionstringbuilder", Because = "Needs the optional DbProviderFactory.CreateConnectionStringBuilder.")]
	public void ConnectionString_is_empty_by_default()
	{
		var builder = Fixture.Factory.CreateConnectionStringBuilder();
		if (builder is null)
		{
			ConformanceOutcome.NotImplemented("The provider does not create a connection string builder.");
			return;
		}

		Assert.Equal(string.Empty, builder.ConnectionString);
	}
}
