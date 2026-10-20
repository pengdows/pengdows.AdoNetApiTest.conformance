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
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void Depth_returns_zero()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		Assert.Equal(0, reader.Depth);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executescalar")]
	public virtual void ExecuteScalar_returns_null_when_empty()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.SelectNoRows;
		Assert.Null(command.ExecuteScalar());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void FieldCount_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		Assert.True(reader.Read());
		Assert.Equal(1, reader.FieldCount);
	}

	/// <summary>
	/// For a result without hidden columns, VisibleFieldCount reports the visible
	/// column count and is never greater than FieldCount.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.visiblefieldcount.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.visiblefieldcount")]
	public virtual void VisibleFieldCount_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();

		Assert.Equal(1, reader.VisibleFieldCount);
		Assert.InRange(reader.VisibleFieldCount, 0, reader.FieldCount);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getbytes")]
	public virtual void GetBytes_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.CreateSelectSql(new byte[] { 0x7E, 0x57 });
		using var reader = command.ExecuteReader();
		var hasData = reader.Read();
		Assert.True(hasData);

		var buffer = new byte[2];
		Assert.Equal(2, reader.GetBytes(0, 0, buffer, 0, buffer.Length));
		Assert.Equal(new byte[] { 0x7E, 0x57 }, buffer);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getchars")]
	public virtual void GetChars_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectTextSql;
		using var reader = command.ExecuteReader();
		var hasData = reader.Read();
		Assert.True(hasData);

		var buffer = new char[4];
		Assert.Equal(4, reader.GetChars(0, 0, buffer, 0, buffer.Length));
		Assert.Equal(new[] { 't', 'e', 's', 't' }, buffer);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void GetEnumerator_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		var hasData = reader.Read();
		Assert.True(hasData);

		Assert.NotNull(reader.GetEnumerator());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void GetFieldValue_of_DBNull_throws_when_not_null()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		var hasData = reader.Read();

		Assert.True(hasData);
		Assert.ThrowsAny<InvalidCastException>(() => reader.GetFieldValue<DBNull>(0));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldvalue")]
	public virtual void GetFieldValue_throws_before_read()
		=> X_throws_before_read(r => r.GetFieldValue<DBNull>(0));

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldvalue")]
	public virtual void GetFieldValue_throws_when_done()
		=> X_throws_when_done(r => r.GetFieldValue<DBNull>(0));

	/// <summary>
	/// GetFieldValueAsync must return the field value on a successful read. The
	/// default implementation may delegate to GetFieldValue, but it must preserve
	/// the reader contract through the asynchronous API.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldvalueasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldvalueasync")]
	public virtual async Task GetFieldValueAsync_returns_value()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

		Assert.True(await reader.ReadAsync().ConfigureAwait(false));
		var value = await reader.GetFieldValueAsync<object>(0).ConfigureAwait(false);

		Assert.Equal(1, Convert.ToInt32(value));
	}

	/// <summary>
	/// A reader returned by ExecuteReaderAsync must support successful ReadAsync
	/// and expose the same row data as the synchronous reader API.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.readasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.readasync")]
	public virtual async Task ReadAsync_returns_rows()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

		Assert.True(await reader.ReadAsync().ConfigureAwait(false));
		Assert.Equal(1, Convert.ToInt32(reader.GetValue(0)));
		Assert.False(await reader.ReadAsync().ConfigureAwait(false));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void GetFieldType_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectTextSql;
		using var reader = command.ExecuteReader();
		Assert.True(reader.Read());
		// IDataRecord.GetFieldType: "the type of Object that would be returned from GetValue" (a base type is allowed).
		Assert.True(reader.GetFieldType(0).IsInstanceOfType(reader.GetValue(0)));
	}

	/// <summary>
	/// The provider-specific reader API must expose the provider-specific field
	/// type for a valid current column. The returned type may differ from the
	/// common CLR type, but it must be usable as metadata.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getproviderspecificfieldtype.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getproviderspecificfieldtype", Waiver = "Provider-specific accessors fall back to the portable ones; a deviation changes no portable result.")]
	public virtual void GetProviderSpecificFieldType_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();

		Assert.True(reader.Read());
		Assert.NotNull(reader.GetProviderSpecificFieldType(0));
	}

	/// <summary>
	/// GetProviderSpecificValue returns the current column value using the
	/// provider-specific representation selected by the reader.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getproviderspecificvalue.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getproviderspecificvalue", Waiver = "Provider-specific accessors fall back to the portable ones; a deviation changes no portable result.")]
	public virtual void GetProviderSpecificValue_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();

		Assert.True(reader.Read());
		Assert.NotNull(reader.GetProviderSpecificValue(0));
	}

	/// <summary>
	/// GetProviderSpecificValues copies one provider-specific value per result
	/// column and reports the number copied.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getproviderspecificvalues.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getproviderspecificvalues", Waiver = "Provider-specific accessors fall back to the portable ones; a deviation changes no portable result.")]
	public virtual void GetProviderSpecificValues_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();

		Assert.True(reader.Read());
		var values = new object[1];

		Assert.Equal(1, reader.GetProviderSpecificValues(values));
		Assert.NotNull(values[0]);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void GetName_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneAsIdSql;
		using var reader = command.ExecuteReader();
		// SQL-92 folds an undelimited name to upper case (Oracle, Firebird return ID); GetName returns the name the database gave.
		Assert.Equal("id", reader.GetName(0), ignoreCase: true);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getordinal")]
	public virtual void GetOrdinal_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneAsIdSql;
		using var reader = command.ExecuteReader();
		Assert.Equal(0, reader.GetOrdinal("Id"));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getordinal")]
	public virtual void GetOrdinal_throws_when_out_of_range()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		Assert.ThrowsAny<IndexOutOfRangeException>(() => reader.GetOrdinal("Name"));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void GetString_works()
		=> GetX_works(
			SelectTextSql,
			r => r.GetString(0),
			"test");

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void GetValue_throws_before_read()
		=> X_throws_before_read(r => r.GetValue(0));

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void GetValues_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql("'a', NULL");
		using var reader = command.ExecuteReader();
		var hasData = reader.Read();
		Assert.True(hasData);

		// Array may be wider than row
		var values = new object[3];
		var result = reader.GetValues(values);

		Assert.Equal(2, result);
		Assert.Equal("a", values[0]);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getvalues")]
	public virtual void GetValues_copies_only_what_fits_when_the_array_is_shorter()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql("'a', 'b'");
		using var reader = command.ExecuteReader();
		Assert.True(reader.Read());

		// IDataRecord.GetValues: "You can pass an Object array that contains fewer than the number of columns ...
		// Only the amount of data the Object array holds is copied to the array."
		var values = new object[1];
		Assert.Equal(1, reader.GetValues(values));
		Assert.Equal("a", values[0]);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getvalues", Waiver = "A zero-length array is the extreme of the documented shorter array; providers that throw here still copy a shorter array correctly, so no data is lost.")]
	public virtual void GetValues_with_an_empty_array_copies_nothing()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		Assert.True(reader.Read());

		var values = new object[0];
		Assert.Equal(0, reader.GetValues(values));
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void HasRows_returns_true_when_rows()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		Assert.True(reader.HasRows);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.hasrows")]
	public virtual void HasRows_returns_false_when_no_rows()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.SelectNoRows;
		using var reader = command.ExecuteReader();
		Assert.False(reader.HasRows);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void IsClosed_returns_false_when_active()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();
		Assert.False(reader.IsClosed);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void IsClosed_returns_true_when_closed()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		var reader = command.ExecuteReader();
		reader.Close();

		Assert.True(reader.IsClosed);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void IsDBNull_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql("NULL");
		using var reader = command.ExecuteReader();
		var hasData = reader.Read();

		Assert.True(hasData);
		Assert.True(reader.IsDBNull(0));
	}

