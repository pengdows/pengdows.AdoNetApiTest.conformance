using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;
using Xunit;

[assembly: AssemblyFixture(typeof(FirebirdServer))]

namespace FirebirdClient.Tests;

public class FirebirdClientDbFactoryFixture : IDbFactoryFixture, ISqlDialectFixture
{
	public DbProviderFactory Factory => FirebirdClientFactory.Instance;
	public string ConnectionString => FirebirdServer.ContainerConnectionString + ";Charset=UTF8";

	public string SelectStatement(string projection) => $"SELECT {projection} FROM rdb$database";

	public string StatementTerminator => string.Empty;
}
