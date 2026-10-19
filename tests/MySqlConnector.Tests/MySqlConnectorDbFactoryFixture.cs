using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using MySqlConnector;
using pengdows.AdoNetApiTest.conformance.Databases;
using Xunit;

[assembly: AssemblyFixture(typeof(MySqlServer))]

namespace MySqlConnector.Tests;

public class MySqlConnectorDbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => MySqlConnectorFactory.Instance;
	public string ConnectionString => $"Server={MySqlServer.Host};port={MySqlServer.Port};user id={MySqlServer.User};password={MySqlServer.Password};SSL Mode=none;Database={MySqlServer.Database}";
}
