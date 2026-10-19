using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using Microsoft.Data.Sqlite;

namespace MicrosoftSqlite7.Tests;

public class MicrosoftSqlite7DbFactoryFixture : IDbFactoryFixture, IParameterMarkerFixture
{
	public DbProviderFactory Factory => SqliteFactory.Instance;
	public string ConnectionString => "data source=temp.db";

	/// <summary>'@' is how this provider writes a parameter; it advertises no DataSourceInformation to say so.</summary>
	public string ParameterMarker(string name) => "@" + name;
}
