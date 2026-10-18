using System;
using System.Data.Common;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class TransactionSemanticsTestBase<TFixture>
	where TFixture : class, ITransactionSemanticsFixture
{
	/// <summary>
	/// Disposing an active transaction without committing must roll back its
	/// changes. The SQL and row identity are fixture-owned; the transaction
	/// lifecycle and result are the shared contract under test.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.dispose
	/// and https://learn.microsoft.com/dotnet/framework/data/adonet/local-transactions.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction")]
	public virtual void Dispose_without_commit_rolls_back_changes()
	{
		using var connection = CreateOpenConnection();
		Fixture.SetupTransactionSemantics(connection);

		try
		{
			using (var transaction = connection.BeginTransaction())
			using (var command = connection.CreateCommand())
			{
				command.Transaction = transaction;
				command.CommandText = Fixture.TransactionalInsertSql;
				command.ExecuteNonQuery();
			}

			using var verification = connection.CreateCommand();
			verification.CommandText = Fixture.TransactionalRowCountSql;
			Assert.Equal(0, Convert.ToInt32(verification.ExecuteScalar()));
		}
		finally
		{
			Fixture.CleanupTransactionSemantics(connection);
		}
	}
}
