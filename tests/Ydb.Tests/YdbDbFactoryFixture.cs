using System;
using System.Collections.Generic;
using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using Ydb.Sdk.Ado;

namespace Ydb.Tests;

/// <summary>YQL reads <c>'text'</c> as a byte string (<c>String</c>), so text is <c>'text'u</c> (<c>Utf8</c>); names are quoted with backticks.</summary>
// The counted statements are INSERT, UPDATE and DELETE: ExecuteNonQuery owes a row count only for those, and -1 for UPSERT.
public sealed class YdbDbFactoryFixture : IDbFactoryFixture, IDmlFixture, ISqlLiteralFixture, IDisposable
{
    public DbProviderFactory Factory => YdbProviderFactory.Instance;

    public string ConnectionString =>
        Environment.GetEnvironmentVariable("YDB_CONNECTION_STRING")
        ?? "Host=localhost;Port=2136;Database=/local";

	public IReadOnlyList<string> DmlSetupSql => new[]
	{
		"CREATE TABLE IF NOT EXISTS people (id Int32, name Utf8, PRIMARY KEY (id))",
		"UPSERT INTO people (id, name) VALUES (1, 'Alice'), (2, 'Bob')"
	};

	public IReadOnlyList<string> DmlCleanupSql => new[] { "DROP TABLE IF EXISTS people" };
    public string DmlInsertSql => "INSERT INTO people (id, name) VALUES (3, 'Carol'u);";
    public string DmlMultiRowUpdateSql => "UPDATE people SET name = 'Updated'u WHERE id IN (1, 2);";
    public string DmlUpdateSql => "UPDATE people SET name = 'Alicia'u WHERE id = 1;";
    public string DmlUpdateNoRowsSql => "UPDATE people SET name = 'Nobody'u WHERE id = 999;";
    public string DmlDeleteSql => "DELETE FROM people WHERE id = 2;";
    public string DmlDeleteNoRowsSql => "DELETE FROM people WHERE id = 999;";

    public string TextLiteral(string text) => "'" + text.Replace("\\", "\\\\").Replace("'", "\\'") + "'u";

    public string QuotedIdentifier(string name) => "`" + name.Replace("`", "\\`") + "`";

    public void Dispose()
    {
    }
}
