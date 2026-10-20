using System.Data.Common;
using System.Data.SqlClient;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;
using Xunit;

[assembly: AssemblyFixture(typeof(SqlServerServer))]

namespace SqlClientBcl.Tests;

public class SqlClientDbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => SqlClientFactory.Instance;
	public string ConnectionString => $"data source={SqlServerServer.Host},{SqlServerServer.Port};user id={SqlServerServer.User};password={SqlServerServer.Password};initial catalog={SqlServerServer.Database}";
}
