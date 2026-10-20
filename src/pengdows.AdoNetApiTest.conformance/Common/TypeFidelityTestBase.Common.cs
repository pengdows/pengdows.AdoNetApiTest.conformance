using System;
using System.Data.Common;
using System.Linq;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class TypeFidelityTestBase<TFixture>
	where TFixture : class, ITypeFidelityFixture
{
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getvalue", Because = "value fidelity per type is an interoperability expectation; the contract does not state it")]
	public virtual void Types_are_read_back_exactly_from_literals() => Run(Paths.Read);

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.value", Because = "value fidelity per type is an interoperability expectation; the contract does not state it")]
	public virtual void Types_are_written_exactly_by_parameters() => Run(Paths.Write);

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldvalue", Because = "value fidelity per type is an interoperability expectation; the contract does not state it")]
	public virtual void Types_round_trip_exactly_through_parameters() => Run(Paths.RoundTrip);

	/// <summary>
	/// The same query text runs again after its table is dropped and recreated with a different column type. ODP.NET's
	/// statement cache keeps the old cursor and fails with ORA-24449 until the connection is closed.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executereader.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executereader", Because = "no page says a provider's statement cache must notice a schema change")]
	public virtual void Query_runs_after_its_table_is_recreated()
	{
		var specs = Fixture.Types.Where(spec => spec.Values.Any(value => value.Expected is not null))
			.GroupBy(spec => spec.ClrType).Select(group => group.First()).Take(3).ToList();
		var current = specs[0];
		var table = TablePrefix + "_recreated";
		using var connection = CreateOpenConnection();
		try
		{
			foreach (var spec in specs)
			{
				current = spec;
				var value = spec.Values.First(v => v.Expected is not null);
				Execute(connection, DropTableSql(table));
				Execute(connection, CreateTableSql(table, spec.ColumnSql));
				Execute(connection, $"insert into {table} (Id, V) values (1, {value.Literal})");

				// Several runs, as an application makes: a statement cache may keep a statement only once it repeats.
				for (var run = 0; run < 10; run++)
				{
					using var command = connection.CreateCommand();
					command.CommandText = $"select V from {table} where Id = 1";
					using var reader = command.ExecuteReader();
					Assert.True(reader.Read());
					_ = reader.GetValue(0);
				}
			}
		}
		catch (DbException ex)
		{
			ConformanceOutcome.Different($"Re-running a query after its table was recreated as {current.ColumnSql} threw {ex.GetType().Name}: {FirstLine(ex.Message)}");
		}
		finally
		{
			try
			{
				using var cleanup = CreateOpenConnection();
				Execute(cleanup, DropTableSql(table));
			}
			catch (DbException)
			{
			}
		}
	}
}
