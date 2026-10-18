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

	private static async Task<int> ExecuteAsync(DbConnection connection, string sql)
	{
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
	}
}
