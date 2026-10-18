using System;
using System.Data.Common;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class DbProviderFactoryTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	/// <summary>
	/// The factory must not be required to implement command builders. When the
	/// capability flag is available, it must agree with the factory result.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatecommandbuilder.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatecommandbuilder", Because = "providers that do not expose command builders are explicitly skipped")]
	public virtual void DbProviderFactory_CreateCommandBuilder_matches_capability()
	{
		DbCommandBuilder commandBuilder;
		try
		{
			commandBuilder = Fixture.Factory.CreateCommandBuilder();
		}
		catch (NotSupportedException)
		{
			commandBuilder = null;
		}
#if NETSTANDARD2_0
		// CanCreateCommandBuilder is not part of this target framework. The
		// factory result is the only available capability signal.
		if (commandBuilder is null)
		{
			ConformanceOutcome.NotImplemented("The provider does not expose the optional command-builder capability.");
			return;
		}
#else
		AssertOptionalFactoryObject(Fixture.Factory.CanCreateCommandBuilder, commandBuilder is not null, "command builder");
#endif
	}

	/// <summary>
	/// Retained under its historical name, but now checks the optional capability
	/// agreement rather than requiring a command builder.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.createcommandbuilder.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatecommandbuilder", Because = "NOW VALID OPTIONAL CAPABILITY-AGREEMENT TEST")]
	#if NETSTANDARD2_0
	public virtual void DbProviderFactory_CreateCommandBuilder_is_not_null()
		=> ConformanceOutcome.NotImplemented("CanCreateCommandBuilder is not available on this target framework.");
	#else
	public virtual void DbProviderFactory_CreateCommandBuilder_is_not_null()
		=> AssertOptionalFactoryObject(Fixture.Factory.CanCreateCommandBuilder, Fixture.Factory.CreateCommandBuilder() is not null, "command builder");
	#endif

	/// <summary>
	/// The factory must not be required to implement data adapters. When the
	/// capability flag is available, it must agree with the factory result.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatedataadapter.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatedataadapter", Because = "providers that do not expose data adapters are explicitly skipped")]
	public virtual void DbProviderFactory_CreateDataAdapter_matches_capability()
	{
		System.Data.Common.DbDataAdapter dataAdapter;
		try
		{
			dataAdapter = Fixture.Factory.CreateDataAdapter();
		}
		catch (NotSupportedException)
		{
			dataAdapter = null;
		}
#if NETSTANDARD2_0
		// CanCreateDataAdapter is not part of this target framework. The
		// factory result is the only available capability signal.
		if (dataAdapter is null)
		{
			ConformanceOutcome.NotImplemented("The provider does not expose the optional data-adapter capability.");
			return;
		}
#else
		AssertOptionalFactoryObject(Fixture.Factory.CanCreateDataAdapter, dataAdapter is not null, "data adapter");
#endif
	}

	/// <summary>
	/// Retained under its historical name, but now checks the optional capability
	/// agreement rather than requiring a data adapter.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.createdataadapter.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatedataadapter", Because = "NOW VALID OPTIONAL CAPABILITY-AGREEMENT TEST")]
	#if NETSTANDARD2_0
	public virtual void DbProviderFactory_CreateDataAdapter_is_not_null()
		=> ConformanceOutcome.NotImplemented("CanCreateDataAdapter is not available on this target framework.");
	#else
	public virtual void DbProviderFactory_CreateDataAdapter_is_not_null()
		=> AssertOptionalFactoryObject(Fixture.Factory.CanCreateDataAdapter, Fixture.Factory.CreateDataAdapter() is not null, "data adapter");
	#endif

