using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class ConnectionTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
#if NET10_0_OR_GREATER
	/// <summary>
	/// The connection-level batch capability flag must agree with
	/// CreateBatch: unsupported connections must reject creation, while
	/// advertised support must produce a batch object.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.cancreatebatch
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.createbatch.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.cancreatebatch")]
	public virtual void CreateBatch_matches_capability()
	{
		using var connection = CreateOpenConnection();
		if (!connection.CanCreateBatch)
		{
			Assert.Throws<NotSupportedException>(() => connection.CreateBatch());
			throw Xunit.Sdk.SkipException.ForSkip("The provider does not implement connection-level batches.");
		}

		using var batch = connection.CreateBatch();
		Assert.NotNull(batch);
	}
#endif

	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.getschema", Because = "providers may report schema metadata as unsupported")]
	public virtual void GetSchema_parameterless_returns_proper_MetaDataCollections_or_throws()
	{
		using var connection = CreateOpenConnection();
		try
		{
			var table = connection.GetSchema();
			CheckMetaDataCollectionsSchema(table);
		}
		catch (NotSupportedException)
		{
			throw Xunit.Sdk.SkipException.ForSkip("The provider does not support GetSchema metadata collections.");
		}
	}

	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.getschema", Because = "providers may report schema metadata as unsupported")]
	public virtual void GetSchema_returns_proper_MetaDataCollections_or_throws()
	{
		using var connection = CreateOpenConnection();
		try
		{
			var table = connection.GetSchema("MetaDataCollections");
			CheckMetaDataCollectionsSchema(table);
		}
		catch (NotSupportedException)
		{
			throw Xunit.Sdk.SkipException.ForSkip("The provider does not support named GetSchema metadata collections.");
		}
	}

#if NET10_0_OR_GREATER
	/// <summary>
	/// GetSchemaAsync must preserve the metadata shape of the synchronous schema
	/// API for providers that expose schema collections.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.getschemaasync.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.getschemaasync")]
	public virtual async Task GetSchemaAsync_returns_metadata_when_supported()
	{
		using var connection = CreateOpenConnection();
		try
		{
			var table = await connection.GetSchemaAsync().ConfigureAwait(false);
			CheckMetaDataCollectionsSchema(table);
		}
		catch (NotSupportedException)
		{
			throw Xunit.Sdk.SkipException.ForSkip("The provider does not support asynchronous schema metadata.");
		}
	}
#endif

	/// <summary>
	/// DataSourceInformation is the provider-neutral source for parameter marker
	/// syntax. This diagnostic makes malformed or incomplete metadata visible;
	/// parameter construction must not silently guess a marker when the provider
	/// exposes unusable metadata.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbmetadatacolumnnames.parametermarkerformat
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.getschema.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbmetadatacolumnnames.parametermarkerformat", Because = "malformed DataSourceInformation is reported as a diagnostic")]
	public virtual void DataSourceInformation_parameter_marker_format_is_usable_when_advertised()
	{
		using var connection = CreateOpenConnection();
		DataTable table;
		try
		{
			table = connection.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);
		}
		catch (NotSupportedException)
		{
			throw Xunit.Sdk.SkipException.ForSkip("The provider does not expose DataSourceInformation.");
		}

		if (table.Rows.Count == 0 || !table.Columns.Contains(DbMetaDataColumnNames.ParameterMarkerFormat))
			throw Xunit.Sdk.SkipException.ForSkip("DataSourceInformation does not expose ParameterMarkerFormat.");

		var format = table.Rows[0][DbMetaDataColumnNames.ParameterMarkerFormat] as string;
		if (string.IsNullOrWhiteSpace(format))
			throw Xunit.Sdk.SkipException.ForSkip("DataSourceInformation exposes an empty ParameterMarkerFormat.");

		if (format != "?")
			Assert.Contains("{0}", format, StringComparison.Ordinal);
	}
}
