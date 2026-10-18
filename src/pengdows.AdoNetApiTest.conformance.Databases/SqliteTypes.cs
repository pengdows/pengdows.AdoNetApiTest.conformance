using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace pengdows.AdoNetApiTest.conformance.Databases;

/// <summary>
/// SQLite has no column types, only four storage classes (INTEGER, REAL, TEXT, BLOB) and a declared type name that sets an
/// affinity. What a provider returns for a value is therefore the provider's own contract, so each client has its own
/// catalog:
/// <list type="bullet">
/// <item><description><b>System.Data.SQLite</b> maps the declared type name to a CLR type (<c>smallint</c> is an
/// <see cref="short"/>, <c>datetime</c> a <see cref="DateTime"/>); its <c>DataTypes</c> collection lists the mapping and the
/// catalog follows it. <c>BinaryGUID</c> defaults to true, so a GUID is stored as a 16-byte blob.</description></item>
/// <item><description><b>Microsoft.Data.Sqlite</b> returns only <see cref="long"/>, <see cref="double"/>, <see cref="string"/> or
/// <see cref="byte"/>[] from <c>GetValue</c> and converts to the other CLR types in the typed read
/// (<see href="https://learn.microsoft.com/dotnet/standard/data/sqlite/types"/>), writing a <see cref="DateTime"/> as
/// <c>yyyy-MM-dd HH:mm:ss.FFFFFFF</c> text and a <see cref="decimal"/> as text because "REAL would be lossy".</description></item>
/// </list>
/// SQLite stores a REAL in 8 bytes, so a NUMERIC-affinity column keeps at most 15 significant digits: a decimal beyond that
/// is the database's limit, not the provider's, and the catalog keeps decimals inside it.
/// </summary>
public static class SqliteTypes
{
	private const string DefaultRender = "typeof({0}) || ':' || hex(CAST({0} AS BLOB))";
	private const string RealRender = "typeof({0}) || ':' || printf('%.17g', {0})";

	// declared before the catalogs: static initializers run in the order they are written
	private static readonly byte[] AllBytes = Enumerable.Range(0, 256).Select(i => (byte) i).ToArray();

	public static IReadOnlyList<TypeSpec> SystemDataSqlite { get; } = BuildSystemDataSqlite();

	public static IReadOnlyList<TypeSpec> MicrosoftDataSqlite { get; } = BuildMicrosoftDataSqlite();

	/// <summary>System.Data.SQLite lists many aliases of the same mapping; each is the same type under another name.</summary>
	public static IReadOnlyList<UncoveredType> SystemDataSqliteUncovered { get; } =
		new[]
		{
			"mediumint", "currency", "numeric", "yesno", "logical", "bool", "boolean", "counter", "autoincrement", "identity", "long",
			"binary", "varbinary", "image", "general", "oleobject", "memo", "longtext", "note", "ntext", "string", "nchar",
			"smalldate", "timestamp", "time", "guid",
		}.Select(name => new UncoveredType(name, "an alias of a covered type or a mapping the catalog shares with it")).ToList();

	public static string ParameterMarker(string name) => "@" + name;

	public static string CreateTableSql(string table, string columnSql) =>
		$"create table [{table}] (Id integer not null primary key, V {columnSql})";

	public static string DropTableSql(string table) => $"drop table if exists [{table}]";

	private static TypeValue V(string label, string literal, object expected) => new TypeValue(label, literal, expected);

	private static TypeValue Null() => V("null", "NULL", null);

