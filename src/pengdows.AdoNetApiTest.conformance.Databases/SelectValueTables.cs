using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace pengdows.AdoNetApiTest.conformance.Databases;

/// <summary>
/// The values <see cref="ISelectValueFixture"/> returns, kept in a table per type (<c>Id</c> is the
/// <see cref="ValueKind"/>, <c>V</c> the value) created with SQL-92 statements. A type whose table the server will not
/// create is left out of the supported types; a value the type cannot hold leaves its row out, so the tests reading it
/// fail instead of passing unseen.
/// </summary>
public sealed class SelectValueTables
{
	private static readonly ValueKind[] s_kinds = (ValueKind[]) Enum.GetValues(typeof(ValueKind));

	private readonly string m_prefix;
	private readonly IReadOnlyDictionary<DbType, string> m_columnTypes;
	private readonly Func<string, string, string> m_createTable;
	private readonly Func<string, string> m_dropTable;
	private readonly Func<string, string> m_select;
	private readonly string m_binaryLiteral;
	private readonly List<DbType> m_created = new();
	private readonly Dictionary<DbType, string> m_failures = new();

	public SelectValueTables(string prefix, IReadOnlyDictionary<DbType, string> columnTypes, Func<string, string, string> createTable,
		Func<string, string> dropTable, Func<string, string> select, string binaryLiteral)
	{
		m_prefix = prefix;
		m_columnTypes = columnTypes;
		m_createTable = createTable;
		m_dropTable = dropTable;
		m_select = select;
		m_binaryLiteral = binaryLiteral;
	}

	/// <summary>SQL-92 column types, plus <c>varbinary</c> (SQL:1999 has no shorter portable binary type).</summary>
	public static IReadOnlyDictionary<DbType, string> DefaultColumnTypes { get; } = new Dictionary<DbType, string>
	{
		[DbType.Binary] = "varbinary(100)",
		[DbType.Double] = "double precision",
		[DbType.Int16] = "smallint",
		[DbType.Int32] = "integer",
		[DbType.Int64] = "bigint",
		[DbType.String] = "varchar(100)",
	};

	/// <summary>The SQL:1999 binary string literal, with <c>{0}</c> for the hexadecimal digits.</summary>
	public const string DefaultBinaryLiteral = "X'{0}'";

	/// <summary>The default column types with <paramref name="overrides"/> applied by <see cref="DbType"/> name; an empty type removes one.</summary>
	public static IReadOnlyDictionary<DbType, string> WithOverrides(IReadOnlyDictionary<string, string> overrides)
	{
		var types = new Dictionary<DbType, string>(DefaultColumnTypes.ToDictionary(p => p.Key, p => p.Value));
		foreach (var pair in overrides ?? new Dictionary<string, string>())
		{
			var dbType = (DbType) Enum.Parse(typeof(DbType), pair.Key, ignoreCase: true);
			if (string.IsNullOrEmpty(pair.Value))
			{
				types.Remove(dbType);
			}
			else
			{
				types[dbType] = pair.Value;
			}
		}

		return types;
	}

	public string TableName(DbType dbType) => m_prefix + "_" + dbType;

	public IReadOnlyList<string> CreateSql(DbType dbType)
	{
		var table = TableName(dbType);
		return new[] { m_createTable(table, m_columnTypes[dbType]) }
			.Concat(s_kinds.Select(kind => $"INSERT INTO {table} (Id, V) VALUES ({(int) kind}, {Literal(dbType, kind)})"))
			.ToList();
	}

	public string DropSql(DbType dbType) => m_dropTable(TableName(dbType));

	public string SelectSql(DbType dbType, ValueKind kind) => $"SELECT V FROM {Existing(dbType)} WHERE Id = {(int) kind}";

	public string SelectSql(byte[] value) => m_select(BinaryLiteral(value));

	public string SelectNoRows => $"SELECT V FROM {Existing(DbType.String)} WHERE 1 = 0";

	public string DeleteNoRows => $"DELETE FROM {Existing(DbType.String)} WHERE 1 = 0";

	/// <summary>The table's name, or why the server would not create it, so a test reading it fails with the cause.</summary>
	private string Existing(DbType dbType) => m_failures.TryGetValue(dbType, out var failure)
		? throw new InvalidOperationException($"The value table {TableName(dbType)} was not created: {failure}")
		: TableName(dbType);

