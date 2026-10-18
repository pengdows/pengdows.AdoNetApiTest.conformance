using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlite8.Tests;

// An in-memory database, so the type tests never share temp.db with the other SQLite tests.
public class MicrosoftSqlite8TypeFidelityFixture : ITypeFidelityFixture
{
	public DbProviderFactory Factory => SqliteFactory.Instance;

	public string ConnectionString => "data source=:memory:";

	public IReadOnlyList<TypeSpec> Types => SqliteTypes.MicrosoftDataSqlite;

	public IReadOnlyList<UncoveredType> UncoveredTypes => new UncoveredType[0];

	public string ParameterMarker(string name) => SqliteTypes.ParameterMarker(name);

	public string CreateTableSql(string table, string columnSql) => SqliteTypes.CreateTableSql(table, columnSql);

	public string DropTableSql(string table) => SqliteTypes.DropTableSql(table);
}

public sealed class MicrosoftSqlite8TypeFidelityTests : TypeFidelityTestBase<MicrosoftSqlite8TypeFidelityFixture>
{
	public MicrosoftSqlite8TypeFidelityTests(MicrosoftSqlite8TypeFidelityFixture fixture)
		: base(fixture)
	{
	}
}
