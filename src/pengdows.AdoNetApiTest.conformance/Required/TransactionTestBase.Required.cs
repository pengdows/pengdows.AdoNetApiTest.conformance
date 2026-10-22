using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public partial class TransactionTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction")]
	public virtual void BeginTransaction_works()
	{
		using var connection = CreateOpenConnection();
		using var transaction = connection.BeginTransaction();
		Assert.NotNull(transaction);
		Assert.Same(connection, transaction.Connection);
	}

#if !NETSTANDARD2_0
	/// <summary>
	/// BeginTransactionAsync is required to provide the same transaction contract
	/// as BeginTransaction. The base implementation may delegate to synchronous
	/// transaction creation, so this test validates the result rather than requiring
	/// a particular I/O strategy.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.begintransactionasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.begintransactionasync")]
	public virtual async Task BeginTransactionAsync_works()
	{
		using var connection = CreateOpenConnection();
		using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

		Assert.NotNull(transaction);
		Assert.Same(connection, transaction.Connection);
	}
#endif




#if NET10_0_OR_GREATER
	/// <summary>
	/// SupportsSavepoints is the capability agreement for the synchronous
	/// savepoint methods. A provider that reports false must reject each method;
	/// a provider that reports true must implement the complete savepoint surface.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.supportssavepoints
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.save.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.supportssavepoints")]
	public virtual void Savepoint_methods_match_support_flag()
	{
		using var connection = CreateOpenConnection();
		const string savepoint = "adonet_specification_savepoint";

		using (var transaction = connection.BeginTransaction())
		{
			if (!transaction.SupportsSavepoints)
			{
				Assert.ThrowsAny<NotSupportedException>(() => transaction.Save(savepoint));
				Assert.ThrowsAny<NotSupportedException>(() => transaction.Rollback(savepoint));
				Assert.ThrowsAny<NotSupportedException>(() => transaction.Release(savepoint));
				return;
			}

			transaction.Save(savepoint);
			transaction.Rollback(savepoint);
			transaction.Save(savepoint);
			transaction.Release(savepoint);
		}

		VerifySavepointRollbackRestoresState(connection);
	}
#endif

#if NET10_0_OR_GREATER
#if !NETSTANDARD2_0
	/// <summary>
	/// The asynchronous savepoint methods must obey the same capability contract
	/// as their synchronous counterparts.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.saveasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.saveasync")]
	public virtual async Task Savepoint_async_methods_match_support_flag()
	{
		using var connection = CreateOpenConnection();
		const string savepoint = "adonet_specification_savepoint_async";

		await using (var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false))
		{
			if (!transaction.SupportsSavepoints)
			{
				await Assert.ThrowsAnyAsync<NotSupportedException>(() => transaction.SaveAsync(savepoint)).ConfigureAwait(false);
				await Assert.ThrowsAnyAsync<NotSupportedException>(() => transaction.RollbackAsync(savepoint)).ConfigureAwait(false);
				await Assert.ThrowsAnyAsync<NotSupportedException>(() => transaction.ReleaseAsync(savepoint)).ConfigureAwait(false);
				return;
			}

			await transaction.SaveAsync(savepoint).ConfigureAwait(false);
			await transaction.RollbackAsync(savepoint).ConfigureAwait(false);
			await transaction.SaveAsync(savepoint).ConfigureAwait(false);
			await transaction.ReleaseAsync(savepoint).ConfigureAwait(false);
		}

		await VerifySavepointRollbackRestoresStateAsync(connection).ConfigureAwait(false);
	}
#endif
#endif

#if NET10_0_OR_GREATER

	private void VerifySavepointRollbackRestoresState(DbConnection connection)
	{
		if (Fixture is not ISavepointSemanticsFixture semantics)
		{
			return;
		}

		foreach (var sql in semantics.SavepointSetupSql)
		{
			Execute(connection, null, sql);
		}

		try
		{
			using (var transaction = connection.BeginTransaction())
			{
				Execute(connection, transaction, semantics.SavepointFirstInsertSql);
				transaction.Save("adonet_semantic_savepoint");
				Execute(connection, transaction, semantics.SavepointSecondInsertSql);
				transaction.Rollback("adonet_semantic_savepoint");
				transaction.Commit();
			}

			Assert.Equal(1, Convert.ToInt32(ExecuteScalar(connection, semantics.SavepointRowCountSql)));
		}
		finally
		{
			foreach (var sql in semantics.SavepointCleanupSql)
			{
				Execute(connection, null, sql);
			}
		}
	}

	private async Task VerifySavepointRollbackRestoresStateAsync(DbConnection connection)
	{
		if (Fixture is not ISavepointSemanticsFixture semantics)
		{
			return;
		}

		foreach (var sql in semantics.SavepointSetupSql)
		{
			await ExecuteAsync(connection, null, sql).ConfigureAwait(false);
		}

		try
		{
			await using (var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false))
			{
				await ExecuteAsync(connection, transaction, semantics.SavepointFirstInsertSql).ConfigureAwait(false);
				await transaction.SaveAsync("adonet_semantic_savepoint_async").ConfigureAwait(false);
				await ExecuteAsync(connection, transaction, semantics.SavepointSecondInsertSql).ConfigureAwait(false);
				await transaction.RollbackAsync("adonet_semantic_savepoint_async").ConfigureAwait(false);
				await transaction.CommitAsync().ConfigureAwait(false);
			}

			Assert.Equal(1, Convert.ToInt32(await ExecuteScalarAsync(connection, semantics.SavepointRowCountSql).ConfigureAwait(false)));
		}
		finally
		{
			foreach (var sql in semantics.SavepointCleanupSql)
			{
				await ExecuteAsync(connection, null, sql).ConfigureAwait(false);
			}
		}
	}

	private static void Execute(DbConnection connection, DbTransaction transaction, string sql)
	{
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		command.Transaction = transaction;
		command.ExecuteNonQuery();
	}

	private static object ExecuteScalar(DbConnection connection, string sql)
	{
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		return command.ExecuteScalar();
	}

	private static async Task ExecuteAsync(DbConnection connection, DbTransaction transaction, string sql)
	{
		await using var command = connection.CreateCommand();
		command.CommandText = sql;
		command.Transaction = transaction;
		await command.ExecuteNonQueryAsync().ConfigureAwait(false);
	}

	private static async Task<object> ExecuteScalarAsync(DbConnection connection, string sql)
	{
		await using var command = connection.CreateCommand();
		command.CommandText = sql;
		return await command.ExecuteScalarAsync().ConfigureAwait(false);
	}
#endif
}
