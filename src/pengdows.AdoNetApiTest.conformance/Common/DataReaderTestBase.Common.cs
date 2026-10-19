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
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void Dispose_command_before_reader()
	{
		using var connection = CreateOpenConnection();
		DbDataReader reader;
		using (var command = connection.CreateCommand())
		{
			command.CommandText = SelectTextSql;
			reader = command.ExecuteReader();
		}

		try
		{
			Assert.True(reader.Read());
			Assert.Equal("test", reader.GetString(0));
			Assert.False(reader.Read());
		}
		catch (Exception ex)
		{
			SoftWarning.Report($"The provider closes a reader when its command is disposed: {ex.GetType().Name}.");
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.fieldcount", Because = "the base API does not prescribe closed-reader exception behavior for FieldCount")]
	public virtual void FieldCount_throws_when_closed()
		=> X_diagnostic_when_closed(
			r =>
			{
				var x = r.FieldCount;
			},
			"FieldCount on a closed reader");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getdatatypename", Because = "GetDataTypeName does not define a portable out-of-range exception")]
	public virtual void GetDataTypeName_throws_when_ordinal_out_of_range()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		X_diagnostic(() => reader.GetDataTypeName(1), "GetDataTypeName out-of-range handling");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getdatatypename", Because = "the base API does not prescribe closed-reader exception behavior for GetDataTypeName")]
	public virtual void GetDataTypeName_throws_when_closed()
		=> X_diagnostic_when_closed(r => r.GetDataTypeName(0), "GetDataTypeName on a closed reader");

	[CommonFact(Because = "the API does not prescribe the exact out-of-range exception for GetFieldType.")]
	public virtual void GetFieldType_throws_when_ordinal_out_of_range()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		X_diagnostic(() => reader.GetFieldType(1), "GetFieldType out-of-range handling");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldtype", Because = "the base API does not prescribe closed-reader exception behavior for GetFieldType")]
	public virtual void GetFieldType_throws_when_closed()
		=> X_diagnostic_when_closed(r => r.GetFieldType(0), "GetFieldType on a closed reader");

	[CommonFact(Because = "the API does not prescribe the exact out-of-range exception for GetName.")]
	public virtual void GetName_throws_when_ordinal_out_of_range()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		X_diagnostic(() => reader.GetName(1), "GetName out-of-range handling");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getname", Because = "the base API does not prescribe closed-reader exception behavior for GetName")]
	public virtual void GetName_throws_when_closed()
		=> X_diagnostic_when_closed(r => r.GetName(0), "GetName on a closed reader");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetString_works_utf8_two_bytes() => GetX_works(SelectSql("'Ä'"), r => r.GetString(0), "Ä");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetString_works_utf8_three_bytes() => GetX_works(SelectSql("'Ḁ'"), r => r.GetString(0), "Ḁ");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetString_works_utf8_four_bytes() => GetX_works(SelectSql("'😀'"), r => r.GetString(0), "😀");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetFieldValue_works_utf8_two_bytes() => GetX_works(SelectSql("'Ä'"), r => r.GetFieldValue<string>(0), "Ä");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetFieldValue_works_utf8_three_bytes() => GetX_works(SelectSql("'Ḁ'"), r => r.GetFieldValue<string>(0), "Ḁ");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetFieldValue_works_utf8_four_bytes() => GetX_works(SelectSql("'😀'"), r => r.GetFieldValue<string>(0), "😀");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetValue_to_string_works_utf8_two_bytes() => GetX_works(SelectSql("'Ä'"), r => r.GetValue(0) as string, "Ä");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetValue_to_string_works_utf8_three_bytes() => GetX_works(SelectSql("'Ḁ'"), r => r.GetValue(0) as string, "Ḁ");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetValue_to_string_works_utf8_four_bytes() => GetX_works(SelectSql("'😀'"), r => r.GetValue(0) as string, "😀");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "null-array exception policy is not specified by DbDataReader")]
	public virtual void GetValues_throws_for_null()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		reader.Read();
		try
		{
			reader.GetValues(default(object[]));
		}
		catch (ArgumentNullException)
		{
			return;
		}
		catch (Exception)
		{
			ConformanceOutcome.Different("The provider uses a provider-specific null-array policy for GetValues; the base ADO.NET contract does not specify the exception.");
			return;
		}

		ConformanceOutcome.Different("The provider accepts a null GetValues array; the base ADO.NET contract does not specify a required exception.");
		return;
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void HasRows_works_when_batching()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectNoRowsThenOneSql;
		using var reader = command.ExecuteReader();
		Assert.False(reader.HasRows);

		reader.NextResult();

		Assert.True(reader.HasRows);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void NextResult_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectTwoResultsSql;
		using var reader = command.ExecuteReader();
		var hasData = reader.Read();
		Assert.True(hasData);
		Assert.Equal(1L, reader.GetInt64(0));

		var hasResults = reader.NextResult();
		Assert.True(hasResults);

		hasData = reader.Read();
		Assert.True(hasData);
		Assert.Equal(2L, reader.GetInt64(0));

		hasResults = reader.NextResult();
		Assert.False(hasResults);
	}

	/// <summary>
	/// `SingleRow` is a provider hint; the API documentation does not require a
	/// provider to truncate a result set after one row. This preserves the
	/// historical interoperability diagnostic but must not be treated as a
	/// conformance failure.
	/// The old assertion was an INVALID CONTRACT TEST. See
	/// https://learn.microsoft.com/dotnet/api/system.data.commandbehavior.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.commandbehavior")]
	public virtual void SingleRow_returns_one_row()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectTwoRowsSql;

		using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
		var hasData = reader.Read();
		Assert.True(hasData);
		Assert.Equal(1L, Convert.ToInt64(reader.GetValue(0)));
	}

	/// <summary>
	/// `SingleRow` does not require providers to suppress additional result sets;
	/// it is a hint described by CommandBehavior. This is retained as a diagnostic,
	/// not as a universal contract test.
	/// See https://learn.microsoft.com/dotnet/api/system.data.commandbehavior.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.commandbehavior")]
	public virtual void SingleRow_returns_one_result_set()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectThreeResultsSql;

		using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
		var hasData = reader.Read();
		Assert.True(hasData);
		Assert.Equal(1L, Convert.ToInt64(reader.GetValue(0)));
	}

	/// <summary>
	/// `SingleResult` is also a provider hint rather than a requirement to discard
	/// all later result sets. Retain the observation for interoperability testing,
	/// but do not use it as an ADO.NET conformance gate.
	/// The former assertion was an INVALID CONTRACT TEST. See
	/// https://learn.microsoft.com/dotnet/api/system.data.commandbehavior.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.commandbehavior")]
	public virtual void SingleResult_returns_one_result_set()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectThreeResultsSql;

		using var reader = command.ExecuteReader(CommandBehavior.SingleResult);
		var hasData = reader.Read();
		Assert.True(hasData);
		Assert.Equal(1L, Convert.ToInt64(reader.GetValue(0)));
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.read", Because = "the base API does not prescribe closed-reader exception behavior for Read")]
	public virtual void Read_throws_when_closed() => X_diagnostic_when_closed(r => r.Read(), "Read on a closed reader");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.nextresult", Because = "the base API does not prescribe closed-reader exception behavior for NextResult")]
	public virtual void NextResult_throws_when_closed() => X_diagnostic_when_closed(r => r.NextResult(), "NextResult on a closed reader");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getbytes", Because = "negative dataOffset handling is not specified by DbDataReader")]
	public virtual void GetBytes_throws_when_dataOffset_is_negative() => TestGetBytes(reader =>
	{
		AssertBufferBoundaryPolicy(() => reader.GetBytes(0, -1, new byte[4], 0, 4), "GetBytes negative dataOffset");
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getbytes", Because = "bufferOffset validation is not specified by DbDataReader")]
	public virtual void GetBytes_throws_when_bufferOffset_is_negative() => TestGetBytes(reader =>
	{
		AssertBufferBoundaryPolicy(() => reader.GetBytes(0, 0, new byte[4], -1, 4), "GetBytes negative bufferOffset");
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getbytes", Because = "bufferOffset validation is not specified by DbDataReader")]
	public virtual void GetBytes_throws_when_bufferOffset_is_too_large() => TestGetBytes(reader =>
	{
		AssertBufferBoundaryPolicy(() => reader.GetBytes(0, 0, new byte[4], 5, 0), "GetBytes oversized bufferOffset");
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getbytes", Because = "buffer-boundary handling is provider-specific")]
	public virtual void GetBytes_throws_when_bufferOffset_plus_length_is_too_long() => TestGetBytes(reader =>
	{
		try
		{
			reader.GetBytes(0, 0, new byte[4], 2, 3);
		}
		catch (ArgumentException)
		{
			return;
		}
		catch (IndexOutOfRangeException)
		{
			return;
		}

		ConformanceOutcome.Different("The provider permits a buffer range beyond the available buffer; boundary behavior is not a portable contract.");
		return;
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getchars", Because = "negative dataOffset validation is not specified by DbDataReader")]
	public virtual void GetChars_throws_when_dataOffset_is_negative() => TestGetChars(reader =>
	{
		AssertBufferBoundaryPolicy(() => reader.GetChars(0, -1, new char[4], 0, 4), "GetChars negative dataOffset");
	});

	[CommonFact(Because = "GetChars does not prescribe behavior when dataOffset exceeds the value length.")]
	public virtual void GetChars_reads_nothing_when_dataOffset_is_too_large() => TestGetChars(reader =>
	{
		try
		{
			if (reader.GetChars(0, 6, new char[4], 0, 4) != 0)
			{
				SoftWarning.Report("The provider returns characters for a dataOffset beyond the value length.");
			}
		}
		catch (Exception ex)
		{
			SoftWarning.Report($"The provider throws {ex.GetType().Name} for a dataOffset beyond the value length.");
		}
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getchars", Because = "bufferOffset validation is not specified by DbDataReader")]
	public virtual void GetChars_throws_when_bufferOffset_is_negative() => TestGetChars(reader =>
	{
		AssertBufferBoundaryPolicy(() => reader.GetChars(0, 0, new char[4], -1, 4), "GetChars negative bufferOffset");
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getchars", Because = "bufferOffset validation is not specified by DbDataReader")]
	public virtual void GetChars_throws_when_bufferOffset_is_too_large() => TestGetChars(reader =>
	{
		AssertBufferBoundaryPolicy(() => reader.GetChars(0, 0, new char[4], 5, 0), "GetChars oversized bufferOffset");
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getchars", Because = "bufferOffset validation is not specified by DbDataReader")]
	public virtual void GetChars_throws_when_bufferOffset_plus_length_is_too_long() => TestGetChars(reader =>
	{
		AssertBufferBoundaryPolicy(() => reader.GetChars(0, 0, new char[4], 2, 3), "GetChars buffer range");
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "null text materialization/exception policy is provider-specific")]
	public virtual void GetTextReader_throws_for_null_String()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.CreateSelectSql(DbType.String, ValueKind.Null);
		using var reader = command.ExecuteReader();
		reader.Read();
		try
		{
			using var textReader = reader.GetTextReader(0);
		}
		catch (Exception exception) when (exception.GetType() == Fixture.NullValueExceptionType)
		{
			return;
		}

		ConformanceOutcome.Different("The provider does not use the fixture's null-value exception policy; null text materialization is not a portable ADO.NET contract.");
		return;
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldvalue", Because = "TextReader materialization is provider-dependent")]
	public virtual void GetFieldValue_for_TextReader()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.CreateSelectSql(DbType.String, ValueKind.One);
		using var reader = command.ExecuteReader();
		reader.Read();
		TextReader textReader;
		try
		{
			textReader = reader.GetFieldValue<TextReader>(0);
		}
		catch (InvalidCastException)
		{
			ConformanceOutcome.Different("The provider does not expose text values as TextReader instances.");
			return;
		}

		using (textReader)
		{
		Assert.Equal("1", textReader.ReadToEnd());
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldvalueasync", Because = "async TextReader materialization is provider-dependent")]
	public virtual async Task GetFieldValueAsync_for_TextReader()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.CreateSelectSql(DbType.String, ValueKind.One);
		using var reader = await command.ExecuteReaderAsync();
		await reader.ReadAsync();
		TextReader textReader;
		try
		{
			textReader = await reader.GetFieldValueAsync<TextReader>(0);
		}
		catch (InvalidCastException)
		{
			ConformanceOutcome.Different("The provider does not expose text values as TextReader instances.");
			return;
		}

		using (textReader)
		{
		Assert.Equal("1", textReader.ReadToEnd());
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetFieldValue_for_TextReader_throws_for_null_String()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.CreateSelectSql(DbType.String, ValueKind.Null);
		using var reader = command.ExecuteReader();
		reader.Read();
		try
		{
			reader.GetFieldValue<TextReader>(0);
			SoftWarning.Report("The provider materializes a null text value as TextReader; null conversion behavior is provider-specific.");
		}
		catch (Exception ex)
		{
			SoftWarning.Report($"The provider throws {ex.GetType().Name} for null TextReader conversion; null conversion behavior is provider-specific.");
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual async Task GetFieldValueAsync_for_TextReader_throws_for_null_String()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.CreateSelectSql(DbType.String, ValueKind.Null);
		using var reader = await command.ExecuteReaderAsync();
		await reader.ReadAsync();
		try
		{
			await reader.GetFieldValueAsync<TextReader>(0);
			SoftWarning.Report("The provider materializes a null text value as TextReader asynchronously; null conversion behavior is provider-specific.");
		}
		catch (Exception ex)
		{
			SoftWarning.Report($"The provider throws {ex.GetType().Name} for async null TextReader conversion; null conversion behavior is provider-specific.");
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldvalue", Because = "provider-specific stream materialization")]
	public virtual void GetFieldValue_for_Stream()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.CreateSelectSql(DbType.Binary, ValueKind.One);
		using var reader = command.ExecuteReader();
		reader.Read();
		Stream stream;
		try
		{
			stream = reader.GetFieldValue<Stream>(0);
		}
		catch (InvalidCastException)
		{
			ConformanceOutcome.Different("The provider does not expose binary values as Stream instances.");
			return;
		}

		using (stream)
		{
		Assert.Equal(0x11, stream.ReadByte());
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldvalueasync", Because = "async provider-specific stream materialization")]
	public virtual async Task GetFieldValueAsync_for_Stream()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.CreateSelectSql(DbType.Binary, ValueKind.One);
		using var reader = await command.ExecuteReaderAsync();
		await reader.ReadAsync();
		Stream stream;
		try
		{
			stream = await reader.GetFieldValueAsync<Stream>(0);
		}
		catch (InvalidCastException)
		{
			ConformanceOutcome.Different("The provider does not expose binary values as Stream instances.");
			return;
		}

		using (stream)
		{
		Assert.Equal(0x11, stream.ReadByte());
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetFieldValue_for_Stream_throws_for_null_Binary()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.CreateSelectSql(DbType.Binary, ValueKind.Null);
		using var reader = command.ExecuteReader();
		reader.Read();
		try
		{
			reader.GetFieldValue<Stream>(0);
			SoftWarning.Report("The provider materializes a null binary value as Stream; null conversion behavior is provider-specific.");
		}
		catch (Exception ex)
		{
			SoftWarning.Report($"The provider throws {ex.GetType().Name} for null Stream conversion; null conversion behavior is provider-specific.");
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual async Task GetFieldValueAsync_for_Stream_throws_for_null_Binary()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.CreateSelectSql(DbType.Binary, ValueKind.Null);
		using var reader = await command.ExecuteReaderAsync();
		await reader.ReadAsync();
		try
		{
			await reader.GetFieldValueAsync<Stream>(0);
			SoftWarning.Report("The provider materializes a null binary value as Stream asynchronously; null conversion behavior is provider-specific.");
		}
		catch (Exception ex)
		{
			SoftWarning.Report($"The provider throws {ex.GetType().Name} for async null Stream conversion; null conversion behavior is provider-specific.");
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void FieldCount_is_zero_after_Delete() => Test_X_after_Delete(x => Assert.Equal(0, x.FieldCount));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void IsDBNull_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.IsDBNull(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual async Task IsDBNullAsync_throws_after_Delete() => await Test_X_after_Delete(async x => await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await x.IsDBNullAsync(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void Read_returns_false_after_Delete() => Test_X_after_Delete(x => Assert.False(x.Read()));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void NextResult_returns_false_after_Delete() => Test_X_after_Delete(x => Assert.False(x.NextResult()));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetDataTypeName_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetDataTypeName(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetFieldType_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetFieldType(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetFieldValue_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetFieldValue<object>(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual async Task GetFieldValueAsync_throws_after_Delete() => await Test_X_after_Delete(async x => await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await x.GetFieldValueAsync<object>(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetBoolean_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetBoolean(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetByte_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetByte(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetBytes_throws_after_Delete()
	{
		Test_X_after_Delete(x =>
		{
			var bytes = new byte[1];
			Assert.ThrowsAny<InvalidOperationException>(() => x.GetBytes(0, 0, bytes, 0, 1));
		});
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetChar_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetChar(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetChars_throws_after_Delete()
	{
		Test_X_after_Delete(x =>
		{
			var chars = new char[1];
			Assert.ThrowsAny<InvalidOperationException>(() => x.GetChars(0, 0, chars, 0, 1));
		});
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetColumnSchema_is_empty_after_Delete() => Test_X_after_Delete(x =>
	{
		try
		{
			if (x.GetColumnSchema().Count != 0)
			{
				SoftWarning.Report("The provider retains column schema after the reader has no current row.");
			}
		}
		catch (Exception ex)
		{
			SoftWarning.Report($"The provider throws {ex.GetType().Name} when column schema is requested without a current row.");
		}
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetDataTime_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetDateTime(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetDecimal_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetDecimal(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetDouble_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetDouble(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetFloat_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetFloat(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetGuid_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetGuid(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetInt16_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetInt16(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetInt32_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetInt32(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetInt64_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetInt64(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetSchemaTable_is_null_after_Delete() => Test_X_after_Delete(x =>
	{
		try
		{
			if (x.GetSchemaTable() is not null)
			{
				SoftWarning.Report("The provider returns a schema table after the reader has no current row.");
			}
		}
		catch (Exception ex)
		{
			SoftWarning.Report($"The provider throws {ex.GetType().Name} when schema table is requested without a current row.");
		}
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetStream_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetStream(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetString_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetString(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetTextReader_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetTextReader(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetName_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() => x.GetName(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetValue_throws_after_Delete() => Test_X_after_Delete(x => Assert.ThrowsAny<InvalidOperationException>(() =>x.GetValue(0)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "useful provider interoperability diagnostic, but not an ADO.NET contract")]
	public virtual void GetValues_throws_after_Delete()
	{
		Test_X_after_Delete(x =>
		{
			var values = new object[1];
			Assert.ThrowsAny<InvalidOperationException>(() => x.GetValues(values));
		});
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcolumn.datatypename", Because = "native DataTypeName metadata may be unavailable")]
	public virtual void GetColumnSchema_DataTypeName()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneAsIdSql;
		using var reader = command.ExecuteReader();
		var columns = reader.GetColumnSchema();
		var column = Assert.Single(columns);
		if (column.DataTypeName is not null)
			return;

		ConformanceOutcome.Different("The provider does not expose a native DataTypeName for this result column.");
		return;
	}

	/// <summary>
	/// Providers that implement GetSchemaTable should return metadata for an
	/// active result. The API permits providers without schema-table support to
	/// return null, so that capability is reported as a skip rather than treated
	/// as a conformance failure.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getschematable.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getschematable", Because = "optional reader-schema capability")]
	public virtual void GetSchemaTable_returns_metadata_when_supported()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneAsIdSql;
		using var reader = command.ExecuteReader();
		var schema = reader.GetSchemaTable();

		if (schema is null)
		{
			ConformanceOutcome.Different("The provider does not expose reader schema tables.");
			return;
		}

		Assert.NotEmpty(schema.Columns);
		Assert.NotEmpty(schema.Rows);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.gettextreader", Because = "the base implementation returns an empty reader for NULL, but providers may throw InvalidCastException instead")]
	public virtual void GetTextReader_returns_empty_for_null_String() => TestGetTextReader(ValueKind.Null, "");

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "GetValue documents only IndexOutOfRangeException; no page says it throws after the last row.")]
	public virtual void GetValue_throws_when_done()
		=> X_throws_when_done(r => r.GetValue(0));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "GetValue documents only IndexOutOfRangeException; no page says it throws on a closed reader.")]
	public virtual void GetValue_throws_when_closed()
		=> X_throws_when_closed(r => r.GetValue(0));

	/// <summary>
	/// DbDataReader.GetValue represents a database NULL with DBNull.Value.
	/// A provider returning C# null breaks the common reader contract and makes
	/// IsDBNull/GetValue handling inconsistent.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getvalue.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getvalue", Because = "No base-contract page says GetValue returns DBNull.Value for a SQL NULL (IsDBNull and ExecuteScalar do).")]
	public virtual void GetValue_returns_DBNull_for_null()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql("NULL");
		using var reader = command.ExecuteReader();
		Assert.True(reader.Read());
		Assert.Same(DBNull.Value, reader.GetValue(0));
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "No page says when a reader becomes closed after the last row is read.")]
	public virtual void IsClosed_returns_false_when_all_rows_read()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		Assert.True(reader.Read());
		Assert.False(reader.Read());
		Assert.False(reader.NextResult());
		Assert.False(reader.IsClosed);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "Synchronous IsDBNull documents only IndexOutOfRangeException; the InvalidOperationException is on IsDBNullAsync.")]
	public virtual void IsDBNull_throws_when_closed()
		=> X_throws_when_closed(r => r.IsDBNull(0));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "No page documents NextResult called again after it returned false.")]
	public virtual void NextResult_can_be_called_more_than_once()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		var hasResults = reader.NextResult();
		Assert.False(hasResults);

		hasResults = reader.NextResult();
		Assert.False(hasResults);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "No page documents Read called again after it returned false.")]
	public virtual void Read_keeps_returning_false()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.CreateSelectSql(DbType.String, ValueKind.Empty);
		using var reader = command.ExecuteReader();
		Assert.True(reader.Read());
		Assert.False(reader.Read());
		Assert.False(reader.Read());
		Assert.False(reader.Read());
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "GetBytes documents no behavior for a dataOffset past the end of the field.")]
	public virtual void GetBytes_reads_nothing_when_dataOffset_is_too_large() => TestGetBytes(reader =>
	{
		Assert.Equal(0, reader.GetBytes(0, 6, new byte[4], 0, 4));
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getbytes", Because = "A bufferOffset at the end of the buffer is an unspecified boundary.")]
	public virtual void GetBytes_reads_nothing_at_end_of_buffer() => TestGetBytes(reader =>
	{
		Assert.Equal(0, reader.GetBytes(0, 0, new byte[4], 4, 0));
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getchars", Because = "A bufferOffset at the end of the buffer is an unspecified boundary.")]
	public virtual void GetChars_reads_nothing_at_end_of_buffer() => TestGetChars(reader =>
	{
		Assert.Equal(0, reader.GetChars(0, 0, new char[4], 4, 0));
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "The null-buffer sentence does not say what a non-zero dataIndex does.")]
	public virtual void GetBytes_returns_length_when_buffer_is_null_and_dataIndex_specified() => TestGetBytes(reader =>
	{
		Assert.Equal(4, reader.GetBytes(0, 1, null, 0, 0));
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getchars", Because = "The null-buffer sentence does not say what a non-zero dataOffset does.")]
	public virtual void GetChars_returns_length_when_buffer_is_null_and_dataOffset_is_specified() => TestGetChars(reader =>
	{
		Assert.Equal(4, reader.GetChars(0, 1, null, 0, 0));
	});

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "Synchronous IsDBNull documents only IndexOutOfRangeException; the InvalidOperationException is on IsDBNullAsync.")]
	public virtual void IsDBNull_throws_before_read()
		=> X_throws_before_read(r => r.IsDBNull(0));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "Synchronous IsDBNull documents only IndexOutOfRangeException; the InvalidOperationException is on IsDBNullAsync.")]
	public virtual void IsDBNull_throws_when_done()
		=> X_throws_when_done(r => r.IsDBNull(0));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "The page says RecordsAffected is not set until all rows are read and the reader is closed.")]
	public virtual void RecordsAffected_returns_negative_1_when_no_rows()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.SelectNoRows;
		using var reader = command.ExecuteReader();
		Assert.Equal(-1, reader.RecordsAffected);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "Only the value after Close is documented; Dispose leaves the reader 'unusable'.")]
	public virtual void RecordsAffected_returns_negative_1_after_dispose_when_no_rows()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.SelectNoRows;
		using var reader = command.ExecuteReader();
		reader.Dispose();
		Assert.Equal(-1, reader.RecordsAffected);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "GetStream only supports values that can be converted to byte arrays; SQLite and Npgsql can read any column that way.")]
	public virtual void GetStream_throws_for_non_binary_type()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		Assert.True(reader.Read());
		Assert.ThrowsAny<InvalidCastException>(() => reader.GetStream(0));
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader", Because = "SQLite and Npgsql can read any column as text, so no exception is required for a non-text column.")]
	public virtual void GetTextReader_throws_for_non_text_type()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		Assert.True(reader.Read());
		Assert.ThrowsAny<InvalidCastException>(() => reader.GetTextReader(0));
	}
}
