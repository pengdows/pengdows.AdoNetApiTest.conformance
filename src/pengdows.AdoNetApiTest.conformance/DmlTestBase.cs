using System;
using System.Data.Common;
using System.Threading.Tasks;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Tests the ADO.NET affected-row contract for data-modification commands.
///
/// This covers the behavior relied upon by optimistic concurrency and write
/// success detection. It is deliberately separate from the existing SELECT-only
/// ExecuteNonQuery test.
/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery.
/// </summary>
public abstract partial class DmlTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IDmlFixture
{
	protected DmlTestBase(TFixture fixture)
		: base(fixture)
	{
	}

	private static int Execute(DbConnection connection, string sql)
	{
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		return command.ExecuteNonQuery();
	}

	/// <summary>
	/// Fixture setup and cleanup may contain multiple provider-specific statements.
	/// Execute them separately because ADO.NET does not require providers to support
	/// multi-statement prepares; Informix, for example, rejects them. Cleanup is
	/// best-effort so it cannot replace the setup or assertion failure.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.prepare.
	/// </summary>
	private void WithDmlState(Action<DbConnection> action)
	{
		using var connection = CreateOpenConnection();
		try
		{
			ExecuteScript(connection, Fixture.DmlSetupSql);
			action(connection);
		}
		finally
		{
			TryExecuteScript(connection, Fixture.DmlCleanupSql);
		}
	}

	private async Task WithDmlStateAsync(Func<DbConnection, Task> action)
	{
		using var connection = CreateOpenConnection();
		try
		{
			ExecuteScript(connection, Fixture.DmlSetupSql);
			await action(connection).ConfigureAwait(false);
		}
		finally
		{
			TryExecuteScript(connection, Fixture.DmlCleanupSql);
		}
	}

	private static void ExecuteScript(DbConnection connection, System.Collections.Generic.IEnumerable<string> statements)
	{
		foreach (var statement in statements)
		{
			if (!string.IsNullOrWhiteSpace(statement))
				Execute(connection, statement);
		}
	}

	private static void TryExecuteScript(DbConnection connection, System.Collections.Generic.IEnumerable<string> statements)
	{
		try
		{
			ExecuteScript(connection, statements);
		}
		catch (Exception)
		{
			// Cleanup must not replace the assertion or setup exception.
		}
	}

	/// <summary>Runs <paramref name="sql"/> through ExecuteReader, reads every row of every result, closes the reader, and returns the rows read and RecordsAffected.</summary>
	private static (int Rows, int RecordsAffected) ReadAllAndClose(DbConnection connection, string sql)
	{
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		using var reader = command.ExecuteReader();
		var rows = 0;
		do
		{
			while (reader.Read())
				rows++;
		}
		while (reader.NextResult());
		reader.Close();
		return (rows, reader.RecordsAffected);
	}

	/// <summary>Runs <paramref name="action"/> over the fixture's RETURNING statement, or reports the test not applicable when the database has none.</summary>
	private void WithReturning(Func<IDmlReturningFixture, string> statement, Action<DbConnection, string> action)
	{
		var sql = Fixture is IDmlReturningFixture returning ? statement(returning) : null;
		if (sql is null)
		{
			ConformanceOutcome.NotApplicable("the fixture states no DML that returns rows (RETURNING, OUTPUT) for this database");
			return;
		}

		WithDmlState(connection => action(connection, sql));
	}

	/// <summary>
	/// Runs <paramref name="action"/> over the fixture's MERGE. A database that rejects the statement has no SQL:2003
	/// MERGE, so the test is not applicable; the rejection is reported, never hidden.
	/// </summary>
	private void WithMerge(Action<DbConnection, string> action)
	{
		if (Fixture is not IMergeFixture merge || merge.DmlMergeSql is null)
		{
			ConformanceOutcome.NotApplicable("the fixture states no MERGE for this database");
			return;
		}

		WithDmlState(connection =>
		{
			try
			{
				action(connection, merge.DmlMergeSql);
			}
			catch (DbException e)
			{
				ConformanceOutcome.NotApplicable("the database rejected the SQL:2003 MERGE: " + e.Message);
			}
		});
	}

	private static async Task<int> ExecuteAsync(DbConnection connection, string sql)
	{
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
	}
}
