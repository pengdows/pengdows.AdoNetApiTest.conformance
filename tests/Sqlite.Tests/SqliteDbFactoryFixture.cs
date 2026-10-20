using System.Data.Common;
using System.Data.SQLite;
using pengdows.AdoNetApiTest.conformance;

// The database allows one writer at a time, so the test classes run one after another.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]

namespace Sqlite.Tests;

public class SqliteDbFactoryFixture : IDbFactoryFixture, ISingleWriterFixture
{
	public DbProviderFactory Factory => SQLiteFactory.Instance;
	public string ConnectionString => "data source=temp.db";
}
