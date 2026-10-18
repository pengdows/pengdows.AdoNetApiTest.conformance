using System;
using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using Devart.Data.MySql;

namespace DotConnectMySql.Tests;

public class DotConnectMySqlDbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => MySqlProviderFactory.Instance;
	public string ConnectionString => "server=localhost;user id=root;password=test;character set=utf8mb4;database=dotconnect;port=" + (Environment.GetEnvironmentVariable("MYSQL_PORT") ?? "3306");
}
