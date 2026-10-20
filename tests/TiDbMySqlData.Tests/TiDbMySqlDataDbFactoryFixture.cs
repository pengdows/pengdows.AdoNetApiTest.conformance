using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using MySql.Data.MySqlClient;
using pengdows.AdoNetApiTest.conformance.Databases;
using Xunit;

[assembly: AssemblyFixture(typeof(TiDbServer))]

namespace TiDbMySqlData.Tests;

public class TiDbMySqlDataDbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => MySqlClientFactory.Instance;
	public string ConnectionString => $"server={TiDbServer.Host};port={TiDbServer.Port};user id={TiDbServer.User};characterset=utf8mb4;database={TiDbServer.Database}";
}
