using System.Data.Common;
using Oracle.ManagedDataAccess.Client;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;
using Xunit;

[assembly: AssemblyFixture(typeof(OracleServer))]

namespace OracleManagedDataAccess.Tests;

public class OracleManagedDataAccessDbFactoryFixture : IDbFactoryFixture, ISqlDialectFixture
{
	public DbProviderFactory Factory => OracleClientFactory.Instance;
	public string ConnectionString => OracleServer.ContainerConnectionString;

	public string SelectStatement(string projection) => $"SELECT {projection} FROM dual";

	public string StatementTerminator => string.Empty;
}
