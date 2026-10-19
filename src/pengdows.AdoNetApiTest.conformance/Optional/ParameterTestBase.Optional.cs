using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.IO;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class ParameterTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	/// <summary>
	/// DataSourceInformation must advertise a usable parameter-marker format. The
	/// shared parameter helper then maps a logical name to either the advertised
	/// named marker or the explicit positional marker.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbmetadatacolumnnames.parametermarkerformat,
	/// https://learn.microsoft.com/dotnet/api/system.data.common.dbmetadatacolumnnames.parametermarkerpattern,
	/// and https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbmetadatacolumnnames.parametermarkerformat", Because = "DataSourceInformation is an optional schema collection.")]
	public virtual void DataSourceInformation_advertises_parameter_marker_format()
	{
		using var connection = CreateOpenConnection();
		DataTable schema;
		try
		{
			schema = connection.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);
		}
		catch (Exception ex) when (ex is NotSupportedException or NotImplementedException or ArgumentException)
		{
			ConformanceOutcome.NotImplemented("The provider does not implement the DataSourceInformation schema collection.");
			return;
		}

		Assert.NotEmpty(schema.Rows);
		var markerFormat = schema.Rows[0][DbMetaDataColumnNames.ParameterMarkerFormat] as string;
		Assert.False(string.IsNullOrWhiteSpace(markerFormat));

		var marker = MakeParameterName(connection, "parameter");
		if (marker == "?")
			return;

		Assert.Contains("{0}", markerFormat, StringComparison.Ordinal);
		Assert.EndsWith("parameter", marker, StringComparison.OrdinalIgnoreCase);
	}
}
