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
			Assert.ThrowsAny<NotSupportedException>(() => connection.CreateBatch());
			ConformanceOutcome.NotImplemented("The provider does not implement connection-level batches.");
			return;
		}

		using var batch = connection.CreateBatch();
		Assert.NotNull(batch);
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
		catch (Exception ex) when (ex is NotSupportedException or NotImplementedException or ArgumentException)
		{
			ConformanceOutcome.NotImplemented("The provider does not expose DataSourceInformation.");
			return;
		}

		if (table.Rows.Count == 0 || !table.Columns.Contains(DbMetaDataColumnNames.ParameterMarkerFormat))
		{
			ConformanceOutcome.NotImplemented("DataSourceInformation does not expose ParameterMarkerFormat.");
			return;
		}

		var format = table.Rows[0][DbMetaDataColumnNames.ParameterMarkerFormat] as string;
		if (string.IsNullOrWhiteSpace(format))
		{
			ConformanceOutcome.NotImplemented("DataSourceInformation exposes an empty ParameterMarkerFormat.");
			return;
		}

		if (format != "?")
			Assert.Contains("{0}", format, StringComparison.Ordinal);
	}

	/// <summary>
	/// Disposal raises the standard DbConnection Disposed event exactly once.
	/// See https://learn.microsoft.com/dotnet/api/system.componentmodel.component.disposed.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection", Because = "Design-time support is optional; a provider whose new commands are DesignTimeVisible opts in and must then support all of it.")]
	public virtual void Dispose_raises_Disposed()
	{
		if (!ImplementsDesignTime())
		{
			return;
		}

		var connection = CreateOpenConnection();
		var disposedCount = 0;
		connection.Disposed += (s, e) => disposedCount++;
		connection.Dispose();
		Assert.Equal(1, disposedCount);
	}

#if !NETSTANDARD2_0
	/// <summary>
	/// DisposeAsync follows the same component-disposal event contract as Dispose.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.disposeasync.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection", Because = "Design-time support is optional; a provider whose new commands are DesignTimeVisible opts in and must then support all of it.")]
	public virtual async Task DisposeAsync_raises_Disposed()
	{
		if (!ImplementsDesignTime())
		{
			return;
		}

		var connection = CreateOpenConnection();
		var disposedCount = 0;
		connection.Disposed += (s, e) => disposedCount++;
		await connection.DisposeAsync().ConfigureAwait(true);
		Assert.Equal(1, disposedCount);
	}
#endif
}
