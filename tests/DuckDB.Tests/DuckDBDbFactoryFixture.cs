using System;
using System.Collections.Generic;
using System.Data.Common;
using DuckDB.NET.Data;
using pengdows.AdoNetApiTest.conformance;

// The database allows one writer at a time, so the test classes run one after another.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]

namespace DuckDB.Tests;

public sealed class DuckDBDbFactoryFixture : IDbFactoryFixture, ISingleWriterFixture, IDmlFixture, IParameterMarkerFixture, IDisposable
{
    public DbProviderFactory Factory => DuckDBClientFactory.Instance;

    public string ConnectionString => "Data Source=:memory:";

    /// <summary>DuckDB.NET binds named parameters written <c>$name</c>.</summary>
    public string ParameterMarker(string name) => "$" + name;

	public IReadOnlyList<string> DmlSetupSql => new[]
	{
		"CREATE TABLE people (id INTEGER, name VARCHAR)",
		"INSERT INTO people VALUES (1, 'Alice'), (2, 'Bob')"
	};

	public IReadOnlyList<string> DmlCleanupSql => new[] { "DROP TABLE IF EXISTS people" };

    public string DmlInsertSql => "INSERT INTO people (id, name) VALUES (3, 'Carol');";

    public string DmlMultiRowUpdateSql => "UPDATE people SET name = 'Updated' WHERE id > 0;";

    public string DmlUpdateSql => "UPDATE people SET name = 'Alicia' WHERE id = 1;";

    public string DmlUpdateNoRowsSql => "UPDATE people SET name = 'Nobody' WHERE id = 999;";

    public string DmlDeleteSql => "DELETE FROM people WHERE id = 2;";

    public string DmlDeleteNoRowsSql => "DELETE FROM people WHERE id = 999;";

    public void Dispose()
    {
    }
}
