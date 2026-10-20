using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using Npgsql;
using pengdows.AdoNetApiTest.conformance.Databases;
using Xunit;

[assembly: AssemblyFixture(typeof(PostgreSqlServer))]

namespace Npgsql4.Tests;

public class Npgsql4DbFactoryFixture : IDbFactoryFixture, IReturningDialectFixture
{
	public DbProviderFactory Factory => NpgsqlFactory.Instance;
	public string ConnectionString => $"host={PostgreSqlServer.Host};port={PostgreSqlServer.Port};user id={PostgreSqlServer.User};password={PostgreSqlServer.Password};database={PostgreSqlServer.Database}";

	public string ReturningFormat => "{0} RETURNING Id";
}