	private static string Hex(byte[] bytes) => "X'" + string.Concat(bytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture))) + "'";

	private static string Quote(string text) => "'" + text.Replace("'", "''") + "'";

	private static TypeValue[] Strings(int size)
	{
		var values = new List<TypeValue>
		{
			V("empty", "''", string.Empty),
			V("ascii", "'abc'", "abc"),
			V("latin", Quote("héllo wörld"), "héllo wörld"),
			V("cjk", Quote("日本語"), "日本語"),
			V("non-BMP", Quote("\U0001F600"), "\U0001F600"),
			V("trailing spaces", "'abc  '", "abc  "),
			V("quote and backslash", Quote("it's \\ \"q\""), "it's \\ \"q\""),
			V("embedded NUL", "'a' || char(0) || 'b'", "a\0b"),
		};
		if (size > 0)
		{
			values.Add(V("long", $"replace(hex(zeroblob({size / 2})), '00', 'xx')", new string('x', size)));
		}

		values.Add(Null());
		return values.ToArray();
	}

	private static IReadOnlyList<TypeSpec> BuildSystemDataSqlite()
	{
		var guid = new Guid("33221100-5544-7766-9988-aabbccddeeff");
		TypeSpec Real(string name, Type clr, DbType dbType, params TypeValue[] values) => new TypeSpec(name, name, clr, dbType, values) { RenderAsText = RealRender };

		return new List<TypeSpec>
		{
			new TypeSpec("tinyint", "tinyint", typeof(byte), DbType.Byte, V("min", "0", (byte) 0), V("max", "255", (byte) 255), Null()) { RenderAsText = DefaultRender },
			new TypeSpec("smallint", "smallint", typeof(short), DbType.Int16, V("min", "-32768", short.MinValue), V("minus one", "-1", (short) -1), V("zero", "0", (short) 0), V("max", "32767", short.MaxValue), Null()) { RenderAsText = DefaultRender },
			new TypeSpec("int", "int", typeof(int), DbType.Int32, V("min", "-2147483648", int.MinValue), V("minus one", "-1", -1), V("zero", "0", 0), V("max", "2147483647", int.MaxValue), Null()) { RenderAsText = DefaultRender },
			new TypeSpec("integer", "integer", typeof(long), DbType.Int64, V("min", "-9223372036854775808", long.MinValue), V("minus one", "-1", -1L), V("zero", "0", 0L), V("max", "9223372036854775807", long.MaxValue), Null()) { RenderAsText = DefaultRender },
			new TypeSpec("bigint", "bigint", typeof(long), DbType.Int64, V("min", "-9223372036854775808", long.MinValue), V("max", "9223372036854775807", long.MaxValue), Null()) { RenderAsText = DefaultRender },
			new TypeSpec("bit", "bit", typeof(bool), DbType.Boolean, V("true", "1", true), V("false", "0", false), Null()) { RenderAsText = DefaultRender },

			Real("real", typeof(double), DbType.Double, V("zero", "0.0", 0d), V("one and a half", "1.5", 1.5), V("minus one and a half", "-1.5", -1.5), V("0.1", "0.1", 0.1),
				V("max", "1.7976931348623157e308", double.MaxValue), V("min normal", "2.2250738585072014e-308", 2.2250738585072014E-308), Null()),
			Real("float", typeof(double), DbType.Double, V("one and a half", "1.5", 1.5), V("0.1", "0.1", 0.1), Null()),
			Real("double", typeof(double), DbType.Double, V("one and a half", "1.5", 1.5), V("0.1", "0.1", 0.1), Null()),
			Real("single", typeof(float), DbType.Single, V("one and a half", "1.5", 1.5f), V("0.1", "0.10000000149011612", 0.1f), V("large", "3.3999999521443642e38", 3.4E+38f), Null()),

			new TypeSpec("decimal", "decimal(18,4)", typeof(decimal), DbType.Decimal,
				V("zero", "0", 0m), V("one and a half", "1.5", 1.5m), V("15 digits", "12345678901.2345", 12345678901.2345m), V("negative", "-12345.6789", -12345.6789m), V("smallest step", "0.0001", 0.0001m), Null())
			{
				Precision = 18,
				Scale = 4,
				RenderAsText = RealRender,
			},
			new TypeSpec("money", "money", typeof(decimal), DbType.Currency,
				V("max in 15 digits", "99999999999.9999", 99999999999.9999m), V("smallest step", "0.0001", 0.0001m), Null())
			{
				RenderAsText = RealRender,
			},

			new TypeSpec("blob", "blob", typeof(byte[]), DbType.Binary,
				V("empty", "X''", new byte[0]), V("one zero byte", "X'00'", new byte[] { 0 }), V("every byte value", Hex(AllBytes), AllBytes), Null()) { RenderAsText = DefaultRender },

			new TypeSpec("varchar", "varchar(50)", typeof(string), DbType.String, Strings(50)) { RenderAsText = DefaultRender },
			new TypeSpec("text", "text", typeof(string), DbType.String, Strings(10000)) { RenderAsText = DefaultRender },
			new TypeSpec("char", "char(10)", typeof(string), DbType.String, V("short", "'abc'", "abc"), V("empty", "''", string.Empty), Null()) { RenderAsText = DefaultRender },
			new TypeSpec("nvarchar", "nvarchar(50)", typeof(string), DbType.String, V("cjk", Quote("日本語"), "日本語"), V("non-BMP", Quote("\U0001F600"), "\U0001F600"), Null()) { RenderAsText = DefaultRender },

			new TypeSpec("datetime", "datetime", typeof(DateTime), DbType.DateTime,
				V("min", "'0001-01-01 00:00:00'", DateTime.MinValue), V("max", "'9999-12-31 23:59:59.9999999'", DateTime.MaxValue),
				V("100ns ticks", "'2020-01-02 03:04:05.1234567'", new DateTime(2020, 1, 2, 3, 4, 5).AddTicks(1234567)), Null())
			{
				WriteOracle = false,
			},
			new TypeSpec("date", "date", typeof(DateTime), DbType.Date,
				V("leap day", "'2020-02-29 00:00:00'", new DateTime(2020, 2, 29)), V("min", "'0001-01-01 00:00:00'", DateTime.MinValue), Null())
			{
				WriteOracle = false,
			},

			new TypeSpec("uniqueidentifier", "uniqueidentifier", typeof(Guid), DbType.Guid,
				V("empty", Hex(Guid.Empty.ToByteArray()), Guid.Empty), V("mixed bytes", Hex(guid.ToByteArray()), guid),
				V("all F", Hex(new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF").ToByteArray()), new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")), Null()) { RenderAsText = DefaultRender },
		};
	}

	private static IReadOnlyList<TypeSpec> BuildMicrosoftDataSqlite()
	{
		var guid = new Guid("33221100-5544-7766-9988-aabbccddeeff");
		var oneThirtyOne = new DateTime(2020, 1, 2, 3, 4, 5).AddTicks(1234567);
		var offset = new DateTimeOffset(oneThirtyOne, TimeSpan.FromHours(5.5));
		var negativeOffset = new DateTimeOffset(oneThirtyOne, TimeSpan.FromHours(-8));

		TypeSpec Integer(string clr, Type typed, DbType dbType, params TypeValue[] values) =>
			new TypeSpec("INTEGER", "INTEGER", typeof(long), dbType, values) { TypedClrType = typed, Display = "INTEGER as " + clr, RenderAsText = DefaultRender };
		TypeSpec Text(string clr, Type typed, DbType dbType, params TypeValue[] values) =>
			new TypeSpec("TEXT", "TEXT", typeof(string), dbType, values) { TypedClrType = typed, Display = "TEXT as " + clr, RenderAsText = DefaultRender };

		return new List<TypeSpec>
		{
			Integer("Byte", typeof(byte), DbType.Byte, V("min", "0", 0L).TypedAs((byte) 0), V("max", "255", 255L).TypedAs((byte) 255), Null()),
			Integer("Int16", typeof(short), DbType.Int16, V("min", "-32768", -32768L).TypedAs(short.MinValue), V("max", "32767", 32767L).TypedAs(short.MaxValue), Null()),
			Integer("Int32", typeof(int), DbType.Int32, V("min", "-2147483648", -2147483648L).TypedAs(int.MinValue), V("max", "2147483647", 2147483647L).TypedAs(int.MaxValue), Null()),
			Integer("Int64", typeof(long), DbType.Int64, V("min", "-9223372036854775808", long.MinValue), V("max", "9223372036854775807", long.MaxValue), V("zero", "0", 0L), Null()),
			Integer("Boolean", typeof(bool), DbType.Boolean, V("true", "1", 1L).TypedAs(true), V("false", "0", 0L).TypedAs(false), Null()),

			new TypeSpec("REAL", "REAL", typeof(double), DbType.Double,
				V("zero", "0.0", 0d), V("one and a half", "1.5", 1.5), V("minus one and a half", "-1.5", -1.5), V("0.1", "0.1", 0.1),
				V("max", "1.7976931348623157e308", double.MaxValue), V("min normal", "2.2250738585072014e-308", 2.2250738585072014E-308), Null())
			{
				Display = "REAL as Double",
				RenderAsText = RealRender,
			},
			new TypeSpec("REAL", "REAL", typeof(double), DbType.Single,
				V("one and a half", "1.5", 1.5).TypedAs(1.5f), V("0.1", "0.10000000149011612", (double) 0.1f).TypedAs(0.1f), V("large", "3.3999999521443642e38", (double) 3.4E+38f).TypedAs(3.4E+38f), Null())
			{
				TypedClrType = typeof(float),
				Display = "REAL as Single",
				RenderAsText = RealRender,
			},

			new TypeSpec("TEXT", "TEXT", typeof(string), DbType.String, Strings(10000)) { Display = "TEXT as String", RenderAsText = DefaultRender },
			Text("Char", typeof(char), DbType.StringFixedLength, V("ascii", "'a'", "a").TypedAs('a'), V("latin", Quote("é"), "é").TypedAs('é'), V("cjk", Quote("日"), "日").TypedAs('日'), Null()),
			Text("DateTime", typeof(DateTime), DbType.DateTime,
				V("min", "'0001-01-01 00:00:00'", "0001-01-01 00:00:00").TypedAs(DateTime.MinValue),
				V("max", "'9999-12-31 23:59:59.9999999'", "9999-12-31 23:59:59.9999999").TypedAs(DateTime.MaxValue),
				V("100ns ticks", "'2020-01-02 03:04:05.1234567'", "2020-01-02 03:04:05.1234567").TypedAs(oneThirtyOne),
				V("whole seconds", "'2020-01-02 03:04:05'", "2020-01-02 03:04:05").TypedAs(new DateTime(2020, 1, 2, 3, 4, 5)), Null()),
			Text("DateTimeOffset", typeof(DateTimeOffset), DbType.DateTimeOffset,
				V("+05:30", "'2020-01-02 03:04:05.1234567+05:30'", "2020-01-02 03:04:05.1234567+05:30").TypedAs(offset),
				V("-08:00", "'2020-01-02 03:04:05.1234567-08:00'", "2020-01-02 03:04:05.1234567-08:00").TypedAs(negativeOffset), Null()),
			Text("Decimal", typeof(decimal), DbType.Decimal,
				V("zero", "'0.0'", "0.0").TypedAs(0m), V("one and a half", "'1.5'", "1.5").TypedAs(1.5m),
				V("28 digits", "'12345678901234567890.1234567890'", "12345678901234567890.1234567890").TypedAs(12345678901234567890.1234567890m),
				V("negative", "'-12345.6789'", "-12345.6789").TypedAs(-12345.6789m), Null()),
			Text("Guid", typeof(Guid), DbType.Guid,
				V("empty", "'00000000-0000-0000-0000-000000000000'", "00000000-0000-0000-0000-000000000000").TypedAs(Guid.Empty),
				V("mixed bytes", "'33221100-5544-7766-9988-AABBCCDDEEFF'", "33221100-5544-7766-9988-AABBCCDDEEFF").TypedAs(guid), Null()),
			Text("TimeSpan", typeof(TimeSpan), DbType.Time,
				V("zero", "'00:00:00'", "00:00:00").TypedAs(TimeSpan.Zero),
				V("under a day", "'23:59:59.9999999'", "23:59:59.9999999").TypedAs(TimeSpan.FromTicks(TimeSpan.TicksPerDay - 1)),
				V("days and ticks", "'1.02:03:04.5000000'", "1.02:03:04.5000000").TypedAs(new TimeSpan(1, 2, 3, 4, 500)),
				V("negative", "'-1.02:03:04.5000000'", "-1.02:03:04.5000000").TypedAs(-new TimeSpan(1, 2, 3, 4, 500)), Null()),

			new TypeSpec("BLOB", "BLOB", typeof(byte[]), DbType.Binary,
				V("empty", "X''", new byte[0]), V("one zero byte", "X'00'", new byte[] { 0 }), V("every byte value", Hex(AllBytes), AllBytes), Null())
			{
				Display = "BLOB as Byte[]",
				RenderAsText = DefaultRender,
			},
		}.Select(Normalize).ToList();
	}

	// The storage text of a GUID is the provider's choice of case, and a TimeSpan's storage text is its own format.
	private static TypeSpec Normalize(TypeSpec spec)
	{
		if (spec.Display.EndsWith("Guid", StringComparison.Ordinal))
		{
			spec.IgnoreCase = true;
			spec.RenderAsText = "typeof({0}) || ':' || upper({0})";
		}

		if (spec.Display.EndsWith("TimeSpan", StringComparison.Ordinal))
		{
			spec.RawFormatIsProviderDefined = true;
			spec.WriteOracle = false;
		}

		return spec;
	}
}
