using pengdows.AdoNetApiTest.conformance;

namespace MySqlData26.Tests;

/// <summary>MySQL renders a value as text with CAST(... AS CHAR); it has no CAST(... AS VARCHAR).</summary>
public class MySqlData26DataTypesFixture : MySqlData26DbFactoryFixture, IDataTypesFidelityFixture
{
	public string CreateTableSql(string table, string columnSql) => $"create table {table} (Id integer not null primary key, V {columnSql} null)";

	public string DropTableSql(string table) => $"drop table if exists {table}";

	public string RenderAsText => "CAST({0} AS CHAR)";
}

public sealed class MySqlData26DataTypesFidelityTests : DataTypesFidelityTestBase<MySqlData26DataTypesFixture>
{
	public MySqlData26DataTypesFidelityTests(MySqlData26DataTypesFixture fixture)
		: base(fixture)
	{
	}
}
