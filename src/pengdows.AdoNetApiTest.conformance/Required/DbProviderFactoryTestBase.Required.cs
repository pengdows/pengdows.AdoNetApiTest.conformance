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

#if NET10_0_OR_GREATER
	/// <summary>
	/// CanCreateBatch must agree with whether CreateBatch provides a batch object.
	/// A provider that does not support batches may report that through
	/// NotSupportedException; it must not advertise support and then fail.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatebatch
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.createbatch.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.cancreatebatch")]
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
			throw Xunit.Sdk.SkipException.ForSkip("The provider does not implement factory-level batches.");
		}

		if (batch is null)
		{
			Assert.False(Fixture.Factory.CanCreateBatch, "The provider advertises batch support but returned null.");
			throw Xunit.Sdk.SkipException.ForSkip("The provider does not implement factory-level batches.");
		}

		Assert.True(Fixture.Factory.CanCreateBatch, "The provider returned a batch while advertising batch support as unavailable.");
		batch.Dispose();
	}

	/// <summary>
	/// A factory advertising batch support must create a batch command.
	/// Unsupported factories may report NotSupportedException.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.createbatchcommand.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.createbatchcommand")]
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
			throw Xunit.Sdk.SkipException.ForSkip("The provider does not implement factory-level batch commands.");
		}

		if (batchCommand is null)
		{
			Assert.False(Fixture.Factory.CanCreateBatch, "The provider advertises batch-command support but returned null.");
			throw Xunit.Sdk.SkipException.ForSkip("The provider does not implement factory-level batch commands.");
		}

		Assert.True(Fixture.Factory.CanCreateBatch,
			"The provider returned a batch command while advertising batch support as unavailable.");
	}
#endif

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
		AssertOptionalFactoryObject(Fixture.Factory.CanCreateDataSourceEnumerator, enumerator is not null, "data-source enumerator");
	}
}
