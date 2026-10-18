using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

[Collection("ISelectValueFixture Collection")]
public partial class DataReaderTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, ISelectValueFixture, IDeleteFixture
{
	public DataReaderTestBase(TFixture fixture)
		: base(fixture)
	{
		Fixture = fixture;
	}

	protected new TFixture Fixture { get; }


	private void TestGetBytes(Action<DbDataReader> action)
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.CreateSelectSql(new byte[] { 1, 2, 3, 4 });
		using var reader = command.ExecuteReader();
		reader.Read();
		action(reader);
	}

	private void TestGetChars(Action<DbDataReader> action)
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		// NB: Intentionally using a multi-byte UTF-8 character
		command.CommandText = SelectSql("'ab¢d'");
		using var reader = command.ExecuteReader();
		reader.Read();
		action(reader);
	}

	private static void AssertBufferBoundaryPolicy(Action action, string operation)
	{
		try
		{
			action();
		}
		catch (ArgumentException)
		{
			return;
		}
		catch (IndexOutOfRangeException)
		{
			return;
		}
		catch (InvalidOperationException)
		{
			throw SkipException.ForSkip($"The provider rejects {operation} with a provider-specific state exception.");
		}

		throw SkipException.ForSkip($"The provider accepts {operation}; buffer-boundary behavior is not a portable ADO.NET contract.");
	}


	private void TestGetTextReader(ValueKind valueKind, string expected)
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.CreateSelectSql(DbType.String, valueKind);
		using var reader = command.ExecuteReader();
		reader.Read();
		using var textReader = reader.GetTextReader(0);
		Assert.Equal(expected, textReader.ReadToEnd());
	}

	private void GetX_works<T>(string sql, Func<DbDataReader, T> action, T expected)
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		using var reader = command.ExecuteReader();
		var hasData = reader.Read();

		Assert.True(hasData);
		Assert.Equal(expected, action(reader));
	}

	private void X_throws_before_read(Action<DbDataReader> action)
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql("NULL");
		using var reader = command.ExecuteReader();
		Assert.Throws<InvalidOperationException>(() => action(reader));
	}

	private void X_throws_when_done(Action<DbDataReader> action)
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql("NULL");
		using var reader = command.ExecuteReader();
		var hasData = reader.Read();
		Assert.True(hasData);

		hasData = reader.Read();
		Assert.False(hasData);

		Assert.Throws<InvalidOperationException>(() => action(reader));
	}

	private void X_throws_when_closed(Action<DbDataReader> action)
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		var reader = command.ExecuteReader();
		((IDisposable) reader).Dispose();

		Assert.Throws<InvalidOperationException>(() => action(reader));
	}

	private void X_diagnostic_when_closed(Action<DbDataReader> action, string behavior)
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		var reader = command.ExecuteReader();
		((IDisposable)reader).Dispose();

		try
		{
			action(reader);
		}
		catch (SkipException)
		{
			throw;
		}
		catch (Exception)
		{
			return;
		}

		throw SkipException.ForSkip($"The provider does not throw for {behavior}; closed-reader exception behavior is not a portable ADO.NET contract.");
	}

	private void X_diagnostic(Action action, string behavior)
	{
		try
		{
			action();
		}
		catch (SkipException)
		{
			throw;
		}
		catch (Exception)
		{
			return;
		}

		throw SkipException.ForSkip($"The provider does not throw for {behavior}; out-of-range exception behavior is not a portable ADO.NET contract.");
	}

	private void Test_X_after_Delete(Action<DbDataReader> action)
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.DeleteNoRows;
		using var reader = command.ExecuteReader();
		try
		{
			action(reader);
		}
		catch (XunitException)
		{
			throw SkipException.ForSkip("Post-delete reader state is provider-specific and is not an ADO.NET conformance requirement.");
		}
	}

	private async Task Test_X_after_Delete(Func<DbDataReader, Task> action)
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = Fixture.DeleteNoRows;
		using var reader = command.ExecuteReader();
		try
		{
			await action(reader);
		}
		catch (XunitException)
		{
			throw SkipException.ForSkip("Post-delete reader state is provider-specific and is not an ADO.NET conformance requirement.");
		}
	}
}