#if NET10_0_OR_GREATER
	/// <summary>
	/// CreateDataSource is a modern optional factory API. Providers that implement
	/// it must return a usable data-source; providers that do not may explicitly
	/// report NotSupportedException.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.createdatasource.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.createdatasource", Because = "unimplemented data sources are skipped; an implemented data source must be usable")]
	public virtual void DbProviderFactory_CreateDataSource_is_usable_when_supported()
	{
		try
		{
			using var dataSource = Fixture.Factory.CreateDataSource(Fixture.ConnectionString);
			Assert.NotNull(dataSource);
		}
		catch (NotSupportedException)
		{
			ConformanceOutcome.NotImplemented("The provider does not expose the optional data-source capability.");
			return;
		}
	}

	/// <summary>
	/// CanCreateBatch must agree with whether CreateBatch provides a batch object.
	/// A provider that does not support batches may report that through
	/// NotSupportedException; it must not advertise support and then fail.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatebatch
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.createbatch.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatebatch")]
	public virtual void DbProviderFactory_CreateBatch_matches_capability()
	{
		DbBatch batch;
		try
		{
			batch = Fixture.Factory.CreateBatch();
		}
		catch (NotSupportedException)
		{
			Assert.False(Fixture.Factory.CanCreateBatch);
			ConformanceOutcome.NotImplemented("The provider does not implement factory-level batches.");
			return;
		}

		if (batch is null)
		{
			Assert.False(Fixture.Factory.CanCreateBatch, "The provider advertises batch support but returned null.");
			ConformanceOutcome.NotImplemented("The provider does not implement factory-level batches.");
			return;
		}

		Assert.True(Fixture.Factory.CanCreateBatch, "The provider returned a batch while advertising batch support as unavailable.");
		batch.Dispose();
	}

	/// <summary>
	/// A factory advertising batch support must create a batch command.
	/// Unsupported factories may report NotSupportedException.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.createbatchcommand.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.createbatchcommand")]
	public virtual void DbProviderFactory_CreateBatchCommand_matches_capability()
	{
		DbBatchCommand batchCommand;
		try
		{
			batchCommand = Fixture.Factory.CreateBatchCommand();
		}
		catch (NotSupportedException)
		{
			Assert.False(Fixture.Factory.CanCreateBatch);
			ConformanceOutcome.NotImplemented("The provider does not implement factory-level batch commands.");
			return;
		}

		if (batchCommand is null)
		{
			Assert.False(Fixture.Factory.CanCreateBatch, "The provider advertises batch-command support but returned null.");
			ConformanceOutcome.NotImplemented("The provider does not implement factory-level batch commands.");
			return;
		}

		Assert.True(Fixture.Factory.CanCreateBatch,
			"The provider returned a batch command while advertising batch support as unavailable.");
	}
#endif

	/// <summary>
	/// Retained under its historical name, but now checks the optional enumerator
	/// capability agreement rather than requiring a null result.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.createdatasourceenumerator.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatedatasourceenumerator", Because = "NOW VALID OPTIONAL CAPABILITY-AGREEMENT TEST")]
	public virtual void DbProviderFactory_CreateDataSourceEnumerator_is_null()
		=> AssertOptionalFactoryObject(Fixture.Factory.CanCreateDataSourceEnumerator, Fixture.Factory.CreateDataSourceEnumerator() is not null, "data-source enumerator");

	/// <summary>
	/// Retained under its historical name, but now checks the optional enumerator
	/// capability agreement rather than requiring the flag to be false.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatedatasourceenumerator.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatedatasourceenumerator", Because = "NOW VALID OPTIONAL CAPABILITY-AGREEMENT TEST")]
	public virtual void DbProviderFactory_CanCreateDataSourceEnumerator_is_false()
		=> AssertOptionalFactoryObject(Fixture.Factory.CanCreateDataSourceEnumerator, Fixture.Factory.CreateDataSourceEnumerator() is not null, "data-source enumerator");

	/// <summary>
	/// A factory advertising data-source enumeration must return an enumerator
	/// whose documented metadata operation completes with a data table. An
	/// unadvertised capability is explicitly skipped; an advertised capability
	/// that cannot be used is a hard failure.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatasourceenumerator.getdatasources.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatasourceenumerator.getdatasources")]
	public virtual void DbDataSourceEnumerator_GetDataSources_matches_capability()
	{
		var enumerator = Fixture.Factory.CreateDataSourceEnumerator();
		if (!AssertOptionalFactoryObject(Fixture.Factory.CanCreateDataSourceEnumerator, enumerator is not null, "data-source enumerator"))
		{
			return;
		}

		Assert.NotNull(enumerator.GetDataSources());
	}

	/// <summary>
	/// These original test names remain active so existing provider test classes do
	/// not lose their inherited surface. Their assertions now validate capability
	/// agreement rather than imposing a provider-specific feature set.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatedatasourceenumerator.
	/// </summary>
	#if NETSTANDARD2_0
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory", Because = "capability property is unavailable on netstandard2.0")]
	public virtual void DbProviderFactory_CanCreateCommandBuilder_is_true()
		=> ConformanceOutcome.NotImplemented("Not supported on this TargetFramework");

	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory", Because = "capability property is unavailable on netstandard2.0")]
	public virtual void DbProviderFactory_CanCreateDataAdapter_is_true()
		=> ConformanceOutcome.NotImplemented("Not supported on this TargetFramework");
	#else
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatecommandbuilder", Because = "NOW VALID OPTIONAL CAPABILITY-AGREEMENT TEST")]
	public virtual void DbProviderFactory_CanCreateCommandBuilder_is_true()
		=> AssertOptionalFactoryObject(Fixture.Factory.CanCreateCommandBuilder, Fixture.Factory.CreateCommandBuilder() is not null, "command builder");

	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatedataadapter", Because = "NOW VALID OPTIONAL CAPABILITY-AGREEMENT TEST")]
	public virtual void DbProviderFactory_CanCreateDataAdapter_is_true()
		=> AssertOptionalFactoryObject(Fixture.Factory.CanCreateDataAdapter, Fixture.Factory.CreateDataAdapter() is not null, "data adapter");
	#endif
}
