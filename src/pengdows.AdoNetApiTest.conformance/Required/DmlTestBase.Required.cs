using System;
using System.Data.Common;
using System.Threading.Tasks;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class DmlTestBase<TFixture>
	where TFixture : class, IDmlFixture
{
	/// <summary>
	/// ADO.NET requires INSERT, UPDATE, and DELETE to return their affected-row
	/// count, including zero when the predicate matches no rows.
	/// This is the contract consumed by optimistic-concurrency code: a zero count
	/// means that no row matched the write predicate.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery")]
	public virtual void ExecuteNonQuery_returns_multirow_affected_count_for_DML()
		=> WithDmlState(connection => Assert.Equal(2, Execute(connection, Fixture.DmlMultiRowUpdateSql)));

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery")]
	public virtual void ExecuteNonQuery_returns_inserted_row_count()
		=> WithDmlState(connection => Assert.Equal(1, Execute(connection, Fixture.DmlInsertSql)));

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery")]
	public virtual void ExecuteNonQuery_returns_updated_row_count()
		=> WithDmlState(connection => Assert.Equal(1, Execute(connection, Fixture.DmlUpdateSql)));

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery")]
	public virtual void ExecuteNonQuery_returns_zero_when_update_matches_no_rows()
		=> WithDmlState(connection => Assert.Equal(0, Execute(connection, Fixture.DmlUpdateNoRowsSql)));

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery")]
	public virtual void ExecuteNonQuery_returns_deleted_row_count()
		=> WithDmlState(connection => Assert.Equal(1, Execute(connection, Fixture.DmlDeleteSql)));

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery")]
	public virtual void ExecuteNonQuery_returns_zero_when_delete_matches_no_rows()
		=> WithDmlState(connection => Assert.Equal(0, Execute(connection, Fixture.DmlDeleteNoRowsSql)));

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery")]
	public virtual async Task ExecuteNonQueryAsync_returns_affected_rows_for_DML()
		=> await WithDmlStateAsync(async connection =>
			Assert.Equal(1, await ExecuteAsync(connection, Fixture.DmlUpdateSql).ConfigureAwait(false))).ConfigureAwait(false);

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.recordsaffected")]
	public virtual void ExecuteReader_returns_affected_rows_after_close_for_DML()
		=> WithDmlState(connection =>
		{
			using var command = connection.CreateCommand();
			command.CommandText = Fixture.DmlDeleteSql;
			using var reader = command.ExecuteReader();
			reader.Close();
			Assert.Equal(1, reader.RecordsAffected);
		});

	/// <summary>
	/// An INSERT that returns the row it inserted is still an INSERT: "For UPDATE, INSERT, and DELETE statements, the
	/// return value is the number of rows affected by the command." A count of 0 would read as "no row was written".
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery")]
	public virtual void ExecuteNonQuery_returns_inserted_row_count_with_RETURNING()
		=> WithReturning(f => f.DmlInsertReturningSql, (connection, sql) => Assert.Equal(1, Execute(connection, sql)));

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery")]
	public virtual void ExecuteNonQuery_returns_deleted_row_count_with_RETURNING()
		=> WithReturning(f => f.DmlDeleteReturningSql, (connection, sql) => Assert.Equal(2, Execute(connection, sql)));

	/// <summary>
	/// RecordsAffected is "the number of rows changed, inserted, or deleted", whether or not the statement also returns
	/// the rows; -1 is for SELECT statements only.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.recordsaffected.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.recordsaffected")]
	public virtual void ExecuteReader_returns_affected_rows_after_close_with_RETURNING()
		=> WithReturning(f => f.DmlDeleteReturningSql, (connection, sql) =>
		{
			var (rows, recordsAffected) = ReadAllAndClose(connection, sql);
			Assert.Equal(2, rows);
			Assert.Equal(2, recordsAffected);
		});
}
