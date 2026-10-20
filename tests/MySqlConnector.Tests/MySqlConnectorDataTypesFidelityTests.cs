using pengdows.AdoNetApiTest.conformance;

namespace MySqlConnector.Tests;

/// <summary>MySQL renders a value as text with CAST(... AS CHAR); it has no CAST(... AS VARCHAR).</summary>
public class MySqlConnectorDataTypesFixture : MySqlConnectorDbFactoryFixture, IDataTypesFidelityFixture
{
	public string CreateTableSql(string table, string columnSql) => $"create table {table} (Id integer not null primary key, V {columnSql} null)";

	public string DropTableSql(string table) => $"drop table if exists {table}";

	public string RenderAsText => "CAST({0} AS CHAR)";
}

public sealed class MySqlConnectorDataTypesFidelityTests : DataTypesFidelityTestBase<MySqlConnectorDataTypesFixture>
{
	public MySqlConnectorDataTypesFidelityTests(MySqlConnectorDataTypesFixture fixture)
		: base(fixture)
	{
	}
}
