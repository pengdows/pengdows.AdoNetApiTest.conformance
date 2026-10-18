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

			throw Xunit.Sdk.SkipException.ForSkip("The provider permits parallel transactions; the base ADO.NET contract does not require rejection.");
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
}
