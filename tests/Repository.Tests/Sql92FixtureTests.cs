using System.Collections.Generic;
using System.Data;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;
using Xunit;

namespace Repository.Tests;

/// <summary>
/// The SQL a provider without hand-written fixtures gets for the DML and reader tests: SQL-92 statements over tables of
/// its own, one row per statement and no terminator, so Oracle, Firebird and the rest accept it unchanged.
/// </summary>
public class Sql92FixtureTests
{
	private static string Create(string table, string column) => $"create table {table} (Id integer not null primary key, V {column})";

	private static string Drop(string table) => $"drop table {table}";

	/// <summary>
	/// The table is created once per fixture and each test only resets its rows: creating and dropping a table per test
	/// deadlocks Firebird's metadata and makes YugabyteDB restart reads.
	/// </summary>
	[Fact]
	public void Dml_CreatesTheTableOnceAndResetsItsRowsPerTest()
	{
		var dml = new Sql92Dml("dml_t", Create, Drop);

		Assert.Equal("create table dml_t (Id integer not null primary key, V varchar(100))", dml.CreateTableSql);
		Assert.Equal("drop table dml_t", dml.DropTableSql);
		Assert.Equal(new[]
		{
			"DELETE FROM dml_t",
			"INSERT INTO dml_t (Id, V) VALUES (1, 'Alice')",
			"INSERT INTO dml_t (Id, V) VALUES (2, 'Bob')",
		}, dml.SetupSql);
		Assert.Equal(new[] { "DELETE FROM dml_t" }, dml.CleanupSql);
	}

	[Fact]
	public void Dml_StatementsChangeTheRowsTheyMatch()
	{
		var dml = new Sql92Dml("dml_t", Create, Drop);

		Assert.Equal("INSERT INTO dml_t (Id, V) VALUES (3, 'Carol')", dml.InsertSql);
		Assert.Equal("UPDATE dml_t SET V = 'Updated' WHERE Id > 0", dml.MultiRowUpdateSql);
		Assert.Equal("UPDATE dml_t SET V = 'Alicia' WHERE Id = 1", dml.UpdateSql);
		Assert.Equal("UPDATE dml_t SET V = 'Nobody' WHERE Id = 999", dml.UpdateNoRowsSql);
		Assert.Equal("DELETE FROM dml_t WHERE Id = 2", dml.DeleteSql);
		Assert.Equal("DELETE FROM dml_t WHERE Id = 999", dml.DeleteNoRowsSql);
	}

	/// <summary>
	/// RETURNING is not SQL-92, so a provider states its form; without one there are no RETURNING statements and the
	/// tests are not applicable. The INSERT adds one row and the DELETE removes both, so a count of 0 cannot pass.
	/// </summary>
	[Fact]
	public void Dml_ReturningStatementsUseTheStatedForm()
	{
		var dml = new Sql92Dml("dml_t", Create, Drop, "{0} RETURNING Id");

		Assert.Equal("INSERT INTO dml_t (Id, V) VALUES (3, 'Carol') RETURNING Id", dml.InsertReturningSql);
		Assert.Equal("DELETE FROM dml_t WHERE Id > 0 RETURNING Id", dml.DeleteReturningSql);
		Assert.Null(new Sql92Dml("dml_t", Create, Drop).InsertReturningSql);
		Assert.Null(new Sql92Dml("dml_t", Create, Drop).DeleteReturningSql);
	}

	/// <summary>The SQL:2003 MERGE updates row 1 and inserts row 3, two rows in all.</summary>
	[Fact]
	public void Dml_MergeUpdatesOneRowAndInsertsAnother()
	{
		var dml = new Sql92Dml("dml_t", Create, Drop);

		Assert.Equal("MERGE INTO dml_t t USING (VALUES (1, 'Merged'), (3, 'Dave')) s (Id, V) ON (t.Id = s.Id) " +
			"WHEN MATCHED THEN UPDATE SET V = s.V WHEN NOT MATCHED THEN INSERT (Id, V) VALUES (s.Id, s.V)", dml.MergeSql);
	}

	private static SelectValueTables Tables(IReadOnlyDictionary<DbType, string> columnTypes = null, string binaryLiteral = null) =>
		new("sv_t", columnTypes ?? SelectValueTables.DefaultColumnTypes, Create, Drop, p => $"SELECT {p} FROM dual", binaryLiteral ?? SelectValueTables.DefaultBinaryLiteral);

	[Fact]
	public void SelectValues_DefaultToSql92ColumnTypes()
	{
		Assert.Equal("integer", SelectValueTables.DefaultColumnTypes[DbType.Int32]);
		Assert.Equal("varchar(100)", SelectValueTables.DefaultColumnTypes[DbType.String]);
		Assert.Equal("varbinary(100)", SelectValueTables.DefaultColumnTypes[DbType.Binary]);
		Assert.Equal("X'{0}'", SelectValueTables.DefaultBinaryLiteral);
	}

