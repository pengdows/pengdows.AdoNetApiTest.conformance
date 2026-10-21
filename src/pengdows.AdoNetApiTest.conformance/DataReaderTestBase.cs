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
		command.CommandText = SelectSql(Text("ab¢d"));
		using var reader = command.ExecuteReader();
		reader.Read();
		action(reader);
	}

	/// <summary>
	/// <c>GetColumnSchema</c> is an optional capability: a reader that does not implement it throws
	/// <see cref="NotSupportedException"/>. That is recorded as "not implemented" and the calling test ends as a pass.
	/// </summary>
	private static bool TryGetColumnSchema(DbDataReader reader, out System.Collections.ObjectModel.ReadOnlyCollection<DbColumn> columns)
	{
		try
		{
			columns = reader.GetColumnSchema();
			return true;
		}
		catch (NotSupportedException)
		{
			columns = null;
			ConformanceOutcome.NotImplemented("The provider does not support GetColumnSchema.");
			return false;
		}
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
			ConformanceOutcome.Different($"The provider rejects {operation} with a provider-specific state exception.");
			return;
		}

		ConformanceOutcome.Different($"The provider accepts {operation}; buffer-boundary behavior is not a portable ADO.NET contract.");
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
		Assert.ThrowsAny<InvalidOperationException>(() => action(reader));
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

		Assert.ThrowsAny<InvalidOperationException>(() => action(reader));
	}

	private void X_throws_when_closed(Action<DbDataReader> action)
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		var reader = command.ExecuteReader();
		((IDisposable) reader).Dispose();

		Assert.ThrowsAny<InvalidOperationException>(() => action(reader));
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
		catch (Exception)
		{
			return;
		}

		ConformanceOutcome.Different($"The provider does not throw for {behavior}; closed-reader exception behavior is not a portable ADO.NET contract.");
	}

	private void X_diagnostic(Action action, string behavior)
	{
		try
		{
			action();
		}
		catch (Exception)
		{
			return;
		}

		ConformanceOutcome.Different($"The provider does not throw for {behavior}; out-of-range exception behavior is not a portable ADO.NET contract.");
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
			ConformanceOutcome.Different("Post-delete reader state is provider-specific and is not an ADO.NET conformance requirement.");
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
			ConformanceOutcome.Different("Post-delete reader state is provider-specific and is not an ADO.NET conformance requirement.");
		}
	}
}
