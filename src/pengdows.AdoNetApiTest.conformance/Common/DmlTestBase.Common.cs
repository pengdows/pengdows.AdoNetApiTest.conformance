using System;
using System.Data;
using System.Data.Common;
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

	/// <summary>
	/// A <see cref="char"/> written to a text column is stored as that character: the configuring-parameters table infers
	/// <see cref="DbType.StringFixedLength"/> for it. System.Data.SQLite 1.0.119 stored '~' as the integer 126 and MySql.Data
	/// 9.7 as "System.Byte[]".
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "The table infers a DbType for Char; no page says what a provider must store for it.")]
	public virtual void Text_column_stores_a_char_as_that_character()
		=> WithStoredText(parameter => parameter.Value = '~', stored => Assert.Equal("~", stored));

	/// <summary>
	/// Size's default "is inferred from the parameter value", and a new parameter's Size is 0, so assigning 0 explicitly (as
	/// a pooled parameter is reset) still stores the whole value. Microsoft.Data.Sqlite stored "" (dotnet/efcore#39377).
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.size.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.size", Because = "No page says what an explicit Size of 0 means; the default is inferred from the value.")]
	public virtual void Text_column_stores_the_whole_string_when_Size_is_zero()
		=> WithStoredText(parameter =>
		{
			parameter.Value = "Carol";
			parameter.Size = 0;
		}, stored => Assert.Equal("Carol", stored));

	/// <summary>
	/// A value a provider turns into text for a string parameter is the same text whatever the current culture, or the same
	/// row is stored differently on two machines (SqlClient, ODP.NET and MySql.Data stored "02.01.2024 13:04:05" under
	/// de-DE). A provider may refuse the value instead.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.dbtype.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.dbtype", Because = "No page says how a provider converts a value to its DbType.")]
	public virtual void Text_column_stores_converted_text_independently_of_the_current_culture()
	{
		var stored = new System.Collections.Generic.List<string>();
		foreach (var culture in new[] { "en-US", "de-DE", "ar-SA" })
		{
			var previous = System.Globalization.CultureInfo.CurrentCulture;
			System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(culture);
			try
			{
				var refused = false;
				WithStoredText(parameter =>
				{
					parameter.DbType = DbType.String;
					parameter.Value = new DateTime(2024, 1, 2, 13, 4, 5);
				}, value => stored.Add(value), _ => refused = true);
				if (refused)
				{
					ConformanceOutcome.Different($"The provider refused the DateTime-to-text conversion under {culture}.");
					return;
				}
			}
			finally
			{
				System.Globalization.CultureInfo.CurrentCulture = previous;
			}
		}

		Assert.All(stored, s => Assert.Equal(stored[0], s));
	}

	/// <summary>
	/// "When you send a null value as a Parameter value in a command to the database, you cannot use null ... Instead you
	/// need to use DBNull.Value." A prepared INSERT stores that NULL whatever DbType the parameter declares. MySqlConnector
	/// 2.5.0 through 2.6.2 map DbType.Object to VECTOR and send a prepared NULL as that type, which MySQL 8.4 rejects with
	/// "Incorrect arguments to mysqld_stmt_execute" (mysql-net/MySqlConnector#1679); unprepared, the same NULL is accepted.
	/// A DbType the provider refuses to assign is left to <c>DbType_accepts_every_DbType_value</c>.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "Prepare is optional, and no page says a prepared NULL must be accepted for every DbType.")]
	public virtual void Prepared_insert_stores_NULL_for_every_DbType()
	{
		if (Fixture is not IDmlParameterFixture parameters)
		{
			ConformanceOutcome.NotApplicable("the fixture states no parameterized DML for its table");
			return;
		}

		WithDmlState(connection =>
		{
			var failures = new System.Collections.Generic.List<string>();
			var id = 100;
			foreach (DbType dbType in Enum.GetValues(typeof(DbType)))
			{
				id++;
				using var command = connection.CreateCommand();
				var key = command.CreateParameter();
				key.ParameterName = "id";
				key.DbType = DbType.Int32;
				key.Value = id;
				var value = command.CreateParameter();
				value.ParameterName = "v";
				try
				{
					value.DbType = dbType;
				}
				catch (Exception exception) when (!(exception is Xunit.Sdk.XunitException))
				{
					continue;
				}

				value.Value = DBNull.Value;
				// SqlClient's Prepare requires an explicit Size on variable-length parameters and Precision and Scale on decimals;
				// none of them changes a NULL.
				value.Size = 1;
				((IDbDataParameter)value).Precision = 1;
				((IDbDataParameter)value).Scale = 0;
				command.Parameters.Add(key);
				command.Parameters.Add(value);
				command.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture, parameters.DmlInsertParameterSql,
					MakeParameterName(connection, key), MakeParameterName(connection, value));
				try
				{
					command.Prepare();
				}
				catch (NotSupportedException)
				{
					ConformanceOutcome.NotImplemented("The provider does not support prepared commands.");
					return;
				}
				catch (Exception exception) when (!(exception is Xunit.Sdk.XunitException))
				{
					// The provider's own rule for preparing this parameter shape, not a NULL it failed to store.
					continue;
				}

				try
				{
					command.ExecuteNonQuery();
				}
				catch (Exception exception) when (!(exception is Xunit.Sdk.XunitException))
				{
					failures.Add($"{dbType}: {exception.GetType().Name}: {exception.Message.Split('\n')[0]}");
					continue;
				}

				using var select = connection.CreateCommand();
				select.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture, parameters.DmlSelectValueSql, id);
				var stored = select.ExecuteScalar();
				if (stored is not null and not DBNull)
				{
					failures.Add($"{dbType}: stored {stored} instead of NULL");
				}
			}

			Assert.True(failures.Count == 0, "A prepared NULL was not stored for: " + string.Join("; ", failures));
		});
	}

	/// <summary>
	/// Inserts one row whose text column is a parameter set by <paramref name="set"/>, then hands the stored text to
	/// <paramref name="check"/>. A provider that refuses the value reports it through <paramref name="refused"/>, or as a
	/// difference; a fixture without parameterized DML makes the test not applicable.
	/// </summary>
	private void WithStoredText(Action<DbParameter> set, Action<string> check, Action<Exception> refused = null)
	{
		if (Fixture is not IDmlParameterFixture parameters)
		{
			ConformanceOutcome.NotApplicable("the fixture states no parameterized DML for its table");
			return;
		}

		WithDmlState(connection =>
		{
			using (var command = connection.CreateCommand())
			{
				var id = command.CreateParameter();
				id.ParameterName = "id";
				id.DbType = DbType.Int32;
				id.Value = 3;
				var value = command.CreateParameter();
				value.ParameterName = "v";
				set(value);
				command.Parameters.Add(id);
				command.Parameters.Add(value);
				command.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture, parameters.DmlInsertParameterSql,
					MakeParameterName(connection, id), MakeParameterName(connection, value));
				try
				{
					command.ExecuteNonQuery();
				}
				catch (Exception exception) when (!(exception is Xunit.Sdk.XunitException))
				{
					if (refused is null)
					{
						ConformanceOutcome.Different("The provider refuses the value: " + exception.GetType().Name + ": " + exception.Message);
					}
					else
					{
						refused(exception);
					}

					return;
				}
			}

			using var select = connection.CreateCommand();
			select.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture, parameters.DmlSelectValueSql, 3);
			check(Convert.ToString(select.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture));
		});
	}
}
