using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Pushes every value of every type in a catalog through a provider and records how faithfully it came back. Each value
/// takes three paths so a failure says where it happened: <c>read</c> (a SQL literal read through the provider),
/// <c>write</c> (a parameter, compared with the literal by how the database itself renders them) and <c>roundtrip</c>
/// (a parameter read back through the provider, with the CLR type, <c>GetFieldValue</c> and a <c>WHERE</c> equality).
/// A value without a literal takes only the <c>roundtrip</c> path. The catalog is hand-written
/// (<see cref="TypeFidelityTestBase{TFixture}"/>) or generated from the provider's own <c>DataTypes</c> collection
/// (<see cref="DataTypesFidelityTestBase{TFixture}"/>).
/// </summary>
public abstract class FidelityTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	protected const string TablePrefix = "conformance_type";

	// One table per type: a provider whose statement cache keys on SQL text (ODP.NET) cannot re-run a cached query once
	// its table is dropped and recreated with another column type. Query_runs_after_its_table_is_recreated reports that.
	private string Table { get; set; } = TablePrefix;

	private static readonly MethodInfo s_getFieldValue = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetFieldValue));

	protected FidelityTestBase(TFixture fixture)
		: base(fixture)
	{
	}

	/// <summary>The types to exercise.</summary>
	protected abstract IReadOnlyList<TypeSpec> Specs { get; }

	/// <summary>A statement that creates <paramref name="table"/> with an <c>Id</c> primary key and a nullable column <c>V</c>.</summary>
	protected abstract string CreateTableSql(string table, string columnSql);

	/// <summary>A statement that drops <paramref name="table"/>.</summary>
	protected abstract string DropTableSql(string table);

	/// <summary>The parameter <paramref name="name"/> as it appears in SQL.</summary>
	protected abstract string ParameterMarker(string name);

	[Flags]
	protected enum Paths
	{
		Read = 1,
		Write = 2,
		RoundTrip = 4,
	}

	private sealed class Tally
	{
		public int Checks { get; set; }

		public int NotExact { get; set; }
	}

	private DbConnection _connection;

	/// <summary>The open connection; one the provider closed after an error (Npgsql does, for a rejected parameter value) is replaced.</summary>
	private DbConnection Connection
	{
		get
		{
			if (_connection is { State: ConnectionState.Open })
			{
				return _connection;
			}

			_connection?.Dispose();
			_connection = CreateOpenConnection();
			return _connection;
		}
	}

	protected void Run(Paths paths)
	{
		var tally = new Tally();
		try
		{
			RunTypes(paths, tally);
		}
		finally
		{
			_connection?.Dispose();
			_connection = null;
		}

		if (tally.NotExact > 0)
		{
			ConformanceOutcome.Different($"{tally.NotExact} of {tally.Checks} type-fidelity checks were not exact; the findings say which.");
		}
	}

	private void RunTypes(Paths paths, Tally tally)
	{
		var specs = Specs;
		for (var index = 0; index < specs.Count; index++)
		{
			var spec = specs[index];
			// Each test gets its own tables too: a pooled connection (Firebird's) can keep the previous test's table in use.
			Table = TablePrefix + "_" + ((int) paths).ToString(CultureInfo.InvariantCulture) + "_" + index.ToString(CultureInfo.InvariantCulture);
			if (!CreateColumn(spec, tally))
			{
				continue;
			}

			try
			{
				foreach (var value in spec.Values)
				{
					Evaluate(spec, value, paths, tally);
				}

				if (paths.HasFlag(Paths.RoundTrip))
				{
					Probe(spec, tally);
				}
			}
			finally
			{
				try
				{
					Execute(Connection, DropTableSql(Table));
				}
				catch (DbException)
				{
				}
			}
		}
	}

	/// <summary>
	/// Creates the table for <paramref name="spec"/>. A rejected column definition is a finding; a spec with a
	/// <see cref="TypeSpec.FallbackColumnSql"/> (a <c>DataTypes</c> <c>CreateFormat</c> the database refuses) then tries that.
	/// </summary>
	private bool CreateColumn(TypeSpec spec, Tally tally)
	{
		DropQuietly();
		if (spec.FallbackColumnSql is null)
		{
			return Attempt(tally, spec.Display, "(column)", "read", () => Execute(Connection, CreateTableSql(Table, spec.ColumnSql)));
		}

		try
		{
			Execute(Connection, CreateTableSql(Table, spec.ColumnSql));
			return true;
		}
		catch (Exception exception) when (!(exception is Xunit.Sdk.XunitException))
		{
			Record(tally, spec, new TypeValue("(column)", null, null), "metadata", new FidelityResult(FidelityStatus.Throws,
				$"the database rejects the advertised column definition {spec.ColumnSql}: {exception.GetType().Name}: {FirstLine(exception.Message)}; using {spec.FallbackColumnSql}"));
		}

		DropQuietly();
		var fallback = spec.FallbackColumnSql;
		return Attempt(tally, spec.Display, "(column)", "read", () => Execute(Connection, CreateTableSql(Table, fallback)));
	}

	/// <summary>True when the database can render the stored value as text with <see cref="TypeSpec.RenderAsText"/>.</summary>
	private bool Renders(TypeSpec spec)
	{
		try
		{
			ReadScalar(Connection, $"select {string.Format(CultureInfo.InvariantCulture, spec.RenderAsText, "V")} from {Table} where Id = 1");
			return true;
		}
		catch (Exception exception) when (!(exception is Xunit.Sdk.XunitException))
		{
			return false;
		}
	}

	private void DropQuietly()
	{
		try
		{
			Execute(Connection, DropTableSql(Table));
		}
		catch (Exception exception) when (!(exception is Xunit.Sdk.XunitException))
		{
		}
	}

	private void Evaluate(TypeSpec spec, TypeValue value, Paths paths, Tally tally)
	{
		if (!Attempt(tally, spec.Display, value.Label, "read", () => Execute(Connection, $"delete from {Table}")))
		{
			return;
		}

		bool literalStored;
		if (value.Literal is not null && spec.Generated)
		{
			try
			{
				Execute(Connection, $"insert into {Table} (Id, V) values (1, {value.Literal})");
				literalStored = true;
			}
			catch (Exception exception) when (!(exception is Xunit.Sdk.XunitException))
			{
				Record(tally, spec, value, "read", new FidelityResult(FidelityStatus.NotApplicable,
					$"the database rejects this value even as a literal, so it is outside the type's range there: {FirstLine(exception.Message)}"));
				return;
			}
		}
		else
		{
			literalStored = value.Literal is not null && Attempt(tally, spec.Display, value.Label, "read",
				() => Execute(Connection, $"insert into {Table} (Id, V) values (1, {value.Literal})"));
		}
		// A database that stores an empty value as NULL (Oracle) does so for the literal as well; that is its rule.
		if (literalStored && spec.Generated && value.ParameterValue is string { Length: 0 } or byte[] { Length: 0 }
			&& ReadScalar(Connection, $"select V from {Table} where Id = 1") is null or DBNull)
		{
			Record(tally, spec, value, "read", new FidelityResult(FidelityStatus.NotApplicable, "the database stores an empty value as NULL"));
			return;
		}

		if (literalStored && paths.HasFlag(Paths.Read))
		{
			Attempt(tally, spec.Display, value.Label, "read", () =>
			{
				var read = Read(Connection, spec, "select V from " + Table + " where Id = 1", null);
				Record(tally, spec, value, "read", RawCompare(spec, value, read.Value));
			});
		}

		if (!paths.HasFlag(Paths.Write) && !paths.HasFlag(Paths.RoundTrip))
		{
			return;
		}

		var parameterStored = Attempt(tally, spec.Display, value.Label, paths.HasFlag(Paths.Write) ? "write" : "roundtrip", () =>
			Execute(Connection, $"insert into {Table} (Id, V) values (2, {ParameterMarker("p")})", command => Bind(command, spec, value)));
		if (!parameterStored)
		{
			return;
		}

		if (paths.HasFlag(Paths.Write) && value.Literal is null)
		{
			Record(tally, spec, value, "write", new FidelityResult(FidelityStatus.NotApplicable, "no literal to compare the stored value against"));
		}
		else if (paths.HasFlag(Paths.Write) && !spec.WriteOracle)
		{
			Record(tally, spec, value, "write", new FidelityResult(FidelityStatus.NotApplicable, "the database defines no text for this stored value to compare against"));
		}
		else if (paths.HasFlag(Paths.Write) && literalStored)
		{
			if (spec.Generated && !Renders(spec))
			{
				Record(tally, spec, value, "write", new FidelityResult(FidelityStatus.NotApplicable, "the database cannot render this type as text"));
			}
			else
			{
				Attempt(tally, spec.Display, value.Label, "write", () =>
				{
					var render = string.Format(CultureInfo.InvariantCulture, spec.RenderAsText, "V");
					var fromLiteral = Convert.ToString(ReadScalar(Connection, $"select {render} from {Table} where Id = 1"), CultureInfo.InvariantCulture);
					var fromParameter = Convert.ToString(ReadScalar(Connection, $"select {render} from {Table} where Id = 2"), CultureInfo.InvariantCulture);
					Record(tally, spec, value, "write", string.Equals(fromLiteral, fromParameter, StringComparison.Ordinal)
						? new FidelityResult(FidelityStatus.Exact, null)
						: new FidelityResult(FidelityStatus.Lossy, $"the database renders the literal as {FidelityComparer.Show(fromLiteral)} but the parameter as {FidelityComparer.Show(fromParameter)}"));
				});
			}
		}

		if (!paths.HasFlag(Paths.RoundTrip))
		{
			return;
		}

		Attempt(tally, spec.Display, value.Label, "roundtrip", () =>
		{
			var read = Read(Connection, spec, "select V from " + Table + " where Id = 2", null);
			Record(tally, spec, value, "roundtrip", RawCompare(spec, value, read.Value));

			if (value.Expected is not null)
			{
				Record(tally, spec, value, "type", spec.RawFormatIsProviderDefined
					? new FidelityResult(FidelityStatus.NotApplicable, "the storage form is the provider's own")
					: read.FieldType == spec.ClrType && read.Value.GetType() == spec.ClrType
					? new FidelityResult(FidelityStatus.Exact, null)
					: new FidelityResult(FidelityStatus.WrongType, $"expected {spec.ClrType.Name} but GetFieldType is {read.FieldType?.Name} and GetValue is {read.Value?.GetType().Name}"));
				Record(tally, spec, value, "typed", read.TypedFailure is not null
					? new FidelityResult(FidelityStatus.Throws, read.TypedFailure)
					: FidelityComparer.Compare(value.ExpectedTyped, read.Typed, spec.IgnoreCase));
			}
		});

		if (!spec.Comparable || value.Expected is null)
		{
			Record(tally, spec, value, "where", new FidelityResult(FidelityStatus.NotApplicable, spec.Comparable ? "NULL never equals NULL" : "the database cannot compare this type with ="));
			return;
		}

		Attempt(tally, spec.Display, value.Label, "where", () =>
		{
			var expectedRows = literalStored ? 2 : 1;
			var found = Convert.ToInt32(ReadScalar(Connection, $"select count(*) from {Table} where V = {ParameterMarker("p")}", command => Bind(command, spec, value)), CultureInfo.InvariantCulture);
			Record(tally, spec, value, "where", found == expectedRows
				? new FidelityResult(FidelityStatus.Exact, null)
				: new FidelityResult(FidelityStatus.Lossy, $"WHERE V = parameter matched {found} of {expectedRows} rows"));
		});
	}

	/// <summary>
	/// Values a driver must either store as given or refuse, never silently store as another value: a <c>TimeSpan</c>
	/// outside a day in a TIME column (drivers have wrapped -01:00 to 23:00), a <c>DateTimeOffset</c> in a column with no
	/// offset (drivers have stored its local wall time, so 13:45+02:00 read back two hours off), and a <c>DateTime</c>
	/// declared <c>DbType.DateTimeOffset</c> (drivers have sent it as NULL). Refusing is exact; a different value or NULL is
	/// lossy. Each runs once per type on the round-trip path.
	/// </summary>
	private void Probe(TypeSpec spec, Tally tally)
	{
		if (spec.DbType == DbType.Time && spec.ClrType == typeof(TimeSpan)
			&& spec.Name.IndexOf("interval", StringComparison.OrdinalIgnoreCase) < 0)
		{
			ProbeValue(spec, tally, "beyond a day", DbType.Time, new TimeSpan(1, 1, 0, 0), stored => stored);
			ProbeValue(spec, tally, "negative", DbType.Time, TimeSpan.FromHours(-1), stored => stored);
		}

		// A DATE column holds no time of day, so it is right to drop one; only types with a time are probed.
		if (spec.ClrType == typeof(DateTime) && spec.DbType != DbType.Date
			&& !(spec.Name.IndexOf("date", StringComparison.OrdinalIgnoreCase) >= 0 && spec.Name.IndexOf("time", StringComparison.OrdinalIgnoreCase) < 0))
		{
			var instant = new DateTimeOffset(2024, 1, 2, 13, 45, 0, TimeSpan.FromHours(2));
			ProbeValue(spec, tally, "DateTimeOffset with offset", spec.DbType, instant, stored => AsInstant(stored, instant.UtcDateTime));
			var utc = new DateTime(2024, 1, 2, 11, 45, 0, DateTimeKind.Utc);
			ProbeValue(spec, tally, "DateTime declared DateTimeOffset", DbType.DateTimeOffset, utc, stored => AsInstant(stored, utc) is DateTimeOffset o
				? (object) o.UtcDateTime
				: stored);
		}
	}

	/// <summary>
	/// The instant a stored timestamp names, when it names <paramref name="utc"/>: a <c>DateTimeOffset</c> as read, a local
	/// <c>DateTime</c> converted to UTC, and an unspecified or UTC <c>DateTime</c> taken as UTC wall time. Anything else is
	/// returned unchanged so the finding shows it.
	/// </summary>
	private static object AsInstant(object stored, DateTime utc)
	{
		var instant = stored switch
		{
			DateTimeOffset o => o.UtcDateTime,
			DateTime { Kind: DateTimeKind.Local } d => d.ToUniversalTime(),
			DateTime d => DateTime.SpecifyKind(d, DateTimeKind.Utc),
			_ => (DateTime?) null,
		};
		return instant is { } i && i.Ticks == utc.Ticks ? new DateTimeOffset(utc, TimeSpan.Zero) : stored;
	}

	private void ProbeValue(TypeSpec spec, Tally tally, string label, DbType dbType, object value, Func<object, object> normalize)
	{
		var probe = new TypeValue(label, null, value);
		if (!Attempt(tally, spec.Display, label, "probe", () => Execute(Connection, $"delete from {Table}")))
		{
			return;
		}

		try
		{
			Execute(Connection, $"insert into {Table} (Id, V) values (3, {ParameterMarker("p")})", command =>
			{
				var parameter = command.CreateParameter();
				parameter.ParameterName = "p";
				parameter.DbType = dbType;
				parameter.Value = value;
				command.Parameters.Add(parameter);
			});
		}
		catch (Exception exception) when (!(exception is Xunit.Sdk.XunitException))
		{
			Record(tally, spec, probe, "probe", new FidelityResult(FidelityStatus.Exact, "refused: " + FirstLine(exception.Message)));
			return;
		}

		Attempt(tally, spec.Display, label, "probe", () =>
		{
			var stored = ReadScalar(Connection, $"select V from {Table} where Id = 3");
			if (stored is null or DBNull)
			{
				Record(tally, spec, probe, "probe", new FidelityResult(FidelityStatus.Lossy, "the value was stored as NULL"));
				return;
			}

			var comparable = normalize(stored);
			Record(tally, spec, probe, "probe", Equals(comparable, value)
				|| (comparable is DateTime d && value is DateTime v && d.Ticks == v.Ticks)
				|| (comparable is DateTimeOffset o && value is DateTimeOffset w && o == w)
				? new FidelityResult(FidelityStatus.Exact, null)
				: new FidelityResult(FidelityStatus.Lossy, $"stored {FidelityComparer.Show(stored)} instead of {FidelityComparer.Show(value)}"));
		});
	}

	private static FidelityResult RawCompare(TypeSpec spec, TypeValue value, object actual) =>
		spec.RawFormatIsProviderDefined
			? new FidelityResult(FidelityStatus.NotApplicable, "the storage form is the provider's own")
			: FidelityComparer.Compare(value.Expected, actual, spec.IgnoreCase);

	/// <summary>Runs a step; an exception from the provider is a finding, not a test failure.</summary>
	private bool Attempt(Tally tally, string type, string value, string path, Action step)
	{
		try
		{
			step();
			return true;
		}
		catch (Exception exception) when (!(exception is Xunit.Sdk.XunitException))
		{
			tally.Checks++;
			tally.NotExact++;
			var closed = _connection is { State: not ConnectionState.Open } ? " (and the provider closed the connection)" : string.Empty;
			ConformanceOutcome.Finding(type, value, path, FidelityStatus.Throws, exception.GetType().Name + ": " + FirstLine(exception.Message) + closed);
			return false;
		}
	}

	private static void Record(Tally tally, TypeSpec spec, TypeValue value, string path, FidelityResult result)
	{
		tally.Checks++;
		if (result.Status != FidelityStatus.Exact && result.Status != FidelityStatus.NotApplicable)
		{
			tally.NotExact++;
		}

		ConformanceOutcome.Finding(spec.Display, value.Label, path, result.Status, result.Detail);
	}

	protected static string FirstLine(string message)
	{
		var end = message.IndexOfAny(new[] { '\r', '\n' });
		var line = end < 0 ? message : message.Substring(0, end);
		return line.Length > 200 ? line.Substring(0, 200) : line;
	}

	private static void Bind(DbCommand command, TypeSpec spec, TypeValue value)
	{
		var parameter = command.CreateParameter();
		parameter.ParameterName = "p";
		if (!spec.InferDbType || value.ParameterValue is null)
		{
			parameter.DbType = spec.DbType;
		}

		if (spec.Size.HasValue)
		{
			parameter.Size = spec.Size.Value;
		}

		var numeric = (IDbDataParameter) parameter;
		if (spec.Precision.HasValue)
		{
			numeric.Precision = spec.Precision.Value;
		}

		if (spec.Scale.HasValue)
		{
			numeric.Scale = spec.Scale.Value;
		}

		parameter.Value = value.ParameterValue ?? DBNull.Value;
		command.Parameters.Add(parameter);
	}

	protected static void Execute(DbConnection connection, string sql, Action<DbCommand> bind = null)
	{
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		bind?.Invoke(command);
		command.ExecuteNonQuery();
	}

	protected static object ReadScalar(DbConnection connection, string sql, Action<DbCommand> bind = null)
	{
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		bind?.Invoke(command);
		return command.ExecuteScalar();
	}

	private sealed class ReadResult
	{
		public object Value { get; set; }

		public Type FieldType { get; set; }

		public object Typed { get; set; }

		public string TypedFailure { get; set; }
	}

	private static ReadResult Read(DbConnection connection, TypeSpec spec, string sql, Action<DbCommand> bind)
	{
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		bind?.Invoke(command);
		using var reader = command.ExecuteReader();
		if (!reader.Read())
		{
			throw new InvalidOperationException("the row that was just inserted could not be read back");
		}

		var result = new ReadResult { Value = reader.GetValue(0), FieldType = reader.GetFieldType(0) };
		if (!reader.IsDBNull(0))
		{
			try
			{
				result.Typed = s_getFieldValue.MakeGenericMethod(spec.TypedClrType).Invoke(reader, new object[] { 0 });
			}
			catch (TargetInvocationException exception)
			{
				result.TypedFailure = exception.InnerException?.GetType().Name + ": " + FirstLine(exception.InnerException?.Message ?? string.Empty);
			}
		}

		return result;
	}
}
