using System;
using System.Data;
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
		using var transaction = connection.BeginTransaction();
		const string savepoint = "adonet_specification_savepoint";

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
		await using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);
		const string savepoint = "adonet_specification_savepoint_async";

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
#endif
#endif
}
