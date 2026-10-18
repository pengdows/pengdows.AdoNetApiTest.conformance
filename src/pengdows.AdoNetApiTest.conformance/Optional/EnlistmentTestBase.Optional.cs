using System;
using System.Data.Common;
using System.Transactions;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class EnlistmentTestBase<TFixture>
	where TFixture : class, IEnlistmentFixture
{
	/// <summary>
	/// A connection explicitly enlisted in a committable transaction must commit
	/// its work when that transaction commits. SQL and row identity are fixture
	/// responsibilities; enlistment and commit behavior are the shared contract.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.enlisttransaction.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.enlisttransaction")]
	public virtual void EnlistTransaction_commits_connection_work()
	{
		using var connection = CreateOpenConnection();
		Fixture.SetupEnlistment(connection);

		try
		{
			using var transaction = new CommittableTransaction();
			connection.EnlistTransaction(transaction);

			using (var command = connection.CreateCommand())
			{
				command.CommandText = Fixture.EnlistmentInsertSql;
				command.ExecuteNonQuery();
			}

			transaction.Commit();

			using var verification = connection.CreateCommand();
			verification.CommandText = Fixture.EnlistmentRowCountSql;
			Assert.Equal(1, Convert.ToInt32(verification.ExecuteScalar()));
		}
		finally
		{
			Fixture.CleanupEnlistment(connection);
		}
	}
}
