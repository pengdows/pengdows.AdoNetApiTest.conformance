using System;
using pengdows.AdoNetApiTest.conformance;

namespace pengdows.AdoNetApiTest.conformance.Databases;

public static class Utility
{
	public static void ExecuteNonQuery(IDbFactoryFixture factoryFixture, string sql)
	{
		using (var connection = factoryFixture.Factory.CreateConnection())
		{
			connection.ConnectionString = Environment.GetEnvironmentVariable("ConnectionString") ?? factoryFixture.ConnectionString;
			connection.Open();

			using (var command = connection.CreateCommand())
			{
				command.CommandText = sql;
				command.ExecuteNonQuery();
			}
		}
	}
}
