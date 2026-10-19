using pengdows.AdoNetApiTest.conformance;

namespace MySqlData9.Tests;

/// <summary>MySQL renders a value as text with CAST(... AS CHAR); it has no CAST(... AS VARCHAR).</summary>
public class MySqlData9DataTypesFixture : MySqlData9DbFactoryFixture, IDataTypesFidelityFixture
{
	public string CreateTableSql(string table, string columnSql) => $"create table {table} (Id integer not null primary key, V {columnSql} null)";

	public string DropTableSql(string table) => $"drop table if exists {table}";

	public string RenderAsText => "CAST({0} AS CHAR)";
}

public sealed class MySqlData9DataTypesFidelityTests : DataTypesFidelityTestBase<MySqlData9DataTypesFixture>
{
	public MySqlData9DataTypesFidelityTests(MySqlData9DataTypesFixture fixture)
		: base(fixture)
	{
	}
}
