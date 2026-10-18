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
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void CommandText_throws_when_set_when_open_reader()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		using var reader = command.ExecuteReader();

		Assert.Throws<InvalidOperationException>(() => command.CommandText = SelectSql("2"));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void Connection_can_be_unset()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		Assert.NotNull(command.Connection);
		command.Connection = null;
		Assert.Null(command.Connection);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void Connection_throws_when_set_when_open_reader()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		using var reader = command.ExecuteReader();

		Assert.Throws<InvalidOperationException>(() => command.Connection = CreateConnection());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void Connection_throws_when_set_to_null_when_open_reader()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		using var reader = command.ExecuteReader();

		Assert.Throws<InvalidOperationException>(() => command.Connection = null);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public void CommandText_can_be_set_to_empty_string()
	{
		using var command = Fixture.Factory.CreateCommand();
		command.CommandText = "";
		Assert.Equal("", command.CommandText);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void CommandType_text_by_default()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.Equal(CommandType.Text, command.CommandType);
	}

	/// <summary>
	/// UpdatedRowSource controls how command results update a data source and is
	/// part of the DbCommand property contract.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.updatedrowsource.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.updatedrowsource")]
	public virtual void UpdatedRowSource_can_be_set_and_read()
	{
		using var command = Fixture.Factory.CreateCommand();
		command.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;
		Assert.Equal(UpdateRowSource.FirstReturnedRecord, command.UpdatedRowSource);
	}

	/// <summary>
	/// DesignTimeVisible is a common command property and must preserve the
	/// value assigned by the consumer.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.designtimevisible.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.designtimevisible")]
	public virtual void DesignTimeVisible_can_be_set_and_read()
	{
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
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.designtimevisible", Because = "the page states the default: true")]
	public virtual void DesignTimeVisible_default_is_true()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.True(command.DesignTimeVisible);
	}

	/// <summary>
	/// "The default is Both unless the command is automatically generated."
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.updatedrowsource.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.updatedrowsource", Because = "the page states the default: Both")]
	public virtual void UpdatedRowSource_default_is_Both()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.Equal(UpdateRowSource.Both, command.UpdatedRowSource);
	}

	/// <summary>
	/// "Throws a ArgumentException if the value entered was not one of the UpdateRowSource values."
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.updatedrowsource.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.updatedrowsource", Because = "the page states an ArgumentException for a value that is not an UpdateRowSource")]
	public virtual void UpdatedRowSource_throws_for_a_value_that_is_not_defined()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.ThrowsAny<ArgumentException>(() => command.UpdatedRowSource = (UpdateRowSource) 99);
	}

#if NETSTANDARD2_1_OR_GREATER
	/// <summary>
	/// DisposeAsync must complete the command disposal contract without requiring
	/// providers to implement a separate asynchronous resource path.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.disposeasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.disposeasync")]
	public virtual async Task DisposeAsync_works()
	{
		var command = Fixture.Factory.CreateCommand();
		await command.DisposeAsync().ConfigureAwait(false);
	}
