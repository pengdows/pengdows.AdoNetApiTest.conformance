using System;
using System.Collections.Generic;
using System.Data.Common;

namespace pengdows.AdoNetApiTest.conformance.Databases;

/// <summary>
/// SQL-92 statements for the DML tests over a table of their own: two rows to start, each statement a single row
/// insert or a plain UPDATE/DELETE with no terminator, so every SQL database accepts them unchanged. The fixture creates
/// the table once and each test only resets its rows; DDL per test deadlocks Firebird's metadata under parallel test
/// classes and makes YugabyteDB restart reads.
/// </summary>
public sealed class Sql92Dml
{
	public Sql92Dml(string table, Func<string, string, string> createTable, Func<string, string> dropTable)
	{
		CreateTableSql = createTable(table, "varchar(100)");
		DropTableSql = dropTable(table);
		SetupSql = new[]
		{
			$"DELETE FROM {table}",
			$"INSERT INTO {table} (Id, V) VALUES (1, 'Alice')",
			$"INSERT INTO {table} (Id, V) VALUES (2, 'Bob')",
		};
		CleanupSql = new[] { $"DELETE FROM {table}" };
		InsertSql = $"INSERT INTO {table} (Id, V) VALUES (3, 'Carol')";
		MultiRowUpdateSql = $"UPDATE {table} SET V = 'Updated' WHERE Id > 0";
		UpdateSql = $"UPDATE {table} SET V = 'Alicia' WHERE Id = 1";
		UpdateNoRowsSql = $"UPDATE {table} SET V = 'Nobody' WHERE Id = 999";
		DeleteSql = $"DELETE FROM {table} WHERE Id = 2";
		DeleteNoRowsSql = $"DELETE FROM {table} WHERE Id = 999";
	}

	public string CreateTableSql { get; }

	public string DropTableSql { get; }

	public IReadOnlyList<string> SetupSql { get; }

	public IReadOnlyList<string> CleanupSql { get; }

	public string InsertSql { get; }

	public string MultiRowUpdateSql { get; }

	public string UpdateSql { get; }

	public string UpdateNoRowsSql { get; }

	public string DeleteSql { get; }

	public string DeleteNoRowsSql { get; }

	/// <summary>A table name of its own, so fixtures sharing a database never collide.</summary>
	public static string UniqueName(string prefix) => prefix + Guid.NewGuid().ToString("N").Substring(0, 8);

	public static string DefaultCreateTable(string table, string column) => $"create table {table} (Id integer not null primary key, V {column})";

	public static string DefaultDropTable(string table) => $"drop table {table}";

	/// <summary>Creates the table over <paramref name="fixture"/>'s connection; a failure is left for the tests to report.</summary>
	public void Create(IDbFactoryFixture fixture)
	{
		lock (SelectValueTables.DdlLock)
		{
			SelectValueTables.TryCreate(fixture, CreateTableSql, out _);
		}
	}

	public void Drop(IDbFactoryFixture fixture)
	{
		lock (SelectValueTables.DdlLock)
		{
			SelectValueTables.TryExecute(fixture, new[] { DropTableSql }, out _);
		}
	}
}

/// <summary>
/// Gives a provider whose own fixture has no DML statements the SQL-92 ones, over its connection. A fixture that states
/// its table SQL through <see cref="IDataTypesFidelityFixture"/> has the table created that way.
/// </summary>
public class DmlFixture<TFactoryFixture> : IDmlFixture, IDisposable
	where TFactoryFixture : IDbFactoryFixture, new()
{
	private readonly TFactoryFixture m_inner = new();
	private readonly Sql92Dml m_dml;

	public DmlFixture()
	{
		var tables = m_inner as IDataTypesFidelityFixture;
		m_dml = new Sql92Dml(Sql92Dml.UniqueName("dml_"),
			tables is null ? Sql92Dml.DefaultCreateTable : tables.CreateTableSql,
			tables is null ? Sql92Dml.DefaultDropTable : tables.DropTableSql);
		m_dml.Create(this);
	}

	public DbProviderFactory Factory => m_inner.Factory;

	public string ConnectionString => m_inner.ConnectionString;

	public IReadOnlyList<string> DmlSetupSql => m_dml.SetupSql;

	public IReadOnlyList<string> DmlCleanupSql => m_dml.CleanupSql;

	public string DmlInsertSql => m_dml.InsertSql;

	public string DmlMultiRowUpdateSql => m_dml.MultiRowUpdateSql;

	public string DmlUpdateSql => m_dml.UpdateSql;

	public string DmlUpdateNoRowsSql => m_dml.UpdateNoRowsSql;

	public string DmlDeleteSql => m_dml.DeleteSql;

	public string DmlDeleteNoRowsSql => m_dml.DeleteNoRowsSql;

	public void Dispose()
	{
		m_dml.Drop(this);
		(m_inner as IDisposable)?.Dispose();
	}
}
