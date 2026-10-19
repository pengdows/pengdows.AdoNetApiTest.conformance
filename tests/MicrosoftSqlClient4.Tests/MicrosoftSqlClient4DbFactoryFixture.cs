using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using Microsoft.Data.SqlClient;
using pengdows.AdoNetApiTest.conformance.Databases;
using Xunit;

[assembly: AssemblyFixture(typeof(SqlServerServer))]

namespace MicrosoftSqlClient4.Tests;

public class MicrosoftSqlClient4DbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => SqlClientFactory.Instance;
	public string ConnectionString => $"data source={SqlServerServer.Host},{SqlServerServer.Port};user id={SqlServerServer.User};password={SqlServerServer.Password};initial catalog={SqlServerServer.Database};Encrypt=False";
}
