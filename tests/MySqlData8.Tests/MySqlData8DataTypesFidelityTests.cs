using pengdows.AdoNetApiTest.conformance;

namespace MySqlData8.Tests;

/// <summary>MySQL renders a value as text with CAST(... AS CHAR); it has no CAST(... AS VARCHAR).</summary>
public class MySqlData8DataTypesFixture : MySqlData8DbFactoryFixture, IDataTypesFidelityFixture
{
	public string CreateTableSql(string table, string columnSql) => $"create table {table} (Id integer not null primary key, V {columnSql} null)";

	public string DropTableSql(string table) => $"drop table if exists {table}";

	public string RenderAsText => "CAST({0} AS CHAR)";
}

public sealed class MySqlData8DataTypesFidelityTests : DataTypesFidelityTestBase<MySqlData8DataTypesFixture>
{
	public MySqlData8DataTypesFidelityTests(MySqlData8DataTypesFixture fixture)
		: base(fixture)
	{
	}
}
