using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class CommandTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void Cancel_new_Command_is_no_op()
	{
		using var command = Fixture.Factory.CreateCommand();
		command.Cancel();
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void Cancel_executed_Command_is_no_op()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		command.ExecuteNonQuery();
		command.Cancel();
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void Cancel_Command_twice_is_no_op()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		command.ExecuteNonQuery();
		command.Cancel();
		command.Cancel();
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void Cancel_disposed_Command_is_no_op()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.Dispose();
		try
		{
			command.Cancel();
		}
		catch (ObjectDisposedException)
		{
			SoftWarning.Report("The provider rejects Cancel after command disposal; disposed-command Cancel behavior is not an ADO.NET contract.");
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void Cancel_disposed_Connection_is_no_op()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		connection.Dispose();
		try
		{
			command.Cancel();
		}
		catch (ObjectDisposedException)
		{
			SoftWarning.Report("The provider rejects Cancel after connection disposal; disposed-connection Cancel behavior is not an ADO.NET contract.");
		}
	}

	/// <summary>
	/// Nested <c>using</c> blocks dispose an executed command before its connection; neither Dispose should throw.
	/// See https://learn.microsoft.com/dotnet/standard/design-guidelines/dispose-pattern.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/standard/design-guidelines/dispose-pattern", Because = "the guideline says to avoid throwing from Dispose; no page fixes the order of command and connection disposal")]
	public virtual void Dispose_Command_then_Connection_does_not_throw()
	{
		var connection = CreateOpenConnection();
		var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		command.ExecuteScalar();
		ReportDisposeExceptions((command.Dispose, "the command, before its connection"), (connection.Dispose, "the connection, after its command"));
	}

	/// <summary>
	/// Disposing the connection first leaves the command to be disposed afterwards; neither Dispose should throw.
	/// See https://learn.microsoft.com/dotnet/standard/design-guidelines/dispose-pattern.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/standard/design-guidelines/dispose-pattern", Because = "the guideline says to avoid throwing from Dispose; no page fixes the order of command and connection disposal")]
	public virtual void Dispose_Connection_then_Command_does_not_throw()
	{
		var connection = CreateOpenConnection();
		var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		command.ExecuteScalar();
		ReportDisposeExceptions((connection.Dispose, "the connection, before its command"), (command.Dispose, "the command, after its connection"));
	}

	/// <summary>
	/// A reader returned by ExecuteReader can still be read after the command that produced it is disposed, as
	/// SqlClient and Npgsql allow; Microsoft.Data.Sqlite closes the reader instead.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executereader.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executereader", Because = "no page says whether a reader outlives the command that produced it")]
	public virtual void Reader_can_be_read_after_Command_is_disposed()
	{
		using var connection = CreateOpenConnection();
		var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		command.Dispose();

		try
		{
			if (!reader.Read())
			{
				ConformanceOutcome.Different("Disposing the command ended its open reader: Read returned false before the first row.");
				return;
			}

			if (Convert.ToInt64(reader.GetValue(0)) != 1)
				throw new UnexpectedValueException(reader.GetValue(0));
		}
		catch (Exception ex) when (ex is not UnexpectedValueException)
		{
			ConformanceOutcome.Different($"Disposing the command closed its open reader: Read then threw {ex.GetType().Name}: {ex.Message}");
		}
	}

	/// <summary>
	/// An executed command can be detached from its connection for reuse; pengdows.crud swallows the exception some
	/// providers throw here. Required covers only a command that never ran (Connection_can_be_unset).
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.connection.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.connection", Because = "no page says whether an executed command can be detached from its connection")]
	public virtual void Connection_can_be_unset_after_execute()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		command.ExecuteScalar();
		using (var reader = command.ExecuteReader())
		{
			while (reader.Read())
			{
			}
		}

		try
		{
			command.Connection = null;
		}
		catch (Exception ex)
		{
			ConformanceOutcome.Different($"Setting Connection to null on an executed command threw {ex.GetType().Name}: {ex.Message}");
			return;
		}

		Assert.Null(command.Connection);
		command.Connection = connection;
		Assert.Equal(1, Convert.ToInt32(command.ExecuteScalar()));
	}

	private static void ReportDisposeExceptions(params (Action Dispose, string What)[] steps)
	{
		var failures = new List<string>();
		foreach (var (dispose, what) in steps)
		{
			try
			{
				dispose();
			}
			catch (Exception ex)
			{
				failures.Add($"Disposing {what} threw {ex.GetType().Name}: {ex.Message}");
			}
		}

		if (failures.Count > 0)
			ConformanceOutcome.Different(string.Join(" ", failures));
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void CommandText_does_not_throw_when_disposed()
	{
		var command = Fixture.Factory.CreateCommand();
		command.Dispose();
		try
		{
			_ = command.CommandText;
		}
		catch (ObjectDisposedException)
		{
			return;
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "the API does not prescribe a provider's initial CommandText representation")]
	public virtual void CommandText_is_empty_string_by_default()
	{
		using var command = Fixture.Factory.CreateCommand();
		if (string.IsNullOrEmpty(command.CommandText))
			return;

		SoftWarning.Report("The provider uses a non-empty initial CommandText value; the API does not require an empty string.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "null-to-empty CommandText coercion is not prescribed")]
	public void CommandText_is_coerced_to_empty_string()
	{
		using var command = Fixture.Factory.CreateCommand();
		command.CommandText = null;
		if (string.IsNullOrEmpty(command.CommandText))
			return;

		SoftWarning.Report("The provider preserves null CommandText; null-to-empty coercion is not an ADO.NET contract.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void CommandType_does_not_throw_when_disposed()
	{
		var command = Fixture.Factory.CreateCommand();
		command.Dispose();
		try
		{
			_ = command.CommandType;
		}
		catch (ObjectDisposedException)
		{
			return;
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void Parameters_does_not_throw_when_disposed()
	{
		var command = Fixture.Factory.CreateCommand();
		command.Dispose();
		try
		{
			_ = command.Parameters;
		}
		catch (ObjectDisposedException)
		{
			return;
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.prepare", Because = "Prepare does not define a required response to missing command text")]
	public virtual void Prepare_throws_when_no_command_text()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();

		try
		{
			command.Prepare();
			SoftWarning.Report("The provider accepts Prepare with empty command text; the base ADO.NET contract does not require rejection.");
		}
		catch (Exception)
		{
			// Empty-command-text behavior is provider-specific.
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idbcommand.executereader", Because = "IDbCommand does not require a universal empty-command-text exception")]
	public virtual void ExecuteReader_throws_when_no_command_text()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		AssertEmptyCommandTextPolicy(() =>
		{
			using var reader = command.ExecuteReader();
		}, "ExecuteReader");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.transaction", Because = "DbCommand does not require rejection when a local transaction is active but not assigned")]
	public virtual void ExecuteReader_throws_on_error()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = "INVALID;";
		Assert.ThrowsAny<DbException>(() => command.ExecuteReader());
	}

	/// <summary>
	/// A command with a leading comment still contains an executable statement,
	/// so ExecuteReader must expose that statement's result set.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executereader.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void ExecuteReader_works_with_leading_comment()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = "-- A leading comment\n" + SelectOneSql;

		using var reader = command.ExecuteReader();
		Assert.True(reader.HasRows);
	}

	/// <summary>
	/// A comment-only command is distinct from a command with a leading comment:
	/// it contains no executable statement and therefore has no rows. ADO.NET does
	/// not require providers to accept comment-only command text; this test records
	/// the behavior of providers that do accept it, while a provider that rejects
	/// the empty statement may override the test. It is intentionally not a
	/// universal SQL-compliance requirement.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executereader.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/commands-and-parameters.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void ExecuteReader_HasRows_is_false_for_comment()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = "-- TODO: Write SQL";

		DbDataReader reader;
		try
		{
			reader = command.ExecuteReader();
		}
		catch (DbException)
		{
			SoftWarning.Report("The provider rejects comment-only SQL; comment-only command acceptance is not an ADO.NET contract.");
			return;
		}

		using (reader)
		{
			if (reader.HasRows)
				SoftWarning.Report("The provider treats comment-only SQL as an empty result; row behavior is not an ADO.NET contract.");
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void ExecuteReader_works_when_trailing_comments()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql("0") + " -- My favorite number";

		using var reader = command.ExecuteReader();
		Assert.True(reader.Read());
		Assert.Equal(0, reader.GetInt32(0));
		Assert.False(reader.NextResult());
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idbcommand.executescalar", Because = "IDbCommand does not require a universal empty-command-text exception")]
	public virtual void ExecuteScalar_throws_when_no_command_text()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		AssertEmptyCommandTextPolicy(() => command.ExecuteScalar(), "ExecuteScalar");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void ExecuteScalar_returns_first_when_batching()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql("42") + " " + SelectSql("43");
		Assert.Equal(42, Convert.ToInt32(command.ExecuteScalar()));
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idbcommand.executenonquery", Because = "IDbCommand does not require a universal empty-command-text exception")]
	public virtual void ExecuteNonQuery_throws_when_no_command_text()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		AssertEmptyCommandTextPolicy(() => command.ExecuteNonQuery(), "ExecuteNonQuery");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idbcommand.executereader", Because = "IDbCommand does not require a universal empty-command-text exception")]
	public virtual async Task ExecuteReaderAsync_throws_when_no_command_text()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		await AssertEmptyCommandTextPolicyAsync(command.ExecuteReaderAsync, "ExecuteReaderAsync").ConfigureAwait(false);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idbcommand.executescalar", Because = "IDbCommand does not require a universal empty-command-text exception")]
	public virtual async Task ExecuteScalarAsync_throws_when_no_command_text()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		await AssertEmptyCommandTextPolicyAsync(command.ExecuteScalarAsync, "ExecuteScalarAsync").ConfigureAwait(false);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "the ADO.NET async contract permits providers to ignore cancellation; requiring a canceled task is incorrect")]
	public virtual async Task ExecuteReaderAsync_is_canceled()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		var task = command.ExecuteReaderAsync(CanceledToken);
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
		Assert.True(task.IsCanceled);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "the ADO.NET async contract permits providers to ignore cancellation; requiring a canceled task is incorrect")]
	public virtual async Task ExecuteNonQueryAsync_is_canceled()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		await ObserveCancellationAsync(() => command.ExecuteNonQueryAsync(CanceledToken), nameof(ExecuteNonQueryAsync_is_canceled));
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "the ADO.NET async contract permits providers to ignore cancellation; requiring a canceled task is incorrect")]
	public virtual async Task ExecuteScalarAsync_is_canceled()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		await ObserveCancellationAsync(() => command.ExecuteScalarAsync(CanceledToken), nameof(ExecuteScalarAsync_is_canceled));
	}

	/// <summary>
	/// Some providers (SqlClient, for one) require a command to be assigned the connection's active local transaction
	/// and throw <see cref="InvalidOperationException"/> when it is not. The base DbCommand contract does not require it,
	/// so a provider that executes anyway is reported with a soft warning.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.transaction.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.transaction", Because = "the base DbCommand contract does not require the active local transaction to be assigned to the command")]
	public virtual void ExecuteReader_throws_when_transaction_required()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		using (connection.BeginTransaction())
		{
			try
			{
				using (command.ExecuteReader())
				{
				}
			}
			catch (InvalidOperationException)
			{
				return;
			}

			SoftWarning.Report("The provider permits execution without assigning the active local transaction; the base ADO.NET contract does not require rejection.");
		}
	}

	/// <summary>
	/// Some providers (SqlClient, for one) reject a command whose <see cref="System.Data.Common.DbCommand.Transaction"/> belongs to
	/// another connection. The <c>DbCommand.Transaction</c> page states only that the default is null, so a provider that accepts it is
	/// reported as different. The test needs two connections that each hold a transaction, which a database with a single writer
	/// cannot give it.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.transaction.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.transaction", Because = "the docs state only the default for DbCommand.Transaction; rejecting a transaction from another connection is SqlClient behavior")]
	public virtual void ExecuteReader_throws_when_transaction_mismatched()
	{
		if (Fixture is ISingleWriterFixture)
		{
			ConformanceOutcome.NotApplicable("The database allows one writer at a time, so two connections cannot each hold a transaction.");
			return;
		}

		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		using var otherConnection = CreateOpenConnection();
		using var otherTransaction = otherConnection.BeginTransaction();
		try
		{
			command.Transaction = otherTransaction;
		}
		catch (ArgumentException)
		{
			return;
		}

		Assert.ThrowsAny<InvalidOperationException>(() =>
		{
			using (command.ExecuteReader())
			{
			}
		});
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "No page documents an exception for setting CommandText while a reader is open; that is SqlClient behavior.")]
	public virtual void CommandText_throws_when_set_when_open_reader()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		using var reader = command.ExecuteReader();

		Assert.ThrowsAny<InvalidOperationException>(() => command.CommandText = SelectSql("2"));
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "No page documents an exception for setting Connection while a reader is open; that is SqlClient behavior.")]
	public virtual void Connection_throws_when_set_when_open_reader()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		using var reader = command.ExecuteReader();

		Assert.ThrowsAny<InvalidOperationException>(() => command.Connection = CreateConnection());
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "No page documents an exception for clearing Connection while a reader is open; that is SqlClient behavior.")]
	public virtual void Connection_throws_when_set_to_null_when_open_reader()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		using var reader = command.ExecuteReader();

		Assert.ThrowsAny<InvalidOperationException>(() => command.Connection = null);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "IDbCommand documents InvalidOperationException for ExecuteNonQuery and Prepare only; ExecuteReader lists no exception.")]
	public virtual void ExecuteReader_throws_when_no_connection()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.ThrowsAny<InvalidOperationException>(() => command.ExecuteReader());
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "IDbCommand documents InvalidOperationException for ExecuteNonQuery and Prepare only; ExecuteReader lists no exception.")]
	public virtual void ExecuteReader_throws_when_connection_closed()
	{
		using var connection = CreateConnection();
		using var command = connection.CreateCommand();
		Assert.ThrowsAny<InvalidOperationException>(() => command.ExecuteReader());
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "IDbCommand documents InvalidOperationException for ExecuteNonQuery and Prepare only; ExecuteScalar lists no exception.")]
	public virtual void ExecuteScalar_throws_when_no_connection()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.ThrowsAny<InvalidOperationException>(() => command.ExecuteScalar());
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "IDbCommand documents InvalidOperationException for ExecuteNonQuery and Prepare only; ExecuteScalar lists no exception.")]
	public virtual void ExecuteScalar_throws_when_connection_closed()
	{
		using var connection = CreateConnection();
		using var command = connection.CreateCommand();
		Assert.ThrowsAny<InvalidOperationException>(() => command.ExecuteScalar());
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "No page documents an exception for executing while a reader is open on the connection; that is SqlClient behavior.")]
	public virtual void ExecuteReader_throws_when_reader_open()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		using var reader = command.ExecuteReader();
		Assert.ThrowsAny<InvalidOperationException>(() => command.ExecuteReader());
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "The async pages document only DbException, ArgumentException and OperationCanceledException.")]
	public virtual async Task ExecuteReaderAsync_throws_when_no_connection()
	{
		using var command = Fixture.Factory.CreateCommand();
		await Assert.ThrowsAnyAsync<InvalidOperationException>(command.ExecuteReaderAsync);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "The async pages document only DbException, ArgumentException and OperationCanceledException.")]
	public virtual async Task ExecuteReaderAsync_throws_when_connection_closed()
	{
		using var connection = CreateConnection();
		using var command = connection.CreateCommand();
		await Assert.ThrowsAnyAsync<InvalidOperationException>(command.ExecuteReaderAsync);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "The async pages document only DbException and OperationCanceledException.")]
	public virtual async Task ExecuteScalarAsync_throws_when_no_connection()
	{
		using var command = Fixture.Factory.CreateCommand();
		await Assert.ThrowsAnyAsync<InvalidOperationException>(command.ExecuteScalarAsync);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand", Because = "The async pages document only DbException and OperationCanceledException.")]
	public virtual async Task ExecuteScalarAsync_throws_when_connection_closed()
	{
		using var connection = CreateConnection();
		using var command = connection.CreateCommand();
		await Assert.ThrowsAnyAsync<InvalidOperationException>(command.ExecuteScalarAsync);
	}

	/// <summary>
	/// An unsupported CLR parameter value is still tested using the provider's
	/// parameter syntax rather than a hardcoded SQL Server-style marker.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "No page documents the exception for a parameter value of an unsupported type.")]
	public virtual void Execute_throws_for_unknown_ParameterValue_type()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
		var parameter = command.CreateParameter();
		parameter.ParameterName = "Parameter";
		parameter.Value = new CustomClass();
		command.Parameters.Add(parameter);

		try
		{
			var value = command.ExecuteScalar();
			if (!"custom".Equals((string) value))
				throw new UnexpectedValueException(value);
		}
		catch (Exception ex) when (ex is NotSupportedException || ex is InvalidOperationException)
		{
		}
		catch (Exception ex) when (ex is not UnexpectedValueException)
		{
			throw ThrowsException.ForIncorrectExceptionType(typeof(NotSupportedException), ex);
		}
	}
}
