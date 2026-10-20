using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class DmlTestBase<TFixture>
	where TFixture : class, IDmlFixture
{
	/// <summary>
	/// MERGE changes rows, yet ExecuteNonQuery names only "UPDATE, INSERT, and DELETE statements" and gives -1 "for all
	/// other types of statements". Reporting the rows MERGE changed is what RecordsAffected describes and what Npgsql and
	/// SqlClient do, so -1 is a difference, not a failure.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery", Because = "ExecuteNonQuery names only INSERT, UPDATE and DELETE; no page states the count for MERGE.")]
	public virtual void ExecuteNonQuery_returns_changed_row_count_for_MERGE()
		=> WithMerge((connection, sql) => ExpectMergeCount(Execute(connection, sql)));

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.recordsaffected", Because = "RecordsAffected counts rows changed, inserted, or deleted; no page names MERGE.")]
	public virtual void ExecuteReader_returns_affected_rows_after_close_for_MERGE()
		=> WithMerge((connection, sql) => ExpectMergeCount(ReadAllAndClose(connection, sql).RecordsAffected));

	private static void ExpectMergeCount(int count)
	{
		if (count == -1)
		{
			ConformanceOutcome.Different("MERGE updated one row and inserted one, and the provider reported -1, as for a statement that changes no rows");
			return;
		}

		Assert.Equal(2, count);
	}
}
