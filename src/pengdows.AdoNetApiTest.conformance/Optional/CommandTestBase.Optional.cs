using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class CommandTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	/// <summary>
	/// CommandTimeout is an optional provider capability. A provider that exposes
	/// it accepts and preserves a valid value; a provider may explicitly report
	/// that timeout control is unsupported.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.commandtimeout.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.commandtimeout", Because = "unsupported timeout control is allowed")]
	public virtual void CommandTimeout_can_be_set_and_read()
	{
		using var command = Fixture.Factory.CreateCommand();
		try
		{
			command.CommandTimeout = 17;
		}
		catch (NotSupportedException)
		{
			ConformanceOutcome.NotImplemented("The provider does not support command timeouts.");
			return;
		}
		Assert.Equal(17, command.CommandTimeout);
	}

	/// <summary>
	/// SchemaOnly requests column metadata without returning result rows. Providers
	/// that implement the behavior must preserve the schema while reporting no
	/// data rows; an explicit NotSupportedException is treated as an optional
	/// capability response.
	/// See https://learn.microsoft.com/dotnet/api/system.data.commandbehavior.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.commandbehavior")]
	public virtual void ExecuteReader_supports_SchemaOnly_when_advertised()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneAsValueSql;

		try
		{
			using var reader = command.ExecuteReader(CommandBehavior.SchemaOnly);
			if (reader.FieldCount != 1 || reader.Read())
			{
				SoftWarning.Report("The provider does not expose metadata-only CommandBehavior.SchemaOnly results.");
			}
		}
		catch (NotSupportedException)
		{
			ConformanceOutcome.NotImplemented("The provider does not support CommandBehavior.SchemaOnly.");
			return;
		}
	}

	/// <summary>
	/// A prepared command is executed again with new parameter values, the reason to prepare it. MySql.Data's Prepare
	/// against TiDB threw KeyNotFoundException, and pengdows.crud turns preparation off for several providers.
	/// See https://learn.microsoft.com/dotnet/api/system.data.idbcommand.prepare.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idbcommand.prepare")]
	public virtual void Prepared_command_executes_again_with_new_parameter_values()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
		var parameter = command.CreateParameter();
		parameter.ParameterName = "Parameter";
		parameter.DbType = DbType.Int32;
		parameter.Value = 1;
		command.Parameters.Add(parameter);

		try
		{
			command.Prepare();
		}
		catch (NotSupportedException)
		{
			ConformanceOutcome.NotImplemented("The provider does not support prepared commands.");
			return;
		}

		Assert.Equal(1, Convert.ToInt32(command.ExecuteScalar()));
		parameter.Value = 2;
		Assert.Equal(2, Convert.ToInt32(command.ExecuteScalar()));
		parameter.Value = int.MaxValue;
		Assert.Equal(int.MaxValue, Convert.ToInt32(command.ExecuteScalar()));
	}

	/// <summary>
	/// Changing CommandText after Prepare runs the new text, whether the provider prepares it again or not.
	/// See https://learn.microsoft.com/dotnet/api/system.data.idbcommand.prepare.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idbcommand.prepare")]
	public virtual void Prepared_command_runs_new_CommandText()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		try
		{
			command.Prepare();
		}
		catch (NotSupportedException)
		{
			ConformanceOutcome.NotImplemented("The provider does not support prepared commands.");
			return;
		}

		Assert.Equal(1, Convert.ToInt32(command.ExecuteScalar()));
		command.CommandText = SelectSql("2");
		Assert.Equal(2, Convert.ToInt32(command.ExecuteScalar()));
		command.Prepare();
		Assert.Equal(2, Convert.ToInt32(command.ExecuteScalar()));
	}

#if !NETSTANDARD2_0
	/// <summary>
	/// PrepareAsync follows the same valid-command contract as Prepare. A provider
	/// may explicitly report that preparation is unsupported.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.prepareasync.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.prepareasync")]
	public virtual async Task PrepareAsync_valid_command_remains_executable_or_reports_not_supported()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		try
		{
			await command.PrepareAsync().ConfigureAwait(false);
		}
		catch (NotSupportedException)
		{
			ConformanceOutcome.NotImplemented("The provider does not support prepared commands.");
			return;
		}

		Assert.Equal(1, Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false)));
	}
#endif

	/// <summary>
	/// UpdatedRowSource controls how command results update a data source and is
	/// part of the DbCommand property contract.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.updatedrowsource.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.updatedrowsource", Because = "Only a provider with a DataAdapter needs the members DbDataAdapter.Update reads; one that has a DataAdapter must support all of them.")]
	public virtual void UpdatedRowSource_can_be_set_and_read()
	{
		if (!ImplementsDataAdapter())
		{
			return;
		}

		using var command = Fixture.Factory.CreateCommand();
		command.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;
		Assert.Equal(UpdateRowSource.FirstReturnedRecord, command.UpdatedRowSource);
	}

	/// <summary>
	/// DesignTimeVisible is a common command property and must preserve the
	/// value assigned by the consumer.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.designtimevisible.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.designtimevisible", Because = "Design-time support is optional; a provider whose new commands are DesignTimeVisible opts in and must then support all of it.")]
	public virtual void DesignTimeVisible_can_be_set_and_read()
	{
		if (!ImplementsDesignTime())
		{
			return;
		}

		using var command = Fixture.Factory.CreateCommand();
		command.DesignTimeVisible = false;
		Assert.False(command.DesignTimeVisible);
		command.DesignTimeVisible = true;
		Assert.True(command.DesignTimeVisible);
	}

	/// <summary>
	/// "The default is true."
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.designtimevisible.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.designtimevisible", Because = "Design-time support is optional; a provider whose new commands are DesignTimeVisible opts in and must then support all of it.")]
	public virtual void DesignTimeVisible_default_is_true()
	{
		if (!ImplementsDesignTime())
		{
			return;
		}

		using var command = Fixture.Factory.CreateCommand();
		Assert.True(command.DesignTimeVisible);
	}

	/// <summary>
	/// "The default is Both unless the command is automatically generated."
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.updatedrowsource.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.updatedrowsource", Because = "Only a provider with a DataAdapter needs the members DbDataAdapter.Update reads; one that has a DataAdapter must support all of them.")]
	public virtual void UpdatedRowSource_default_is_Both()
	{
		if (!ImplementsDataAdapter())
		{
			return;
		}

		using var command = Fixture.Factory.CreateCommand();
		Assert.Equal(UpdateRowSource.Both, command.UpdatedRowSource);
	}

	/// <summary>
	/// "Throws a ArgumentException if the value entered was not one of the UpdateRowSource values."
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.updatedrowsource.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.updatedrowsource", Because = "Only a provider with a DataAdapter needs the members DbDataAdapter.Update reads; one that has a DataAdapter must support all of them.")]
	public virtual void UpdatedRowSource_throws_for_a_value_that_is_not_defined()
	{
		if (!ImplementsDataAdapter())
		{
			return;
		}

		using var command = Fixture.Factory.CreateCommand();
		Assert.ThrowsAny<ArgumentException>(() => command.UpdatedRowSource = (UpdateRowSource) 99);
	}
}
