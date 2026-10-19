using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using Devart.Data.MySql;
using pengdows.AdoNetApiTest.conformance.Databases;
using Xunit;

[assembly: AssemblyFixture(typeof(MySqlServer))]

namespace DotConnectMySql.Tests;

public class DotConnectMySqlDbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => MySqlProviderFactory.Instance;
	public string ConnectionString => $"server={MySqlServer.Host};port={MySqlServer.Port};user id={MySqlServer.User};password={MySqlServer.Password};character set=utf8mb4;database={MySqlServer.Database}";
}
