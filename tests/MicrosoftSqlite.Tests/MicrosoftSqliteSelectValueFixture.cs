using System;
using System.Collections.Generic;
using System.Data;
using pengdows.AdoNetApiTest.conformance.Databases;
using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite.Tests;

public class MicrosoftSqliteSelectValueFixture : MicrosoftSqliteDbFactoryFixture, ISelectValueFixture, IDeleteFixture, IDisposable
{
	public MicrosoftSqliteSelectValueFixture() => SqliteDatabase.CreateSelectValueTable(this);
	public void Dispose() => SqliteDatabase.DropSelectValueTable(this);
	public string CreateSelectSql(DbType dbType, ValueKind kind) => SqliteDatabase.CreateSelectSql(dbType, kind);
	public string CreateSelectSql(byte[] value) => SqliteDatabase.CreateSelectSql(value);
	public string SelectNoRows => SqliteDatabase.SelectNoRows;

	public IReadOnlyCollection<DbType> SupportedDbTypes { get; } = new[]
	{
		DbType.Binary,
		DbType.Double,
		DbType.Int64,
		DbType.String,
	};

	public Type NullValueExceptionType => SqliteDatabase.NullValueExceptionType;
	public string DeleteNoRows => SqliteDatabase.DeleteNoRows;
}
