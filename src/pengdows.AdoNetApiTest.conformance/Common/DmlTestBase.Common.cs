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
