using System;
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

	public string SelectStatement(string projection)
	{
		// Firebird cannot infer the type of a parameter used as a bare SELECT
		// expression. Keep the shared parameter-binding test provider-neutral by
		// giving Firebird the type context required by its SQL dialect.
		if (projection.StartsWith("@", StringComparison.Ordinal) ||
			projection.StartsWith("?", StringComparison.Ordinal))
		{
			projection = $"CAST({projection} AS BIGINT)";
		}

		return $"SELECT {projection} FROM rdb$database";
	}

	public string StatementTerminator => string.Empty;
}