	[Fact]
	public void SelectValues_CreateATablePerTypeWithARowPerKind()
	{
		Assert.Equal(new[]
		{
			"create table sv_t_String (Id integer not null primary key, V varchar(100))",
			"INSERT INTO sv_t_String (Id, V) VALUES (0, NULL)",
			"INSERT INTO sv_t_String (Id, V) VALUES (1, '')",
			"INSERT INTO sv_t_String (Id, V) VALUES (2, '0')",
			"INSERT INTO sv_t_String (Id, V) VALUES (3, '1')",
			"INSERT INTO sv_t_String (Id, V) VALUES (4, NULL)",
			"INSERT INTO sv_t_String (Id, V) VALUES (5, NULL)",
		}, Tables().CreateSql(DbType.String));
		Assert.Equal("drop table sv_t_String", Tables().DropSql(DbType.String));
	}

	[Theory]
	[InlineData(DbType.Int16, ValueKind.Minimum, "-32768")]
	[InlineData(DbType.Int32, ValueKind.Maximum, "2147483647")]
	[InlineData(DbType.Int64, ValueKind.Minimum, "-9223372036854775808")]
	[InlineData(DbType.Double, ValueKind.Maximum, "1.79e308")]
	[InlineData(DbType.Int32, ValueKind.Empty, "NULL")]
	[InlineData(DbType.Int32, ValueKind.Null, "NULL")]
	[InlineData(DbType.Binary, ValueKind.One, "X'11'")]
	[InlineData(DbType.Binary, ValueKind.Zero, "X'00'")]
	[InlineData(DbType.Binary, ValueKind.Empty, "X''")]
	[InlineData(DbType.Binary, ValueKind.Maximum, "NULL")]
	public void SelectValues_UseTheValuesEachKindDocuments(DbType dbType, ValueKind kind, string literal) =>
		Assert.Equal(literal, Tables().Literal(dbType, kind));

	[Fact]
	public void SelectValues_SelectByKind()
	{
		Assert.Equal("SELECT V FROM sv_t_Int32 WHERE Id = 3", Tables().SelectSql(DbType.Int32, ValueKind.One));
		Assert.Equal("SELECT V FROM sv_t_String WHERE 1 = 0", Tables().SelectNoRows);
		Assert.Equal("DELETE FROM sv_t_String WHERE 1 = 0", Tables().DeleteNoRows);
	}

	private sealed class NoServerFactory : System.Data.Common.DbProviderFactory
	{
		public override System.Data.Common.DbConnection CreateConnection() => throw new System.InvalidOperationException("no server here");
	}

	private sealed class NoServerFixture : IDbFactoryFixture
	{
		public System.Data.Common.DbProviderFactory Factory { get; } = new NoServerFactory();

		public string ConnectionString => "x";
	}

	/// <summary>A table the server would not create is left out, and a test that reads it anyway is told why.</summary>
	[Fact]
	public void SelectValues_SayWhyATableWasNotCreated()
	{
		var tables = Tables();

		Assert.Empty(tables.Create(new NoServerFixture()));
		var error = Assert.Throws<System.InvalidOperationException>(() => tables.SelectSql(DbType.String, ValueKind.One));
		Assert.Contains("sv_t_String", error.Message);
		Assert.Contains("no server here", error.Message);
		Assert.Contains("no server here", Assert.Throws<System.InvalidOperationException>(() => tables.SelectNoRows).Message);
	}

	[Fact]
	public void SelectValues_SelectBytesWithTheBinaryLiteral()
	{
		Assert.Equal("SELECT X'7E57' FROM dual", Tables().SelectSql(new byte[] { 0x7E, 0x57 }));
		Assert.Equal("SELECT hextoraw('7E57') FROM dual", Tables(binaryLiteral: "hextoraw('{0}')").SelectSql(new byte[] { 0x7E, 0x57 }));
	}

	[Fact]
	public void SelectValues_OverridesReplaceAndRemoveColumnTypes()
	{
		var types = SelectValueTables.WithOverrides(new Dictionary<string, string> { ["Binary"] = "bytea", ["Double"] = "", ["Boolean"] = "boolean" });

		Assert.Equal("bytea", types[DbType.Binary]);
		Assert.Equal("boolean", types[DbType.Boolean]);
		Assert.False(types.ContainsKey(DbType.Double));
		Assert.Equal("integer", types[DbType.Int32]);
	}

	[Theory]
	[InlineData(DbType.Boolean, ValueKind.One, "'1'")]
	[InlineData(DbType.Boolean, ValueKind.Zero, "'0'")]
	[InlineData(DbType.Date, ValueKind.One, "'1111-11-11'")]
	[InlineData(DbType.Guid, ValueKind.One, "'11111111-1111-1111-1111-111111111111'")]
	[InlineData(DbType.Decimal, ValueKind.Maximum, "99999999999999999999.999999999999999")]
	public void SelectValues_CoverTheTypesAConfigurationAdds(DbType dbType, ValueKind kind, string literal) =>
		Assert.Equal(literal, Tables().Literal(dbType, kind));
}