#endif

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void Parameters_is_not_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.NotNull(command.Parameters);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void Parameters_returns_same_object()
	{
		using var command = Fixture.Factory.CreateCommand();
		var parameters = command.Parameters;
		Assert.Same(parameters, command.Parameters);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void CreateParameter_is_not_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.NotNull(command.CreateParameter());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void Prepare_throws_when_no_connection()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.Throws<InvalidOperationException>(() => command.Prepare());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void Prepare_throws_when_connection_closed()
	{
		using var connection = CreateConnection();
		using var command = connection.CreateCommand();
		Assert.Throws<InvalidOperationException>(() => command.Prepare());
	}


	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void ExecuteReader_throws_when_no_connection()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.Throws<InvalidOperationException>(() => command.ExecuteReader());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void ExecuteReader_throws_when_connection_closed()
	{
		using var connection = CreateConnection();
		using var command = connection.CreateCommand();
		Assert.Throws<InvalidOperationException>(() => command.ExecuteReader());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void ExecuteReader_throws_when_reader_open()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		using var reader = command.ExecuteReader();
		Assert.ThrowsAny<InvalidOperationException>(() => command.ExecuteReader());
	}

	/// <summary>
	/// Parameter binding uses the provider's advertised SQL placeholder while the
	/// DbParameter keeps only its logical name; no '@' prefix is assumed.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "NOW VALID PROVIDER-NEUTRAL PARAMETER-SYNTAX TEST")]
	public virtual void ExecuteReader_binds_parameters()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = $"SELECT {MakeParameterName(connection, "Parameter")};";
		var parameter = command.CreateParameter();
		parameter.ParameterName = "Parameter";
		parameter.Value = 1;
		command.Parameters.Add(parameter);

		var scalar = command.ExecuteScalar();
		if (scalar is int)
			Assert.Equal(1, scalar);
		else
			Assert.Equal(1L, scalar);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void ExecuteReader_supports_CloseConnection()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql("0");
		using (command.ExecuteReader(CommandBehavior.CloseConnection))
		{
		}
		Assert.Equal(ConnectionState.Closed, connection.State);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void ExecuteScalar_throws_when_no_connection()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.Throws<InvalidOperationException>(() => command.ExecuteScalar());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void ExecuteScalar_throws_when_connection_closed()
	{
		using var connection = CreateConnection();
		using var command = connection.CreateCommand();
		Assert.Throws<InvalidOperationException>(() => command.ExecuteScalar());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executescalar", Because = "the first column of the first row, as an object; no CLR type is named")]
	public virtual void ExecuteScalar_returns_integer()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		var result = command.ExecuteScalar();
		// The page says "the first column of the first row" as an object; it names no CLR type, so any numeric type holding 1 is correct.
		Assert.NotNull(result);
		Assert.IsNotType<DBNull>(result);
		Assert.Equal(1m, Convert.ToDecimal(result, System.Globalization.CultureInfo.InvariantCulture));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void ExecuteScalar_returns_real()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql("3.14");
		var result = command.ExecuteScalar();
		if (result is double)
			Assert.Equal(3.14, result);
		else
			Assert.Equal(3.14m, result);
	}

	/// <summary>
	/// ExecuteScalar returns the first column of the first row as an object. This
	/// diagnostic preserves the suite's original textual representation assertion;
	/// providers with a different representation can override it.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executescalar.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void ExecuteScalar_returns_string_when_text()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectTextSql;
		Assert.Equal("test", command.ExecuteScalar());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void ExecuteScalar_returns_DBNull_when_null()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql("NULL");
		Assert.Equal(DBNull.Value, command.ExecuteScalar());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void ExecuteScalar_returns_first_when_multiple_columns()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql("42, 43");
		Assert.Equal(42, Convert.ToInt32(command.ExecuteScalar()));
	}

	/// <summary>
	/// Without ORDER BY, SQL does not define row order. The ordered UNION makes the
	/// first-row assertion test ExecuteScalar's first-column/first-row contract
	/// instead of relying on accidental query-plan order.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executescalar.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executescalar", Because = "NOW VALID CONTRACT TEST (explicit ORDER BY)")]
	public virtual void ExecuteScalar_returns_first_when_multiple_rows()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectTwoRows42Sql;
		Assert.Equal(42, Convert.ToInt32(command.ExecuteScalar()));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void ExecuteNonQuery_throws_when_no_connection()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.Throws<InvalidOperationException>(() => command.ExecuteNonQuery());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void ExecuteNonQuery_throws_when_connection_closed()
	{
		using var connection = CreateConnection();
		using var command = connection.CreateCommand();
		Assert.Throws<InvalidOperationException>(() => command.ExecuteNonQuery());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual void ExecuteNonQuery_returns_negative_one_for_SELECT()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		Assert.Equal(-1, command.ExecuteNonQuery());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual async Task ExecuteReaderAsync_throws_when_no_connection()
	{
		using var command = Fixture.Factory.CreateCommand();
		await Assert.ThrowsAsync<InvalidOperationException>(command.ExecuteReaderAsync);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual async Task ExecuteReaderAsync_throws_when_connection_closed()
	{
		using var connection = CreateConnection();
		using var command = connection.CreateCommand();
		await Assert.ThrowsAsync<InvalidOperationException>(command.ExecuteReaderAsync);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual async Task ExecuteReaderAsync_throws_on_error()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = "INVALID;";
		await Assert.ThrowsAnyAsync<DbException>(command.ExecuteReaderAsync);
	}

	/// <summary>
	/// The asynchronous command API must be usable for a successful result, not
	/// only for invalid-state and cancellation paths. The base implementation may
	/// delegate to synchronous execution, but it must still return the command's
	/// result through the asynchronous API.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executereaderasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executereaderasync")]
	public virtual async Task ExecuteReaderAsync_returns_reader()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
		Assert.True(await reader.ReadAsync().ConfigureAwait(false));
		Assert.Equal(1, Convert.ToInt32(reader.GetValue(0)));
		Assert.False(await reader.ReadAsync().ConfigureAwait(false));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual async Task ExecuteScalarAsync_throws_when_no_connection()
	{
		using var command = Fixture.Factory.CreateCommand();
		await Assert.ThrowsAsync<InvalidOperationException>(command.ExecuteScalarAsync);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual async Task ExecuteScalarAsync_throws_when_connection_closed()
	{
		using var connection = CreateConnection();
		using var command = connection.CreateCommand();
		await Assert.ThrowsAsync<InvalidOperationException>(command.ExecuteScalarAsync);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand")]
	public virtual async Task ExecuteScalarAsync_throws_on_error()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = "INVALID;";
		await Assert.ThrowsAnyAsync<DbException>(command.ExecuteScalarAsync);
	}

	/// <summary>
	/// ExecuteScalarAsync must return the first column of the first row just as
	/// ExecuteScalar does. This tests successful asynchronous execution rather
	/// than cancellation policy, which providers are permitted to implement by
	/// delegating to synchronous execution.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executescalarasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executescalarasync")]
	public virtual async Task ExecuteScalarAsync_returns_first_value()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		var value = await command.ExecuteScalarAsync().ConfigureAwait(false);
		Assert.Equal(1, Convert.ToInt32(value));
	}

	/// <summary>
	/// ExecuteNonQueryAsync must return the same affected-row result as the
	/// synchronous ExecuteNonQuery contract for a result-producing statement.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonqueryasync
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonqueryasync")]
	public virtual async Task ExecuteNonQueryAsync_returns_result_producing_statement_count()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;

		Assert.Equal(-1, await command.ExecuteNonQueryAsync().ConfigureAwait(false));
	}

	/// <summary>
	/// An unsupported CLR parameter value is still tested using the provider's
	/// parameter syntax rather than a hardcoded SQL Server-style marker.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "NOW VALID PROVIDER-NEUTRAL PARAMETER-SYNTAX TEST; unsupported CLR-value handling remains a provider diagnostic")]
	public virtual void Execute_throws_for_unknown_ParameterValue_type()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = $"SELECT {MakeParameterName(connection, "Parameter")};";
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
