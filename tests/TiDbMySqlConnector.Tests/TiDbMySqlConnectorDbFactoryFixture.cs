using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using MySqlConnector;
using pengdows.AdoNetApiTest.conformance.Databases;
using Xunit;

[assembly: AssemblyFixture(typeof(TiDbServer))]

namespace TiDbMySqlConnector.Tests;

public class TiDbMySqlConnectorDbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => MySqlConnectorFactory.Instance;
	public string ConnectionString => $"Server={TiDbServer.Host};port={TiDbServer.Port};user id={TiDbServer.User};SSL Mode=none;Database={TiDbServer.Database}";
}
