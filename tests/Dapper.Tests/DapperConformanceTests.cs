using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Dapper.Tests;

public sealed class DapperConformanceTests(DapperSqliteFixture fixture) : IClassFixture<DapperSqliteFixture>
{
    private sealed record Item(long Id, string? Name, long? Amount);

    [Fact]
    public void ExecuteScalar_Query_And_ExecuteNonQuery_Use_AdoNet_Contracts()
    {
        using var connection = fixture.Open();

        Assert.Equal(ConnectionState.Open, connection.State);
        Assert.Equal(2, connection.ExecuteScalar<int>("select count(*) from Items"));
        Assert.Equal("one", connection.QuerySingle<string>("select Name from Items where Id = @Id", new { Id = 1 }));

        var affected = connection.Execute("update Items set Amount = @Amount where Id = @Id", new { Id = 1, Amount = 11 });

        Assert.Equal(1, affected);
        Assert.Equal(11, connection.ExecuteScalar<int>("select Amount from Items where Id = 1"));
    }

    [Fact]
    public void CommandDefinition_Binds_Command_And_Transaction()
    {
        using var connection = fixture.Open();
        using var transaction = connection.BeginTransaction();
        var command = new CommandDefinition(
            "update Items set Amount = @Amount where Id = @Id",
            new { Id = 1, Amount = 42 },
            transaction,
            commandTimeout: 30,
            commandType: CommandType.Text);

        Assert.Equal(1, connection.Execute(command));
        transaction.Commit();

        Assert.Equal(42, connection.ExecuteScalar<int>("select Amount from Items where Id = 1"));
    }

    [Fact]
    public void Parameters_Handle_Nulls_DbTypes_Strings_And_ListExpansion()
    {
        using var connection = fixture.Open();
        var parameters = new DynamicParameters();
        parameters.Add("id", 1, DbType.Int32);
        parameters.Add("missing", null, DbType.String);

        Assert.Equal("one", connection.QuerySingle<string>(
            "select Name from Items where Id = @id and @missing is null", parameters));

        var values = connection.Query<int>(
            "select Id from Items where Id in @ids order by Id", new { ids = new[] { 1, 2 } }).ToArray();

        Assert.Equal(new[] { 1, 2 }, values);

        var dbString = new DbString("one", length: 3) { IsAnsi = false };
        Assert.Equal(1, connection.ExecuteScalar<int>(
            "select count(*) from Items where Name = @name", new { name = dbString }));
    }

    [Fact]
    public void Query_Maps_Nulls_And_Uses_Reader_Metadata()
    {
        using var connection = fixture.Open();

        var items = connection.Query<Item>("select Id, Name, Amount from Items order by Id").ToArray();

        Assert.Equal(2, items.Length);
        Assert.Equal(new Item(1L, "one", 10L), items[0]);
        Assert.Equal(new Item(2L, null, null), items[1]);

        using var reader = connection.ExecuteReader("select Id, Name from Items order by Id");
        Assert.Equal(2, reader.FieldCount);
        Assert.Equal("Id", reader.GetName(0));
        Assert.Equal(typeof(long), reader.GetFieldType(0));
        Assert.True(reader.Read());
        Assert.Equal(1L, reader.GetValue(0));
        Assert.Equal("one", reader.GetString(1));
    }

    [Fact]
    public void Reader_Disposal_Does_Not_Close_An_AlreadyOpen_Connection()
    {
        using var connection = fixture.Open();

        using (var reader = connection.ExecuteReader("select Id from Items"))
        {
            Assert.True(reader.Read());
        }

        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Fact]
    public void QuerySingle_And_QueryFirst_Enforce_Row_Semantics()
    {
        using var connection = fixture.Open();

        Assert.Equal(1, connection.QueryFirst<int>("select Id from Items order by Id"));
        Assert.Throws<InvalidOperationException>(() =>
            connection.QuerySingle<int>("select Id from Items order by Id"));
        Assert.Null(connection.QuerySingleOrDefault<string>("select Name from Items where Id = 99"));
    }

    [Fact]
    public async Task Async_Execute_Query_And_Reader_Use_DbCommand_Async_APIs()
    {
        await using var connection = (SqliteConnection)fixture.Open();

        Assert.Equal(2, await connection.ExecuteScalarAsync<int>("select count(*) from Items"));

        var items = (await connection.QueryAsync<Item>(
            "select Id, Name, Amount from Items order by Id")).ToArray();
        Assert.Equal(2, items.Length);

        await using var reader = await connection.ExecuteReaderAsync("select Id from Items order by Id");
        Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1L, reader.GetFieldValue<long>(0));
    }

    [Fact]
    public async Task Cancellation_Is_Propagated_To_Async_Reader()
    {
        await using var connection = (SqliteConnection)fixture.Open();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await connection.QueryAsync<int>(new CommandDefinition(
                "select Id from Items", cancellationToken: cancellation.Token)));
    }
}
