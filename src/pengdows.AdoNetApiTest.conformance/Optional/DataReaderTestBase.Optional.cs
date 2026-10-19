using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

public partial class DataReaderTestBase<TFixture>
	where TFixture : class, ISelectValueFixture, IDeleteFixture
{
#if NET10_0_OR_GREATER
	/// <summary>
	/// The asynchronous schema API must expose the same column metadata as the
	/// synchronous API. Providers without schema-table support may return null.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getschematableasync.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getschematableasync")]
	public virtual async Task GetSchemaTableAsync_returns_metadata_when_supported()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneAsIdSql;
		using var reader = command.ExecuteReader();
		var schema = await reader.GetSchemaTableAsync().ConfigureAwait(false);

		if (schema is null)
		{
			ConformanceOutcome.NotImplemented("The provider does not expose reader schema tables asynchronously.");
			return;
		}

		Assert.NotEmpty(schema.Columns);
		Assert.NotEmpty(schema.Rows);
	}

	/// <summary>
	/// The asynchronous column-schema API must preserve the column metadata
	/// exposed synchronously.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getcolumnschemaasync.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getcolumnschemaasync")]
	public virtual async Task GetColumnSchemaAsync_returns_column_metadata()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneAsIdSql;
		using var reader = command.ExecuteReader();
		System.Collections.ObjectModel.ReadOnlyCollection<DbColumn> columns;
		try
		{
			columns = await reader.GetColumnSchemaAsync().ConfigureAwait(false);
		}
		catch (NotSupportedException)
		{
			ConformanceOutcome.NotImplemented("The provider does not support GetColumnSchemaAsync.");
			return;
		}


		var column = Assert.Single(columns);
		Assert.Equal("id", column.ColumnName);
	}
#endif

	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void GetColumnSchema_ColumnName()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneAsIdSql;
		using var reader = command.ExecuteReader();
		if (!TryGetColumnSchema(reader, out var columns))
		{
			return;
		}

		var column = Assert.Single(columns);
		Assert.Equal("id", column.ColumnName);
	}

	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "the docs name no CLR type for a literal; any numeric type (integer, decimal or floating point) describes SELECT 1")]
	public virtual void GetColumnSchema_DataType()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneAsIdSql;
		using var reader = command.ExecuteReader();
		if (!TryGetColumnSchema(reader, out var columns))
		{
			return;
		}

		var column = Assert.Single(columns);
		switch (Type.GetTypeCode(column.DataType))
		{
		case TypeCode.Byte:
		case TypeCode.Int16:
		case TypeCode.Int32:
		case TypeCode.Int64:
		case TypeCode.SByte:
		case TypeCode.UInt16:
		case TypeCode.UInt32:
		case TypeCode.UInt64:
		case TypeCode.Decimal:
		case TypeCode.Double:
		case TypeCode.Single:
			return;
		}
		Assert.Fail($"DataType {column.DataType} isn't numeric");
	}

	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "The documentation says implementations may ignore the cancellation request; a provider that does passes as not implemented.")]
	public virtual async Task GetFieldValueAsync_is_canceled()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		reader.Read();

		try
		{
			// Documented: the exception is stored in the returned task. A provider that throws it directly still honored the request.
			await reader.GetFieldValueAsync<int>(0, CanceledToken);
		}
		catch (OperationCanceledException)
		{
			return;
		}

		ConformanceOutcome.NotImplemented("The provider ignores the cancellation request, which the documentation allows.");
	}

	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "The documentation says implementations may ignore the cancellation request; a provider that does passes as not implemented.")]
	public virtual async Task IsDBNullAsync_is_canceled()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		reader.Read();

		try
		{
			// Documented: the exception is stored in the returned task. A provider that throws it directly still honored the request.
			await reader.IsDBNullAsync(0, CanceledToken);
		}
		catch (OperationCanceledException)
		{
			return;
		}

		ConformanceOutcome.NotImplemented("The provider ignores the cancellation request, which the documentation allows.");
	}
}
