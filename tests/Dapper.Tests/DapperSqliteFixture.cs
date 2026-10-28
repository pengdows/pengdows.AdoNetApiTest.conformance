using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Dapper.Tests;

public sealed class DapperSqliteFixture
{
    public DbConnection Open()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            create table Items (Id integer primary key, Name text null, Amount integer null);
            insert into Items (Id, Name, Amount) values (1, 'one', 10), (2, null, null);
            """;
        command.ExecuteNonQuery();
        return connection;
    }
}