#if !NETSTANDARD2_0
	/// <summary>
	/// IsDBNullAsync must report the value state through the asynchronous reader
	/// API just as IsDBNull does. The test deliberately uses a non-null literal so
	/// it does not depend on a provider-specific column type.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.isdbnullasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.isdbnullasync")]
	public virtual async Task IsDBNullAsync_returns_value_state()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

		Assert.True(await reader.ReadAsync().ConfigureAwait(false));
		Assert.False(await reader.IsDBNullAsync(0).ConfigureAwait(false));
	}
#endif

#if !NETSTANDARD2_0
	/// <summary>
	/// NextResultAsync must complete successfully and report that a single-result
	/// command has no additional result set.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.nextresultasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.nextresultasync")]
	public virtual async Task NextResultAsync_returns_false_after_last_result()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

		Assert.False(await reader.NextResultAsync().ConfigureAwait(false));
	}
#endif

#if !NETSTANDARD2_0
	/// <summary>
	/// DisposeAsync must close the reader and release its active state.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.disposeasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.disposeasync")]
	public virtual async Task DisposeAsync_closes_reader()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

		await reader.DisposeAsync().ConfigureAwait(false);

		Assert.True(reader.IsClosed);
	}
#endif

#if !NETSTANDARD2_0
#if !NETSTANDARD2_0
	/// <summary>
	/// CloseAsync must complete the documented asynchronous reader-close
	/// operation and leave the reader closed.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.closeasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.closeasync")]
	public virtual async Task CloseAsync_closes_reader()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		using var reader = command.ExecuteReader();

		await reader.CloseAsync().ConfigureAwait(false);

		Assert.True(reader.IsClosed);
	}
