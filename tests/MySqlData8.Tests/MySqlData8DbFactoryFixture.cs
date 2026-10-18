using System;
using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using MySql.Data.MySqlClient;

namespace MySqlData8.Tests;

public class MySqlData8DbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => MySqlClientFactory.Instance;
	public string ConnectionString => "server=localhost;user id=root;password=test;characterset=utf8mb4;database=mysqldata8;port=" + (Environment.GetEnvironmentVariable("MYSQL_PORT") ?? "3306");
}
