using System;
using System.Data.Common;
using System.Data.SQLite;
using pengdows.AdoNetApiTest.conformance;

namespace FaithlifeSqlite.Tests;

public class FaithlifeSqliteDbFactoryFixture : IDbFactoryFixture, IParameterMarkerFixture
{
	public DbProviderFactory Factory { get; } = new FaithlifeSqliteFactory();
	public string ConnectionString => "data source=temp.db";

	/// <summary>'@' is how this provider writes a parameter; it advertises no DataSourceInformation to say so.</summary>
	public string ParameterMarker(string name) => "@" + name;

	private sealed class FaithlifeSqliteFactory : DbProviderFactory
	{
		public override bool CanCreateCommandBuilder => false;
		public override bool CanCreateDataSourceEnumerator => false;
		public override bool CanCreateDataAdapter => false;
		public override DbCommand CreateCommand() => new SQLiteCommand();
		public override DbCommandBuilder CreateCommandBuilder() => throw new NotSupportedException();
		public override DbConnection CreateConnection() => new SQLiteConnection();
		public override DbConnectionStringBuilder CreateConnectionStringBuilder() => new SQLiteConnectionStringBuilder();
		public override DbDataAdapter CreateDataAdapter() => throw new NotSupportedException();
		public override DbDataSourceEnumerator CreateDataSourceEnumerator() => throw new NotSupportedException();
		public override DbParameter CreateParameter() => new SQLiteParameter();
	}
}
