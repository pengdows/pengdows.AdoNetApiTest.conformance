using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using Microsoft.Data.Sqlite;

namespace MicrosoftSqlite8.Tests;

public class MicrosoftSqlite8DbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => SqliteFactory.Instance;
	public string ConnectionString => "data source=temp.db";
}
