using pengdows.AdoNetApiTest.conformance.Databases;

[assembly: AssemblyFixture(typeof(PostgreSqlServer))]

namespace Npgsql7.Tests;

public class Npgsql7DbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => NpgsqlFactory.Instance;
	public string ConnectionString => $"host={PostgreSqlServer.Host};port={PostgreSqlServer.Port};user id={PostgreSqlServer.User};password={PostgreSqlServer.Password};database={PostgreSqlServer.Database}";
}
