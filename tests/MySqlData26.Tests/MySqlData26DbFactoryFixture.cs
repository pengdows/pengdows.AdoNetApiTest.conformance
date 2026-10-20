using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using MySql.Data.MySqlClient;
using pengdows.AdoNetApiTest.conformance.Databases;
using Xunit;

[assembly: AssemblyFixture(typeof(MySqlServer))]

namespace MySqlData26.Tests;

public class MySqlData26DbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => MySqlClientFactory.Instance;
	public string ConnectionString => $"server={MySqlServer.Host};port={MySqlServer.Port};user id={MySqlServer.User};password={MySqlServer.Password};characterset=utf8mb4;database={MySqlServer.Database}";
}