	private string BinaryLiteral(byte[] value) =>
		string.Format(System.Globalization.CultureInfo.InvariantCulture, m_binaryLiteral, string.Concat(value.Select(b => b.ToString("X2"))));

	/// <summary>The SQL literal of each kind's value as <see cref="ValueKind"/> documents it, or <c>NULL</c> where the kind has none.</summary>
	public string Literal(DbType dbType, ValueKind kind)
	{
		var values = dbType switch
		{
			DbType.Binary => new[] { null, BinaryLiteral(new byte[0]), BinaryLiteral(new byte[] { 0 }), BinaryLiteral(new byte[] { 0x11 }), null, null },
			DbType.Boolean => new[] { null, null, "'0'", "'1'", "'0'", "'1'" },
			DbType.Date => new[] { null, null, null, "'1111-11-11'", "'0001-01-01'", "'9999-12-31'" },
			DbType.DateTime => new[] { null, null, null, "'1111-11-11 11:11:11.111'", "'0001-01-01 00:00:00'", "'9999-12-31 23:59:59.999'" },
			DbType.DateTimeOffset => new[] { null, null, null, "'1111-11-11 11:11:11.111 +11:11'", "'0001-01-01 00:00:00 +00:00'", "'9999-12-31 23:59:59.999 +14:00'" },
			DbType.Decimal => new[] { null, null, "0", "1", "0.000000000000001", "99999999999999999999.999999999999999" },
			DbType.Double => new[] { null, null, "0", "1", "2.23e-308", "1.79e308" },
			DbType.Guid => new[] { null, null, "'00000000-0000-0000-0000-000000000000'", "'11111111-1111-1111-1111-111111111111'", "'33221100-5544-7766-9988-aabbccddeeff'", "'ccddeeff-aabb-8899-7766-554433221100'" },
			DbType.Int16 => new[] { null, null, "0", "1", "-32768", "32767" },
			DbType.Int32 => new[] { null, null, "0", "1", "-2147483648", "2147483647" },
			DbType.Int64 => new[] { null, null, "0", "1", "-9223372036854775808", "9223372036854775807" },
			DbType.Single => new[] { null, null, "0", "1", "1.18e-38", "3.40e38" },
			DbType.String => new[] { null, "''", "'0'", "'1'", null, null },
			DbType.Time => new[] { null, null, "'00:00:00'", "'11:11:11.111'", "'00:00:00'", "'23:59:59.999'" },
			_ => throw new NotSupportedException($"No select values for {dbType}."),
		};
		return values[(int) kind] ?? "NULL";
	}

	/// <summary>
	/// Held while a fixture creates or drops its tables: test classes run in parallel, and Firebird loses rows and reports
	/// "table id is not defined" when fixtures change its metadata at the same time over pooled connections.
	/// </summary>
	internal static readonly object DdlLock = new();

	/// <summary>Creates the tables over <paramref name="fixture"/>'s connection and returns the types whose table exists.</summary>
	public IReadOnlyCollection<DbType> Create(IDbFactoryFixture fixture)
	{
		lock (DdlLock)
		{
			foreach (var dbType in m_columnTypes.Keys.OrderBy(t => t.ToString(), StringComparer.Ordinal))
			{
				var statements = CreateSql(dbType);
				if (!TryCreate(fixture, statements[0], out var failure))
				{
					m_failures[dbType] = failure;
					continue;
				}

				m_created.Add(dbType);
				TryExecute(fixture, statements.Skip(1), out _);
			}

			return m_created.ToList();
		}
	}

	public void Drop(IDbFactoryFixture fixture)
	{
		lock (DdlLock)
		{
			foreach (var dbType in m_created)
			{
				TryExecute(fixture, new[] { DropSql(dbType) }, out _);
			}

			m_created.Clear();
		}
	}

	/// <summary>
	/// Runs a CREATE, retrying it a few times: DDL from another test class (the type-fidelity tables) can win a metadata
	/// deadlock on Firebird, and the losing statement is rolled back, so running it again is safe.
	/// </summary>
	internal static bool TryCreate(IDbFactoryFixture fixture, string statement, out string failure)
	{
		for (var attempt = 1; ; attempt++)
		{
			if (TryExecute(fixture, new[] { statement }, out failure) || attempt == 3)
			{
				return failure is null;
			}

			System.Threading.Thread.Sleep(200 * attempt);
		}
	}

