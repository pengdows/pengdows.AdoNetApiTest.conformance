using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using Devart.Data.PostgreSql;
using pengdows.AdoNetApiTest.conformance.Databases;
using Xunit;

[assembly: AssemblyFixture(typeof(PostgreSqlServer))]

namespace DotConnectPostgres.Tests;

public class DotConnectPostgresDbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => PgSqlProviderFactory.Instance;
	public string ConnectionString => $"host={PostgreSqlServer.Host};port={PostgreSqlServer.Port};user id={PostgreSqlServer.User};password={PostgreSqlServer.Password};unpreparedexecute=true;database={PostgreSqlServer.Database};";
}
