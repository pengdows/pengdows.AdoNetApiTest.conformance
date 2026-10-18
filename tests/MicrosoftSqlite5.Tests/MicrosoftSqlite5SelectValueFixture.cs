using System;
using System.Collections.Generic;
using System.Data;
using pengdows.AdoNetApiTest.conformance.Databases;
using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite5.Tests;

public class MicrosoftSqlite5SelectValueFixture : MicrosoftSqlite5DbFactoryFixture, ISelectValueFixture, IDeleteFixture, IDisposable
{
	public MicrosoftSqlite5SelectValueFixture() => SqliteDatabase.CreateSelectValueTable(this);
	public void Dispose() => SqliteDatabase.DropSelectValueTable(this);
	public string CreateSelectSql(DbType dbType, ValueKind kind) => SqliteDatabase.CreateSelectSql(dbType, kind);
	public string CreateSelectSql(byte[] value) => SqliteDatabase.CreateSelectSql(value);
	public string SelectNoRows => SqliteDatabase.SelectNoRows;
	public IReadOnlyCollection<DbType> SupportedDbTypes => SqliteDatabase.SupportedDbTypes;
	public Type NullValueExceptionType => SqliteDatabase.NullValueExceptionType;
	public string DeleteNoRows => SqliteDatabase.DeleteNoRows;
}
