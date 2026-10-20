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
	/// A provider that supports preparation must leave a valid command executable
	/// after Prepare. Providers that do not support preparation may report that
	/// explicitly with NotSupportedException.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.prepare.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.prepare")]
	public virtual void Prepare_valid_command_remains_executable_or_reports_not_supported()
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
}
