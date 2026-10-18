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
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void Parameter_default_Direction_is_input()
	{
		Assert.Equal(ParameterDirection.Input, Fixture.Factory.CreateParameter().Direction);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.parametername", Because = "the documented default is an empty string")]
	public virtual void Parameter_default_ParameterName_is_empty_string()
	{
		Assert.Equal(string.Empty, Fixture.Factory.CreateParameter().ParameterName);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void Parameter_default_Precision_is_zero()
	{
		Assert.Equal((byte) 0, Fixture.Factory.CreateParameter().Precision);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void Parameter_default_Scale_is_zero()
	{
		Assert.Equal((byte) 0, Fixture.Factory.CreateParameter().Scale);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void Parameter_default_Size_is_zero()
	{
		Assert.Equal(0, Fixture.Factory.CreateParameter().Size);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void Parameter_default_SourceVersion_is_Current()
	{
		Assert.Equal(DataRowVersion.Current, Fixture.Factory.CreateParameter().SourceVersion);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void Parameter_default_Value_is_null()
	{
		Assert.Null(Fixture.Factory.CreateParameter().Value);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void Parameter_Value_can_be_set_to_null()
	{
		var parameter = Fixture.Factory.CreateParameter();
		parameter.Value = null;
		Assert.True(parameter.Value is null or DBNull);
	}

	/// <summary>
	/// The DbParameter properties describe how a provider binds a value and must
	/// preserve values assigned by the consumer. This test covers the universal
	/// input-parameter contract; output and input/output directions are separate
	/// provider capabilities and must not be imposed on providers such as SQLite.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void Parameter_properties_round_trip()
	{
		var parameter = Fixture.Factory.CreateParameter();

		parameter.DbType = DbType.Int32;
		parameter.Direction = ParameterDirection.Input;
		parameter.IsNullable = true;
		parameter.ParameterName = "value";
		parameter.Size = 32;
		parameter.SourceColumn = "source";
		parameter.SourceColumnNullMapping = true;
		parameter.SourceVersion = DataRowVersion.Original;
		parameter.Value = 42;

		Assert.Equal(DbType.Int32, parameter.DbType);
		Assert.Equal(ParameterDirection.Input, parameter.Direction);
		Assert.True(parameter.IsNullable);
		Assert.Equal("value", parameter.ParameterName);
		Assert.Equal(32, parameter.Size);
		Assert.Equal("source", parameter.SourceColumn);
		Assert.True(parameter.SourceColumnNullMapping);
		Assert.Equal(DataRowVersion.Original, parameter.SourceVersion);
		Assert.Equal(42, parameter.Value);
	}

	/// <summary>
	/// DataSourceInformation must advertise a usable parameter-marker format. The
	/// shared parameter helper then maps a logical name to either the advertised
	/// named marker or the explicit positional marker.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbmetadatacolumnnames.parametermarkerformat,
	/// https://learn.microsoft.com/dotnet/api/system.data.common.dbmetadatacolumnnames.parametermarkerpattern,
	/// and https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbmetadatacolumnnames.parametermarkerformat")]
	public virtual void DataSourceInformation_advertises_parameter_marker_format()
	{
		using var connection = CreateOpenConnection();
		var schema = connection.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);

		Assert.NotEmpty(schema.Rows);
		var markerFormat = schema.Rows[0][DbMetaDataColumnNames.ParameterMarkerFormat] as string;
		Assert.False(string.IsNullOrWhiteSpace(markerFormat));

		var marker = MakeParameterName(connection, "parameter");
		if (marker == "?")
			return;

		Assert.Contains("{0}", markerFormat, StringComparison.Ordinal);
		Assert.EndsWith("parameter", marker, StringComparison.OrdinalIgnoreCase);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterName_can_be_set_to_null()
	{
		Fixture.Factory.CreateParameter().ParameterName = null;
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterName_default_is_empty_string()
	{
		Assert.Equal("", Fixture.Factory.CreateParameter().ParameterName);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterCollection_Contains_object_returns_false_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.False(command.Parameters.Contains(default(object)));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterCollection_Contains_string_returns_false_for_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.False(command.Parameters.Contains(default(string)));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterCollection_RemoveAt_negative_one_throws()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.Throws<ArgumentOutOfRangeException>(() => command.Parameters.RemoveAt(-1));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterCollection_RemoveAt_zero_throws_when_empty()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.Throws<ArgumentOutOfRangeException>(() => command.Parameters.RemoveAt(0));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterCollection_Insert_throws_for_negative_one()
	{
		using var command = Fixture.Factory.CreateCommand();
		var parameter = command.CreateParameter();
		Assert.Throws<ArgumentOutOfRangeException>(() => command.Parameters.Insert(-1, parameter));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterCollection_Insert_succeeds()
	{
		using var command = Fixture.Factory.CreateCommand();
		var parameter = command.CreateParameter();
		command.Parameters.Insert(0, parameter);
		Assert.Same(parameter, Assert.Single(command.Parameters));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterCollection_Insert_throws_for_one()
	{
		using var command = Fixture.Factory.CreateCommand();
		var parameter = command.CreateParameter();
		Assert.Throws<ArgumentOutOfRangeException>(() => command.Parameters.Insert(1, parameter));
	}

	/// <summary>
	/// AddRange must add each supplied parameter in order, and CopyTo must expose
	/// the same collection order through the standard collection API.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection.addrange
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection.copyto.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection")]
	public virtual void ParameterCollection_AddRange_and_CopyTo_preserve_order()
	{
		using var command = Fixture.Factory.CreateCommand();
		var first = command.CreateParameter();
		first.ParameterName = "first";
		var second = command.CreateParameter();
		second.ParameterName = "second";

		command.Parameters.AddRange(new[] { first, second });

		Assert.Equal(2, command.Parameters.Count);
		Assert.Same(first, command.Parameters[0]);
		Assert.Same(second, command.Parameters[1]);

		var copy = Array.CreateInstance(first.GetType(), 2);
		command.Parameters.CopyTo(copy, 0);
		Assert.Same(first, copy.GetValue(0));
		Assert.Same(second, copy.GetValue(1));
	}

	/// <summary>
	/// The parameter collection implements the collection enumeration contract
	/// and enumerates its parameters in collection order.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection.getenumerator.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection.getenumerator")]
	public virtual void ParameterCollection_GetEnumerator_preserves_order()
	{
		using var command = Fixture.Factory.CreateCommand();
		var first = command.CreateParameter();
		var second = command.CreateParameter();
		command.Parameters.Add(first);
		command.Parameters.Add(second);

		var enumerator = command.Parameters.GetEnumerator();
		Assert.True(enumerator.MoveNext());
		Assert.Same(first, enumerator.Current);
		Assert.True(enumerator.MoveNext());
		Assert.Same(second, enumerator.Current);
		Assert.False(enumerator.MoveNext());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterCollection_Remove_succeeds()
	{
		using var command = Fixture.Factory.CreateCommand();
		var parameter = command.CreateParameter();
		command.Parameters.Add(parameter);
		command.Parameters.Remove(parameter);
		Assert.Empty(command.Parameters);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterCollection_RemoveAt_succeeds()
	{
		using var command = Fixture.Factory.CreateCommand();
		var parameter = command.CreateParameter();
		command.Parameters.Add(parameter);
		command.Parameters.RemoveAt(0);
		Assert.Empty(command.Parameters);
	}
}
