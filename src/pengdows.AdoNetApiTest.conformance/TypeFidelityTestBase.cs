using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Pushes every value of every cataloged type through a provider and records how faithfully it came back. Each value
/// takes three paths so a failure says where it happened: <c>read</c> (a SQL literal read through the provider),
/// <c>write</c> (a parameter, compared with the literal by how the database itself renders them) and <c>roundtrip</c>
/// (a parameter read back through the provider, with the CLR type, <c>GetFieldValue</c> and a <c>WHERE</c> equality).
/// </summary>
public abstract partial class TypeFidelityTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, ITypeFidelityFixture
{
	private const string Table = "conformance_type";

	private static readonly MethodInfo s_getFieldValue = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetFieldValue));

	protected TypeFidelityTestBase(TFixture fixture)
		: base(fixture)
	{
		Fixture = fixture;
	}

	protected new TFixture Fixture { get; }

	[Flags]
	private enum Paths
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

	private void Run(Paths paths)
	{
		var tally = new Tally();
		using var connection = CreateOpenConnection();
		foreach (var spec in Fixture.Types)
		{
			if (!Attempt(tally, spec.ColumnSql, "(column)", "read", () =>
			{
				Execute(connection, Fixture.DropTableSql(Table));
				Execute(connection, Fixture.CreateTableSql(Table, spec.ColumnSql));
			}))
			{
				continue;
			}

			try
			{
				foreach (var value in spec.Values)
				{
					Evaluate(connection, spec, value, paths, tally);
				}
			}
			finally
			{
				try
				{
					Execute(connection, Fixture.DropTableSql(Table));
				}
				catch (DbException)
				{
				}
			}
		}

		if (tally.NotExact > 0)
		{
			ConformanceOutcome.Different($"{tally.NotExact} of {tally.Checks} type-fidelity checks were not exact; the findings say which.");
		}
	}

	private void Evaluate(DbConnection connection, TypeSpec spec, TypeValue value, Paths paths, Tally tally)
	{
		Execute(connection, $"delete from {Table}");

		var literalStored = Attempt(tally, spec.ColumnSql, value.Label, "read",
			() => Execute(connection, $"insert into {Table} (Id, V) values (1, {value.Literal})"));
		if (literalStored && paths.HasFlag(Paths.Read))
		{
			Attempt(tally, spec.ColumnSql, value.Label, "read", () =>
			{
				var read = Read(connection, spec, "select V from " + Table + " where Id = 1", null);
				Record(tally, spec, value, "read", FidelityComparer.Compare(value.Expected, read.Value));
			});
		}

		if (!paths.HasFlag(Paths.Write) && !paths.HasFlag(Paths.RoundTrip))
		{
			return;
		}

		var parameterStored = Attempt(tally, spec.ColumnSql, value.Label, paths.HasFlag(Paths.Write) ? "write" : "roundtrip", () =>
			Execute(connection, $"insert into {Table} (Id, V) values (2, {Fixture.ParameterMarker("p")})", command => Bind(command, spec, value)));
		if (!parameterStored)
		{
			return;
		}

		if (paths.HasFlag(Paths.Write) && literalStored)
		{
			Attempt(tally, spec.ColumnSql, value.Label, "write", () =>
			{
				var render = string.Format(CultureInfo.InvariantCulture, spec.RenderAsText, "V");
				var fromLiteral = Convert.ToString(ReadScalar(connection, $"select {render} from {Table} where Id = 1"), CultureInfo.InvariantCulture);
				var fromParameter = Convert.ToString(ReadScalar(connection, $"select {render} from {Table} where Id = 2"), CultureInfo.InvariantCulture);
				Record(tally, spec, value, "write", string.Equals(fromLiteral, fromParameter, StringComparison.Ordinal)
					? new FidelityResult(FidelityStatus.Exact, null)
					: new FidelityResult(FidelityStatus.Lossy, $"the database renders the literal as {FidelityComparer.Show(fromLiteral)} but the parameter as {FidelityComparer.Show(fromParameter)}"));
			});
		}

		if (!paths.HasFlag(Paths.RoundTrip))
		{
			return;
		}

		Attempt(tally, spec.ColumnSql, value.Label, "roundtrip", () =>
		{
			var read = Read(connection, spec, "select V from " + Table + " where Id = 2", null);
			Record(tally, spec, value, "roundtrip", FidelityComparer.Compare(value.Expected, read.Value));

			if (value.Expected is not null)
			{
				Record(tally, spec, value, "type", read.FieldType == spec.ClrType && read.Value.GetType() == spec.ClrType
					? new FidelityResult(FidelityStatus.Exact, null)
					: new FidelityResult(FidelityStatus.WrongType, $"expected {spec.ClrType.Name} but GetFieldType is {read.FieldType?.Name} and GetValue is {read.Value?.GetType().Name}"));
				Record(tally, spec, value, "typed", read.TypedFailure is not null
					? new FidelityResult(FidelityStatus.Throws, read.TypedFailure)
					: FidelityComparer.Compare(value.Expected, read.Typed));
			}
		});

		if (!spec.Comparable || value.Expected is null)
		{
			Record(tally, spec, value, "where", new FidelityResult(FidelityStatus.NotApplicable, spec.Comparable ? "NULL never equals NULL" : "the database cannot compare this type with ="));
			return;
		}

		Attempt(tally, spec.ColumnSql, value.Label, "where", () =>
		{
			var expectedRows = literalStored ? 2 : 1;
			var found = Convert.ToInt32(ReadScalar(connection, $"select count(*) from {Table} where V = {Fixture.ParameterMarker("p")}", command => Bind(command, spec, value)), CultureInfo.InvariantCulture);
			Record(tally, spec, value, "where", found == expectedRows
				? new FidelityResult(FidelityStatus.Exact, null)
				: new FidelityResult(FidelityStatus.Lossy, $"WHERE V = parameter matched {found} of {expectedRows} rows"));
		});
	}

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
			ConformanceOutcome.Finding(type, value, path, FidelityStatus.Throws, exception.GetType().Name + ": " + FirstLine(exception.Message));
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

		ConformanceOutcome.Finding(spec.ColumnSql, value.Label, path, result.Status, result.Detail);
	}

	private static string FirstLine(string message)
	{
		var end = message.IndexOfAny(new[] { '\r', '\n' });
		var line = end < 0 ? message : message.Substring(0, end);
		return line.Length > 200 ? line.Substring(0, 200) : line;
	}

	private static void Bind(DbCommand command, TypeSpec spec, TypeValue value)
	{
		var parameter = command.CreateParameter();
		parameter.ParameterName = "p";
		parameter.DbType = spec.DbType;
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

	private static void Execute(DbConnection connection, string sql, Action<DbCommand> bind = null)
	{
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		bind?.Invoke(command);
		command.ExecuteNonQuery();
	}

	private static object ReadScalar(DbConnection connection, string sql, Action<DbCommand> bind = null)
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
				result.Typed = s_getFieldValue.MakeGenericMethod(spec.ClrType).Invoke(reader, new object[] { 0 });
			}
			catch (TargetInvocationException exception)
			{
				result.TypedFailure = exception.InnerException?.GetType().Name + ": " + FirstLine(exception.InnerException?.Message ?? string.Empty);
			}
		}

		return result;
	}
}
