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
	public virtual void DbProviderFactory_CreateConnectionStringBuilder_is_not_null() => Assert.NotNull(Fixture.Factory.CreateConnectionStringBuilder());

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory")]
	public virtual void DbProviderFactory_CreateParameter_is_not_null() => Assert.NotNull(Fixture.Factory.CreateParameter());


	/// <summary>
	/// The capability flag must agree with whether the factory creates a data-source
	/// enumerator. Providers that expose this optional API remain visible in the
	/// comparison output.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatedatasourceenumerator.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory")]
	public virtual void DbProviderFactory_CanCreateDataSourceEnumerator_is_accurate()
	{
		var enumerator = Fixture.Factory.CreateDataSourceEnumerator();
		AssertCapabilityMatches(Fixture.Factory.CanCreateDataSourceEnumerator, enumerator is not null, "data-source enumerator");
	}
}
