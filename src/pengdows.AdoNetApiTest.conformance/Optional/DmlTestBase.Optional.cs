using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class DmlTestBase<TFixture>
	where TFixture : class, IDmlFixture
{
	/// <summary>
	/// A prepared INSERT that returns its row is executed again with new parameter values, the reason to prepare it, and
	/// the connection stays usable. MySqlConnector 2.3.0 through 2.6.0 on MariaDB fail the second execution ("Expected
	/// result set to have 0 columns, but it contains 1 columns") and close the connection; 2.6.1 fixed it. One value is
	/// DBNull.Value, as a write of a null column sends it.
	/// See https://learn.microsoft.com/dotnet/api/system.data.idbcommand.prepare.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idbcommand.prepare")]
	public virtual void Prepared_INSERT_with_RETURNING_executes_again_with_new_parameter_values()
		=> WithReturning(f => f.DmlInsertReturningParameterSql, (connection, sql) =>
		{
			using (var command = connection.CreateCommand())
			{
				var id = command.CreateParameter();
				id.ParameterName = "id";
				id.DbType = DbType.Int32;
				var value = command.CreateParameter();
				value.ParameterName = "v";
				value.DbType = DbType.String;
				command.Parameters.Add(id);
				command.Parameters.Add(value);
				command.CommandText = string.Format(CultureInfo.InvariantCulture, sql,
					MakeParameterName(connection, id), MakeParameterName(connection, value));

				id.Value = 3;
				value.Value = "Carol";
				try
				{
					command.Prepare();
				}
				catch (NotSupportedException)
				{
					ConformanceOutcome.NotImplemented("The provider does not support prepared commands.");
					return;
				}

				Assert.Equal(1, ReadAll(command));
				id.Value = 4;
				value.Value = DBNull.Value;
				Assert.Equal(1, ReadAll(command));
				id.Value = 5;
				value.Value = "Eve";
				Assert.Equal(1, ReadAll(command));
			}

			Assert.Equal(ConnectionState.Open, connection.State);
			Assert.Equal(1, Execute(connection, Fixture.DmlUpdateSql));
		});

	/// <summary>Executes <paramref name="command"/> as a reader and returns the rows of every result.</summary>
	private static int ReadAll(DbCommand command)
	{
		using var reader = command.ExecuteReader();
		var rows = 0;
		do
		{
			while (reader.Read())
				rows++;
		}
		while (reader.NextResult());
		return rows;
	}
}
