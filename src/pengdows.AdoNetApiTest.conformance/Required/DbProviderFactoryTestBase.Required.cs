using System;
using System.Data.Common;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class DbProviderFactoryTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory")]
	public virtual void DbProviderFactory_CreateCommand_is_not_null() => Assert.NotNull(Fixture.Factory.CreateCommand());

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory")]
	public virtual void DbProviderFactory_CreateConnection_is_not_null() => Assert.NotNull(Fixture.Factory.CreateConnection());

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory")]
	public virtual void DbProviderFactory_CreateParameter_is_not_null() => Assert.NotNull(Fixture.Factory.CreateParameter());


}
