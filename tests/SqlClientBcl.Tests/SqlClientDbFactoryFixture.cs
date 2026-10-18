using System.Data.Common;
using System.Data.SqlClient;
using pengdows.AdoNetApiTest.conformance;

namespace SqlClientBcl.Tests;

public class SqlClientDbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => SqlClientFactory.Instance;
	public string ConnectionString => "data source=localhost;user id=sa;password=P@ssw0rd;initial catalog=sqlclientbcl";
}
