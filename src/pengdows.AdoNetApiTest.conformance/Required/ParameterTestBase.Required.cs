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

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter", Waiver = "Only DbDataAdapter.Update reads this property; a different value cannot lose or alter data or change what a statement returns.")]
	public virtual void Parameter_default_SourceVersion_is_Current()
	{
		Assert.Equal(DataRowVersion.Current, Fixture.Factory.CreateParameter().SourceVersion);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void Parameter_default_Value_is_null()
	{
		Assert.Null(Fixture.Factory.CreateParameter().Value);
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

		parameter.DbType = DbType.String;
		parameter.Direction = ParameterDirection.Input;
		parameter.IsNullable = true;
		parameter.ParameterName = "value";
		parameter.Value = "x";
		parameter.Size = 32;

		Assert.Equal(DbType.String, parameter.DbType);
		Assert.Equal(ParameterDirection.Input, parameter.Direction);
		Assert.True(parameter.IsNullable);
		Assert.Equal("value", parameter.ParameterName);
		Assert.Equal(32, parameter.Size);
		Assert.Equal("x", parameter.Value);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter", Waiver = "Only DbDataAdapter.Update reads these properties; a different value cannot lose or alter data or change what a statement returns.")]
	public virtual void Parameter_adapter_properties_round_trip()
	{
		var parameter = Fixture.Factory.CreateParameter();

		parameter.SourceColumn = "source";
		parameter.SourceColumnNullMapping = true;
		parameter.SourceVersion = DataRowVersion.Original;

		Assert.Equal("source", parameter.SourceColumn);
		Assert.True(parameter.SourceColumnNullMapping);
		Assert.Equal(DataRowVersion.Original, parameter.SourceVersion);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterName_default_is_empty_string()
	{
		Assert.Equal("", Fixture.Factory.CreateParameter().ParameterName);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterCollection_RemoveAt_negative_one_throws()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.ThrowsAny<ArgumentOutOfRangeException>(() => command.Parameters.RemoveAt(-1));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterCollection_RemoveAt_zero_throws_when_empty()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.ThrowsAny<ArgumentOutOfRangeException>(() => command.Parameters.RemoveAt(0));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter")]
	public virtual void ParameterCollection_Insert_throws_for_negative_one()
	{
		using var command = Fixture.Factory.CreateCommand();
		var parameter = command.CreateParameter();
		Assert.ThrowsAny<ArgumentOutOfRangeException>(() => command.Parameters.Insert(-1, parameter));
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
		Assert.ThrowsAny<ArgumentOutOfRangeException>(() => command.Parameters.Insert(1, parameter));
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