#endif
#endif

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void Item_by_ordinal_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectTextSql;
		using var reader = command.ExecuteReader();
		var hasData = reader.Read();
		Assert.True(hasData);

		Assert.Equal("test", reader[0]?.ToString());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void Item_by_name_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectTextAsIdSql;
		using var reader = command.ExecuteReader();
		var hasData = reader.Read();
		Assert.True(hasData);

		Assert.Equal("test", reader["Id"]?.ToString());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void Read_works()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectTwoRowsSql;
		using var reader = command.ExecuteReader();
		var hasData = reader.Read();
		Assert.True(hasData);
		Assert.Equal(1L, Convert.ToInt64(reader.GetValue(0), System.Globalization.CultureInfo.InvariantCulture));

		hasData = reader.Read();
		Assert.True(hasData);
		Assert.Equal(2L, Convert.ToInt64(reader.GetValue(0), System.Globalization.CultureInfo.InvariantCulture));

		hasData = reader.Read();
		Assert.False(hasData);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.recordsaffected")]
	public virtual void RecordsAffected_returns_negative_1_after_close_when_no_rows()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.SelectNoRows;
		using var reader = command.ExecuteReader();
		reader.Close();
		Assert.Equal(-1, reader.RecordsAffected);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getbytes")]
	public virtual void GetBytes_returns_length_when_buffer_is_null() => TestGetBytes(reader =>
	{
		Assert.Equal(4, reader.GetBytes(0, 0, null, 0, 0));
	});

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getbytes")]
	public virtual void GetBytes_works_when_buffer_is_large() => TestGetBytes(reader =>
	{
		var buffer = new byte[6];
		Assert.Equal(4, reader.GetBytes(0, 0, buffer, 0, 6));
		Assert.Equal(new byte[] { 1, 2, 3, 4, 0, 0 }, buffer);
	});

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getbytes")]
	public virtual void GetBytes_reads_part_of_blob() => TestGetBytes(reader =>
	{
		var buffer = new byte[5];
		Assert.Equal(2, reader.GetBytes(0, 1, buffer, 2, 2));
		Assert.Equal(new byte[] { 0, 0, 2, 3, 0 }, buffer);
	});

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getchars")]
	public virtual void GetChars_returns_length_when_buffer_is_null() => TestGetChars(reader =>
	{
		Assert.Equal(4, reader.GetChars(0, 0, null, 0, 0));
	});

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getchars")]
	public virtual void GetChars_works_when_buffer_is_large() => TestGetChars(reader =>
	{
		var buffer = new char[6];
		Assert.Equal(4, reader.GetChars(0, 0, buffer, 0, 6));
		Assert.Equal(new[] { 'a', 'b', '¢', 'd', '\0', '\0' }, buffer);
	});

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getchars")]
	public virtual void GetChars_reads_part_of_string() => TestGetChars(reader =>
	{
		var buffer = new char[5];
		Assert.Equal(2, reader.GetChars(0, 1, buffer, 2, 2));
		Assert.Equal(new[] { '\0', '\0', 'b', '¢', '\0' }, buffer);
	});

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void GetTextReader_for_one_String() => TestGetTextReader(ValueKind.One, "1");

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader")]
	public virtual void GetTextReader_for_empty_String() => TestGetTextReader(ValueKind.Empty, "");

}
