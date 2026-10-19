using System;
using System.Collections.Generic;
using System.Data;
using System.Net;

namespace pengdows.AdoNetApiTest.conformance.Databases;

/// <summary>
/// The PostgreSQL types and the values that show whether a provider keeps them intact. The expected values are what the
/// database stores and a correct provider returns: a <c>timestamptz</c> is an instant (read back in UTC), and an
/// <c>interval</c> month counts as 30 days, as PostgreSQL's own <c>extract(epoch from interval '1 mon')</c> does.
/// </summary>
public static class PostgreSqlTypes
{
	private const string AsText = "CAST({0} AS text)";

	public static IReadOnlyList<TypeSpec> Types { get; } = Build();

	public static IReadOnlyList<UncoveredType> UncoveredTypes { get; } = new[]
	{
		new UncoveredType("cidr", "its CLR form differs across Npgsql versions (IPAddress pairs, NpgsqlCidr)"),
		new UncoveredType("macaddr", "PhysicalAddress has no portable literal form"),
		new UncoveredType("money", "its text form follows the server's lc_monetary"),
		new UncoveredType("json", "the value is text and the provider's JSON support is a separate capability"),
		new UncoveredType("jsonb", "the value is text and the provider's JSON support is a separate capability"),
		new UncoveredType("xml", "the value is text"),
		new UncoveredType("bit", "BitArray has no value equality"),
		new UncoveredType("int4range", "ranges are provider types (NpgsqlRange)"),
		new UncoveredType("point", "geometric types are provider types"),
		new UncoveredType("tsvector", "full-text types are provider types"),
	};

	public static string ParameterMarker(string name) => "@" + name;

	public static string CreateTableSql(string table, string columnSql) =>
		$"create table {table} (Id integer not null primary key, V {columnSql} null)";

	public static string DropTableSql(string table) => $"drop table if exists {table}";

	private static TypeValue V(string label, string literal, object expected) => new TypeValue(label, literal, expected);

	private static TypeValue Null() => V("null", "NULL", null);

