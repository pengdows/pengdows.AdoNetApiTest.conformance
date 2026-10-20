using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using DuckDB.NET.Data;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace DuckDB.Tests;

/// <summary>
/// The reader tests read tables a fixture created on another connection, and every <c>:memory:</c> connection is a
/// database of its own, so these use a file removed afterwards.
/// </summary>
public sealed class DuckDBFileDbFactoryFixture : IDbFactoryFixture, IDisposable
{
	private readonly string m_path = Path.Combine(Path.GetTempPath(), "adonet-duckdb-" + Guid.NewGuid().ToString("N") + ".duckdb");

	public DbProviderFactory Factory => DuckDBClientFactory.Instance;

	public string ConnectionString => "Data Source=" + m_path;

	public void Dispose()
	{
		File.Delete(m_path);
		File.Delete(m_path + ".wal");
	}
}

/// <summary>DuckDB spells a binary column <c>blob</c> and builds one from hexadecimal digits with <c>unhex</c>.</summary>
public sealed class DuckDBSelectValueFixture : SelectValueFixture<DuckDBFileDbFactoryFixture>
{
	protected override IReadOnlyDictionary<DbType, string> ColumnTypes { get; } =
		SelectValueTables.WithOverrides(new Dictionary<string, string> { ["Binary"] = "blob" });

	protected override string BinaryLiteral => "unhex('{0}')";
}