	/// <summary>
	/// Runs each statement on its own, opening a new connection after a failure (some drivers close it then);
	/// <paramref name="failure"/> is the first statement that failed and why.
	/// </summary>
	internal static bool TryExecute(IDbFactoryFixture fixture, IEnumerable<string> statements, out string failure)
	{
		failure = null;
		DbConnection connection = null;
		try
		{
			foreach (var statement in statements)
			{
				try
				{
					if (connection is null)
					{
						connection = fixture.Factory.CreateConnection();
						connection.ConnectionString = Environment.GetEnvironmentVariable("ConnectionString") ?? fixture.ConnectionString;
						connection.Open();
					}

					using var command = connection.CreateCommand();
					command.CommandText = statement;
					command.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					failure ??= $"{statement}: {ex.GetType().Name}: {ex.Message}";
					connection?.Dispose();
					connection = null;
				}
			}
		}
		finally
		{
			connection?.Dispose();
		}

		return failure is null;
	}
}

/// <summary>
/// Gives a provider whose own fixture has no select values the SQL-92 tables of <see cref="SelectValueTables"/>, over its
/// connection and SELECT shape. Override <see cref="ColumnTypes"/> and <see cref="BinaryLiteral"/> where the database
/// spells them differently.
/// </summary>
public class SelectValueFixture<TFactoryFixture> : ISelectValueFixture, IDeleteFixture, ISqlDialectFixture, IDisposable
	where TFactoryFixture : IDbFactoryFixture, new()
{
	private readonly TFactoryFixture m_inner = new();
	private readonly Lazy<SelectValueTables> m_tables;
	private readonly Lazy<IReadOnlyCollection<DbType>> m_supported;

	public SelectValueFixture()
	{
		m_tables = new Lazy<SelectValueTables>(() =>
			new SelectValueTables(Sql92Dml.UniqueName("sv_"), ColumnTypes, CreateTableSql, DropTableSql, SelectStatement, BinaryLiteral));
		m_supported = new Lazy<IReadOnlyCollection<DbType>>(() => m_tables.Value.Create(this));

		// Before any test holds a connection, for a database that allows one writer at a time.
		_ = m_supported.Value;
	}

	protected virtual IReadOnlyDictionary<DbType, string> ColumnTypes => SelectValueTables.DefaultColumnTypes;

	protected virtual string BinaryLiteral => SelectValueTables.DefaultBinaryLiteral;

	/// <summary>Creates a value table with an <c>Id</c> primary key and a column <c>V</c>; the inner fixture's SQL when it states one.</summary>
	protected virtual string CreateTableSql(string table, string column) =>
		m_inner is IDataTypesFidelityFixture tables ? tables.CreateTableSql(table, column) : Sql92Dml.DefaultCreateTable(table, column);

	protected virtual string DropTableSql(string table) =>
		m_inner is IDataTypesFidelityFixture tables ? tables.DropTableSql(table) : Sql92Dml.DefaultDropTable(table);

	public DbProviderFactory Factory => m_inner.Factory;

	public string ConnectionString => m_inner.ConnectionString;

	public string SelectStatement(string projection) => m_inner is ISqlDialectFixture dialect ? dialect.SelectStatement(projection) : $"SELECT {projection}";

	public string StatementTerminator => m_inner is ISqlDialectFixture dialect ? dialect.StatementTerminator : ";";

	public IReadOnlyCollection<DbType> SupportedDbTypes => m_supported.Value;

	public string CreateSelectSql(DbType dbType, ValueKind kind)
	{
		_ = m_supported.Value;
		return m_tables.Value.SelectSql(dbType, kind);
	}

	public string CreateSelectSql(byte[] value) => m_tables.Value.SelectSql(value);

	public string SelectNoRows
	{
		get
		{
			_ = m_supported.Value;
			return m_tables.Value.SelectNoRows;
		}
	}

	public string DeleteNoRows
	{
		get
		{
			_ = m_supported.Value;
			return m_tables.Value.DeleteNoRows;
		}
	}

	public Type NullValueExceptionType => typeof(InvalidCastException);

	public void Dispose()
	{
		if (m_supported.IsValueCreated)
		{
			m_tables.Value.Drop(this);
		}

		(m_inner as IDisposable)?.Dispose();
	}
}
