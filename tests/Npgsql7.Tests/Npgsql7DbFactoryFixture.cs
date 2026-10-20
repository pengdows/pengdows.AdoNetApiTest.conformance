using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

[assembly: AssemblyFixture(typeof(PostgreSqlServer))]

namespace Npgsql7.Tests;

public class Npgsql7DbFactoryFixture : IDbFactoryFixture, IReturningDialectFixture
{
	public DbProviderFactory Factory => NpgsqlFactory.Instance;
	public string ConnectionString => $"host={PostgreSqlServer.Host};port={PostgreSqlServer.Port};user id={PostgreSqlServer.User};password={PostgreSqlServer.Password};database={PostgreSqlServer.Database}";

	public string ReturningFormat => "{0} RETURNING Id";
}
