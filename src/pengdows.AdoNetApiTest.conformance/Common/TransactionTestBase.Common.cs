using System;
using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public partial class TransactionTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.begintransaction", Because = "DbConnection only requires starting a transaction; whether parallel transactions are rejected is provider-specific")]
	public virtual void BeginTransaction_throws_when_parallel_transaction()
	{
		using var connection = CreateOpenConnection();
		using (connection.BeginTransaction())
		{
			try
			{
				using var parallel = connection.BeginTransaction();
			}
			catch (InvalidOperationException)
			{
				return;
			}

			ConformanceOutcome.Different("The provider permits parallel transactions; the base ADO.NET contract does not require rejection.");
			return;
		}
	}

	[CommonFact(Because = "the API requires a pending transaction but does not prescribe the exact exception type after disposal.")]
	public virtual void Commit_transaction_throws_after_Dispose()
	{
		using var connection = CreateOpenConnection();
		var transaction = connection.BeginTransaction();
		transaction.Dispose();
		try
		{
			transaction.Commit();
			SoftWarning.Report("The provider permits Commit after transaction disposal.");
		}
		catch (Exception ex)
		{
			SoftWarning.Report($"The provider throws {ex.GetType().Name} for Commit after transaction disposal.");
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.commit", Because = "the API requires a pending transaction but does not prescribe the exception type after completion")]
	public virtual void Commit_transaction_twice_throws()
	{
		using var connection = CreateOpenConnection();
		using var transaction = connection.BeginTransaction();
		Assert.Same(connection, transaction.Connection);
		transaction.Commit();
		Assert.ThrowsAny<Exception>(() => transaction.Commit());
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.rollback", Because = "the API requires a pending transaction but does not prescribe the exception type after completion")]
	public virtual void Commit_transaction_then_Rollback_throws()
	{
		using var connection = CreateOpenConnection();
		using var transaction = connection.BeginTransaction();
		Assert.Same(connection, transaction.Connection);
		transaction.Commit();
		Assert.ThrowsAny<Exception>(() => transaction.Rollback());
	}

	[CommonFact(Because = "the API requires a pending transaction but does not prescribe the exact exception type after disposal.")]
	public virtual void Rollback_transaction_throws_after_Dispose()
	{
		using var connection = CreateOpenConnection();
		var transaction = connection.BeginTransaction();
		transaction.Dispose();
		try
		{
			transaction.Rollback();
			SoftWarning.Report("The provider permits Rollback after transaction disposal.");
		}
		catch (Exception ex)
		{
			SoftWarning.Report($"The provider throws {ex.GetType().Name} for Rollback after transaction disposal.");
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.rollback", Because = "the API requires a pending transaction but does not prescribe the exception type after completion")]
	public virtual void Rollback_transaction_twice_throws()
	{
		using var connection = CreateOpenConnection();
		using var transaction = connection.BeginTransaction();
		Assert.Same(connection, transaction.Connection);
		transaction.Rollback();
		Assert.ThrowsAny<Exception>(() => transaction.Rollback());
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.commit", Because = "the API requires a pending transaction but does not prescribe the exception type after completion")]
	public virtual void Rollback_transaction_then_Commit_throws()
	{
		using var connection = CreateOpenConnection();
		using var transaction = connection.BeginTransaction();
		Assert.Same(connection, transaction.Connection);
		transaction.Rollback();
		Assert.ThrowsAny<Exception>(() => transaction.Commit());
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.begintransaction", Because = "No base-contract page lists an exception for BeginTransaction on a closed connection.")]
	public virtual void BeginTransaction_throws_when_closed()
	{
		using var connection = CreateConnection();
		Assert.ThrowsAny<InvalidOperationException>(() => connection.BeginTransaction());
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction", Because = "'no longer valid' on IDbTransaction.Connection is never defined as committed; the public DbTransaction.Connection page has no null clause.")]
	public virtual void Commit_transaction_clears_Connection()
	{
		using var connection = CreateOpenConnection();
		using var transaction = connection.BeginTransaction();
		Assert.Same(connection, transaction.Connection);
		transaction.Commit();
		Assert.Null(transaction.Connection);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction", Because = "'no longer valid' on IDbTransaction.Connection is never defined as rolled back; the public DbTransaction.Connection page has no null clause.")]
	public virtual void Rollback_transaction_clears_Connection()
	{
		using var connection = CreateOpenConnection();
		using var transaction = connection.BeginTransaction();
		Assert.Same(connection, transaction.Connection);
		transaction.Rollback();
		Assert.Null(transaction.Connection);
	}

#if !NETSTANDARD2_0
	/// <summary>
	/// CommitAsync must complete the same transaction state transition as Commit.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.commitasync.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.commitasync", Because = "'no longer valid' on IDbTransaction.Connection is never defined as committed.")]
	public virtual async Task CommitAsync_clears_Connection()
	{
		using var connection = CreateOpenConnection();
		using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

		await transaction.CommitAsync().ConfigureAwait(false);

		Assert.Null(((IDbTransaction)transaction).Connection);
	}
#endif

#if !NETSTANDARD2_0
	/// <summary>
	/// RollbackAsync must complete the same transaction state transition as
	/// Rollback.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.rollbackasync.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.rollbackasync", Because = "'no longer valid' on IDbTransaction.Connection is never defined as rolled back.")]
	public virtual async Task RollbackAsync_clears_Connection()
	{
		using var connection = CreateOpenConnection();
		using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

		await transaction.RollbackAsync().ConfigureAwait(false);

		Assert.Null(transaction.Connection);
	}
#endif

	/// <summary>
	/// Disposing a transaction makes it no longer valid, so its associated
	/// connection must no longer be exposed through Connection.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.system-data-idbtransaction-connection.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.system-data-idbtransaction-connection", Because = "The behavior of DbTransaction.Dispose is documented as provider specific.")]
	public virtual void Dispose_transaction_clears_Connection()
	{
		using var connection = CreateOpenConnection();
		var transaction = connection.BeginTransaction();

		transaction.Dispose();

		Assert.Null(((IDbTransaction)transaction).Connection);
	}

#if !NETSTANDARD2_0
	/// <summary>
	/// DisposeAsync must release a transaction and invalidate its associated
	/// connection reference just like synchronous disposal.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.disposeasync
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.system-data-idbtransaction-connection.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.disposeasync", Because = "The behavior of DbTransaction.Dispose is documented as provider specific.")]
	public virtual async Task DisposeAsync_clears_Connection()
	{
		using var connection = CreateOpenConnection();
		var transaction = connection.BeginTransaction();

		await transaction.DisposeAsync().ConfigureAwait(false);

		Assert.Null(((IDbTransaction)transaction).Connection);
	}
#endif
}
