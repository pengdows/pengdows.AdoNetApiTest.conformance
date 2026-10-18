using System;
using System.Data.Common;
using pengdows.AdoNetApiTest.conformance;
using MySqlConnector;

namespace MySqlConnector.Tests;

public class MySqlConnectorDbFactoryFixture : IDbFactoryFixture
{
	public DbProviderFactory Factory => MySqlConnectorFactory.Instance;
	public string ConnectionString => "Server=localhost;User Id=root;Password=test;SSL Mode=none;Database=mysqlconnector;port=" + (Environment.GetEnvironmentVariable("MYSQL_PORT") ?? "3306");
}