	private static IReadOnlyList<TypeSpec> Build()
	{
		var guid = new Guid("33221100-5544-7766-9988-aabbccddeeff");
		var instant = new DateTime(2024, 1, 1, 7, 0, 0, DateTimeKind.Utc);

		return new List<TypeSpec>
		{
			new TypeSpec("smallint", "smallint", typeof(short), DbType.Int16,
				V("min", "CAST(-32768 AS smallint)", short.MinValue), V("zero", "CAST(0 AS smallint)", (short) 0),
				V("max", "CAST(32767 AS smallint)", short.MaxValue), Null()) { RenderAsText = AsText },
			new TypeSpec("integer", "integer", typeof(int), DbType.Int32,
				V("min", "-2147483648", int.MinValue), V("zero", "0", 0), V("max", "2147483647", int.MaxValue), Null()) { RenderAsText = AsText },
			new TypeSpec("bigint", "bigint", typeof(long), DbType.Int64,
				V("min", "CAST(-9223372036854775808 AS bigint)", long.MinValue), V("zero", "CAST(0 AS bigint)", 0L),
				V("max", "CAST(9223372036854775807 AS bigint)", long.MaxValue), Null()) { RenderAsText = AsText },
			new TypeSpec("boolean", "boolean", typeof(bool), DbType.Boolean,
				V("true", "true", true), V("false", "false", false), Null()) { RenderAsText = AsText },

			new TypeSpec("real", "real", typeof(float), DbType.Single,
				V("zero", "CAST(0 AS real)", 0f), V("one and a half", "CAST(1.5 AS real)", 1.5f), V("0.1", "CAST(0.1 AS real)", 0.1f),
				V("large", "CAST(3.4E+38 AS real)", 3.4E+38f), Null()) { RenderAsText = AsText },
			// ODP.NET bound DbType.Double as NUMBER and overflowed beyond 1e126 (pengdows.crud TYPE-002); the large values
			// show whether a provider binds a double as a double.
			new TypeSpec("double precision", "double precision", typeof(double), DbType.Double,
				V("zero", "CAST(0 AS double precision)", 0d), V("0.1", "CAST(0.1 AS double precision)", 0.1),
				V("1e200", "CAST(1e200 AS double precision)", 1e200), V("minus 1e200", "CAST(-1e200 AS double precision)", -1e200),
				V("max", "CAST(1.7976931348623157E+308 AS double precision)", double.MaxValue),
				V("min normal", "CAST(2.2250738585072014E-308 AS double precision)", 2.2250738585072014E-308), Null()) { RenderAsText = AsText },
			new TypeSpec("numeric", "numeric(28,10)", typeof(decimal), DbType.Decimal,
				V("zero", "CAST(0 AS numeric(28,10))", 0m),
				V("max", "CAST('999999999999999999.9999999999' AS numeric(28,10))", 999999999999999999.9999999999m),
				V("smallest step", "CAST('0.0000000001' AS numeric(28,10))", 0.0000000001m), Null()) { RenderAsText = AsText },

			new TypeSpec("text", "text", typeof(string), DbType.String,
				V("empty", "''", string.Empty), V("ascii", "'abc'", "abc"), V("non-BMP", "'\U0001F600'", "\U0001F600"),
				V("quote", "'it''s'", "it's"), Null()) { RenderAsText = AsText },
			new TypeSpec("bytea", "bytea", typeof(byte[]), DbType.Binary,
				V("empty", "'\\x'::bytea", Array.Empty<byte>()), V("bytes", "'\\x00ff10'::bytea", new byte[] { 0, 255, 16 }), Null()) { RenderAsText = AsText },
			new TypeSpec("uuid", "uuid", typeof(Guid), DbType.Guid,
				V("empty", "'00000000-0000-0000-0000-000000000000'::uuid", Guid.Empty), V("mixed bytes", "'33221100-5544-7766-9988-aabbccddeeff'::uuid", guid), Null())
			{
				RenderAsText = AsText,
				IgnoreCase = true,
			},

			new TypeSpec("date", "date", typeof(DateTime), DbType.Date,
				V("leap day", "DATE '2024-02-29'", new DateTime(2024, 2, 29)), V("min", "DATE '0001-01-01'", new DateTime(1, 1, 1)), Null()) { RenderAsText = AsText },
			new TypeSpec("timestamp", "timestamp", typeof(DateTime), DbType.DateTime,
				V("microseconds", "TIMESTAMP '2024-01-01 12:34:56.123456'", new DateTime(2024, 1, 1, 12, 34, 56).AddTicks(1234560)), Null()) { RenderAsText = AsText },
			// Npgsql 6+ rejects a DateTimeOffset whose Offset is not zero for timestamptz; pengdows.crud converts to UTC first.
			new TypeSpec("timestamp with time zone", "timestamptz", typeof(DateTime), DbType.DateTimeOffset,
				V("utc", "TIMESTAMPTZ '2024-01-01 07:00:00+00'", instant).Binding(new DateTimeOffset(2024, 1, 1, 7, 0, 0, TimeSpan.Zero))
					.TypedAs(new DateTimeOffset(2024, 1, 1, 7, 0, 0, TimeSpan.Zero)),
				V("plus five hours", "TIMESTAMPTZ '2024-01-01 12:00:00+05'", instant).Binding(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.FromHours(5)))
					.TypedAs(new DateTimeOffset(2024, 1, 1, 7, 0, 0, TimeSpan.Zero)),
				Null())
			{
				RenderAsText = "CAST({0} AT TIME ZONE 'UTC' AS text)",
				TypedClrType = typeof(DateTimeOffset),
			},
			new TypeSpec("time", "time", typeof(TimeSpan), DbType.Time,
				V("microseconds", "TIME '13:45:30.123456'", new TimeSpan(13, 45, 30).Add(TimeSpan.FromTicks(1234560))), V("midnight", "TIME '00:00:00'", TimeSpan.Zero), Null())
			{
				RenderAsText = AsText,
			},
			new TypeSpec("interval", "interval", typeof(TimeSpan), DbType.Object,
				V("day and time", "INTERVAL '1 day 02:03:04.5'", new TimeSpan(1, 2, 3, 4, 500)), V("negative", "INTERVAL '-02:00:00'", TimeSpan.FromHours(-2)), Null())
			{
				RenderAsText = AsText,
			},
			// Npgsql's GetValue returns TimeSpan for interval: a positive month throws and a negative one is dropped
			// silently (pengdows.crud reads NpgsqlInterval instead). A month is bound as 30 days, so the stored text differs
			// from the literal's by design and the write path does not apply.
			new TypeSpec("interval", "interval", typeof(TimeSpan), DbType.Object,
				V("one month", "INTERVAL '1 mon'", TimeSpan.FromDays(30)), V("minus one month", "INTERVAL '-1 mon'", TimeSpan.FromDays(-30)),
				V("month and day", "INTERVAL '1 mon 1 day'", TimeSpan.FromDays(31)), Null())
			{
				Display = "interval (months)",
				RenderAsText = AsText,
				WriteOracle = false,
			},
			// Npgsql's GetValue returns IPAddress for inet and drops the netmask (pengdows.crud reads NpgsqlInet). An IPAddress
			// has no netmask, so the masked value is bound without one and cannot round-trip either.
			new TypeSpec("inet", "inet", typeof(IPAddress), DbType.Object,
				V("host", "'192.168.1.10'::inet", IPAddress.Parse("192.168.1.10")), V("ipv6", "'::1'::inet", IPAddress.Parse("::1")),
				V("netmask", "'192.168.1.10/24'::inet", new ClrText("192.168.1.10/24")).Binding(IPAddress.Parse("192.168.1.10")), Null())
			{
				RenderAsText = AsText,
			},
		};
	}
}
