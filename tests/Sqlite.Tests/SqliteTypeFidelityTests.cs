using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Sqlite.Tests;

// An in-memory database, so the type tests never share temp.db with the other SQLite tests.
public class SqliteTypeFidelityFixture : ITypeFidelityFixture
{
	public DbProviderFactory Factory => SQLiteFactory.Instance;

	public string ConnectionString => "data source=:memory:";

	public IReadOnlyList<TypeSpec> Types => SqliteTypes.SystemDataSqlite;

	public IReadOnlyList<UncoveredType> UncoveredTypes => SqliteTypes.SystemDataSqliteUncovered;

	public string ParameterMarker(string name) => SqliteTypes.ParameterMarker(name);

	public string CreateTableSql(string table, string columnSql) => SqliteTypes.CreateTableSql(table, columnSql);

	public string DropTableSql(string table) => SqliteTypes.DropTableSql(table);
}

public sealed class SqliteTypeFidelityTests : TypeFidelityTestBase<SqliteTypeFidelityFixture>
{
	public SqliteTypeFidelityTests(SqliteTypeFidelityFixture fixture)
		: base(fixture)
	{
	}
}
