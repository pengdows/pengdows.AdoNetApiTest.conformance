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
	/// <param name="returning">The form of DML that returns rows, <c>{0}</c> being the statement; null when the database has none.</param>
	public Sql92Dml(string table, Func<string, string, string> createTable, Func<string, string> dropTable, string returning = null)
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
		SavepointSetupSql = new[] { $"DELETE FROM {table}" };
		SavepointCleanupSql = new[] { $"DELETE FROM {table}" };
		SavepointFirstInsertSql = $"INSERT INTO {table} (Id, V) VALUES (1, 'SavepointBefore')";
		SavepointSecondInsertSql = $"INSERT INTO {table} (Id, V) VALUES (2, 'SavepointAfter')";
		SavepointRowCountSql = $"SELECT COUNT(*) FROM {table}";
		InsertSql = $"INSERT INTO {table} (Id, V) VALUES (3, 'Carol')";
		MultiRowUpdateSql = $"UPDATE {table} SET V = 'Updated' WHERE Id > 0";
		UpdateSql = $"UPDATE {table} SET V = 'Alicia' WHERE Id = 1";
		UpdateNoRowsSql = $"UPDATE {table} SET V = 'Nobody' WHERE Id = 999";
		DeleteSql = $"DELETE FROM {table} WHERE Id = 2";
		DeleteNoRowsSql = $"DELETE FROM {table} WHERE Id = 999";
		InsertReturningSql = returning is null ? null : string.Format(System.Globalization.CultureInfo.InvariantCulture, returning, InsertSql);
		DeleteReturningSql = returning is null ? null : string.Format(System.Globalization.CultureInfo.InvariantCulture, returning, $"DELETE FROM {table} WHERE Id > 0");
		MergeSql = $"MERGE INTO {table} t USING (VALUES (1, 'Merged'), (3, 'Dave')) s (Id, V) ON (t.Id = s.Id) " +
			"WHEN MATCHED THEN UPDATE SET V = s.V WHEN NOT MATCHED THEN INSERT (Id, V) VALUES (s.Id, s.V)";
	}

	public string CreateTableSql { get; }

	public string DropTableSql { get; }

	public IReadOnlyList<string> SetupSql { get; }

	public IReadOnlyList<string> CleanupSql { get; }

	public IReadOnlyList<string> SavepointSetupSql { get; }

	public IReadOnlyList<string> SavepointCleanupSql { get; }

	public string SavepointFirstInsertSql { get; }

	public string SavepointSecondInsertSql { get; }

	public string SavepointRowCountSql { get; }

	public string InsertSql { get; }

	public string MultiRowUpdateSql { get; }

	public string UpdateSql { get; }

	public string UpdateNoRowsSql { get; }

	public string DeleteSql { get; }

	public string DeleteNoRowsSql { get; }

	/// <summary>An INSERT of one row that returns it, or null when the database has no RETURNING form.</summary>
	public string InsertReturningSql { get; }

	/// <summary>A DELETE of both setup rows that returns them, or null when the database has no RETURNING form.</summary>
	public string DeleteReturningSql { get; }

	/// <summary>A SQL:2003 MERGE that updates row 1 and inserts row 3.</summary>
	public string MergeSql { get; }

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
/// its table SQL through <see cref="IDataTypesFidelityFixture"/> has the table created that way, and one that states a
/// RETURNING form through <see cref="IReturningDialectFixture"/> gets the RETURNING statements.
/// </summary>
public class DmlFixture<TFactoryFixture> : IDmlReturningFixture, IMergeFixture, ISavepointSemanticsFixture, IDisposable
	where TFactoryFixture : IDbFactoryFixture, new()
{
	private readonly TFactoryFixture m_inner = new();
	private readonly Sql92Dml m_dml;

	public DmlFixture()
	{
		var tables = m_inner as IDataTypesFidelityFixture;
		m_dml = new Sql92Dml(Sql92Dml.UniqueName("dml_"),
			tables is null ? Sql92Dml.DefaultCreateTable : tables.CreateTableSql,
			tables is null ? Sql92Dml.DefaultDropTable : tables.DropTableSql,
			(m_inner as IReturningDialectFixture)?.ReturningFormat);
		m_dml.Create(this);
	}

	public DbProviderFactory Factory => m_inner.Factory;

	public string ConnectionString => m_inner.ConnectionString;

	public IReadOnlyList<string> DmlSetupSql => m_dml.SetupSql;

	public IReadOnlyList<string> DmlCleanupSql => m_dml.CleanupSql;

	public IReadOnlyList<string> SavepointSetupSql => m_dml.SavepointSetupSql;

	public IReadOnlyList<string> SavepointCleanupSql => m_dml.SavepointCleanupSql;

	public string SavepointFirstInsertSql => m_dml.SavepointFirstInsertSql;

	public string SavepointSecondInsertSql => m_dml.SavepointSecondInsertSql;

	public string SavepointRowCountSql => m_dml.SavepointRowCountSql;

	public string DmlInsertSql => m_dml.InsertSql;

	public string DmlMultiRowUpdateSql => m_dml.MultiRowUpdateSql;

	public string DmlUpdateSql => m_dml.UpdateSql;

	public string DmlUpdateNoRowsSql => m_dml.UpdateNoRowsSql;

	public string DmlDeleteSql => m_dml.DeleteSql;

	public string DmlDeleteNoRowsSql => m_dml.DeleteNoRowsSql;

	public string DmlInsertReturningSql => m_dml.InsertReturningSql;

	public string DmlDeleteReturningSql => m_dml.DeleteReturningSql;

	public string DmlMergeSql => m_dml.MergeSql;

	public void Dispose()
	{
		m_dml.Drop(this);
		(m_inner as IDisposable)?.Dispose();
	}
}