/// <summary>A configuration states the binary literal and column types its database spells differently.</summary>
public class ProviderConfigSelectValueTests
{
	[Fact]
	public void Dialect_ReadsBinaryLiteralAndValueTypes()
	{
		var path = System.IO.Path.GetTempFileName();
		try
		{
			System.IO.File.WriteAllText(path, """
				{ "name": "X", "driver": { "package": "P", "factory": "F" }, "connectionString": "c",
				  "dialect": { "binaryLiteral": "'\\x{0}'::bytea", "valueTypes": { "Binary": "bytea", "Boolean": "boolean" } } }
				""");

			var config = ProviderConfig.Load(path);

			Assert.Equal(@"'\x{0}'::bytea", config.Dialect.BinaryLiteral);
			Assert.Equal("bytea", config.Dialect.ValueTypes["Binary"]);
		}
		finally
		{
			System.IO.File.Delete(path);
		}
	}

	[Fact]
	public void Dialect_DefaultsToTheSql92BinaryLiteral()
	{
		Assert.Equal(SelectValueTables.DefaultBinaryLiteral, new DialectConfig().BinaryLiteral);
		Assert.Empty(new DialectConfig().ValueTypes);
	}
}

/// <summary>A CREATE that loses a metadata deadlock to another test class's DDL (Firebird) is retried, not given up.</summary>
public class SelectValueRetryTests
{
	private sealed class FlakyFactory : System.Data.Common.DbProviderFactory
	{
		public int CreateFailuresLeft { get; set; }

		public List<string> Executed { get; } = new();

		public override System.Data.Common.DbConnection CreateConnection() => new FlakyConnection(this);
	}

	private sealed class FlakyConnection : System.Data.Common.DbConnection
	{
		private readonly FlakyFactory m_factory;
		private ConnectionState m_state;

		public FlakyConnection(FlakyFactory factory) => m_factory = factory;

		public override string ConnectionString { get; set; } = "";
		public override string Database => "";
		public override string DataSource => "";
		public override string ServerVersion => "";
		public override ConnectionState State => m_state;
		public override void ChangeDatabase(string databaseName) { }
		public override void Close() => m_state = ConnectionState.Closed;
		public override void Open() => m_state = ConnectionState.Open;
		protected override System.Data.Common.DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new System.NotSupportedException();
		protected override System.Data.Common.DbCommand CreateDbCommand() => new FlakyCommand(m_factory);
	}

	private sealed class FlakyCommand : System.Data.Common.DbCommand
	{
		private readonly FlakyFactory m_factory;

		public FlakyCommand(FlakyFactory factory) => m_factory = factory;

		public override string CommandText { get; set; } = "";
		public override int CommandTimeout { get; set; }
		public override CommandType CommandType { get; set; }
		public override bool DesignTimeVisible { get; set; }
		public override UpdateRowSource UpdatedRowSource { get; set; }
		protected override System.Data.Common.DbConnection DbConnection { get; set; }
		protected override System.Data.Common.DbParameterCollection DbParameterCollection => throw new System.NotSupportedException();
		protected override System.Data.Common.DbTransaction DbTransaction { get; set; }
		public override void Cancel() { }
		public override void Prepare() { }
		protected override System.Data.Common.DbParameter CreateDbParameter() => throw new System.NotSupportedException();
		protected override System.Data.Common.DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => throw new System.NotSupportedException();
		public override object ExecuteScalar() => null;

		public override int ExecuteNonQuery()
		{
			if (CommandText.StartsWith("create", System.StringComparison.Ordinal) && m_factory.CreateFailuresLeft-- > 0)
			{
				throw new System.InvalidOperationException("deadlock: update conflicts with concurrent update");
			}

			m_factory.Executed.Add(CommandText);
			return 1;
		}
	}

	private sealed class FlakyFixture : IDbFactoryFixture
	{
		public FlakyFactory Flaky { get; } = new();

		public System.Data.Common.DbProviderFactory Factory => Flaky;

		public string ConnectionString => "x";
	}

	[Fact]
	public void SelectValues_RetryACreateThatDeadlocks()
	{
		var fixture = new FlakyFixture();
		fixture.Flaky.CreateFailuresLeft = 1;
		var tables = new SelectValueTables("sv_t", new Dictionary<DbType, string> { [DbType.String] = "varchar(100)" },
			Sql92Dml.DefaultCreateTable, Sql92Dml.DefaultDropTable, p => "SELECT " + p, SelectValueTables.DefaultBinaryLiteral);

		Assert.Equal(new[] { DbType.String }, tables.Create(fixture));
		Assert.Equal("SELECT V FROM sv_t_String WHERE 1 = 0", tables.SelectNoRows);
	}

	[Fact]
	public void Dml_RetriesACreateThatDeadlocks()
	{
		var fixture = new FlakyFixture();
		fixture.Flaky.CreateFailuresLeft = 1;

		new Sql92Dml("dml_t", Sql92Dml.DefaultCreateTable, Sql92Dml.DefaultDropTable).Create(fixture);

		Assert.Contains("create table dml_t (Id integer not null primary key, V varchar(100))", fixture.Flaky.Executed);
	}
}
