using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using Microsoft.Data.Sqlite;

// The database allows one writer at a time, so the test classes run one after another.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]

namespace MicrosoftSqlite5.Tests;

public class MicrosoftSqlite5DbFactoryFixture : IDbFactoryFixture, ISingleWriterFixture, IParameterMarkerFixture
{
	public DbProviderFactory Factory => SqliteFactory.Instance;
	public string ConnectionString => "data source=temp.db";

	/// <summary>'@' is how this provider writes a parameter; it advertises no DataSourceInformation to say so.</summary>
	public string ParameterMarker(string name) => "@" + name;
}
