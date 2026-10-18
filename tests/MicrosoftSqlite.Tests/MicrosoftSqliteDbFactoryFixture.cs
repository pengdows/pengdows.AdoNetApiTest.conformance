using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using Microsoft.Data.Sqlite;

namespace MicrosoftSqlite.Tests;

public class MicrosoftSqliteDbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => SqliteFactory.Instance;
	public string ConnectionString => "data source=temp.db";
}
