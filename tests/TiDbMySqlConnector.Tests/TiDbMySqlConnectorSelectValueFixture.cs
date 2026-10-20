using System;
using System.Collections.Generic;
using System.Data;
using pengdows.AdoNetApiTest.conformance.Databases;
using pengdows.AdoNetApiTest.conformance;

namespace TiDbMySqlConnector.Tests;

public class TiDbMySqlConnectorSelectValueFixture : TiDbMySqlConnectorDbFactoryFixture, ISelectValueFixture, IDeleteFixture, IDisposable
{
	public TiDbMySqlConnectorSelectValueFixture() => MySqlDatabase.CreateSelectValueTable(this);

	public void Dispose() => MySqlDatabase.DropSelectValueTable(this);

	public string CreateSelectSql(DbType dbType, ValueKind kind) => MySqlDatabase.CreateSelectSql(dbType, kind);

	public string CreateSelectSql(byte[] value) => MySqlDatabase.CreateSelectSql(value);

	public string SelectNoRows => MySqlDatabase.SelectNoRows;

	public IReadOnlyCollection<DbType> SupportedDbTypes => MySqlDatabase.SupportedDbTypes;

	public Type NullValueExceptionType => MySqlDatabase.NullValueExceptionType;

	public string DeleteNoRows => MySqlDatabase.DeleteNoRows;
}
