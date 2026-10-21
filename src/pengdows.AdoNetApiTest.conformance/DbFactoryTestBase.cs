using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

public abstract class DbFactoryTestBase<TFixture> : IAsyncLifetime, IDisposable, IClassFixture<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	private static readonly ConditionalWeakTable<TFixture, ParameterMarker> s_parameterMarkers = new();

	protected DbFactoryTestBase(TFixture fixture)
	{
		Fixture = fixture;
		m_cancellationTokenSource = new CancellationTokenSource();
		m_cancellationTokenSource.Cancel();
		CanceledToken = m_cancellationTokenSource.Token;
	}

	public async ValueTask InitializeAsync()
	{
		await OnInitializeAsync().ConfigureAwait(false);
	}

	public async ValueTask DisposeAsync()
	{
		await OnDisposeAsync().ConfigureAwait(false);
	}

	public void Dispose()
	{
		m_cancellationTokenSource.Dispose();
	}

	protected TFixture Fixture { get; }

	protected CancellationToken CanceledToken { get; }

	protected virtual Task OnInitializeAsync() => Task.CompletedTask;

	protected virtual Task OnDisposeAsync() => Task.CompletedTask;

	protected virtual DbConnection CreateConnection() => Fixture.Factory.CreateConnection();

	protected virtual DbConnection CreateOpenConnection()
	{
		var connection = CreateConnection();
		connection.ConnectionString = ConnectionString;
		connection.Open();
		Provenance.Record(Fixture.Factory, connection);
		return connection;
	}

	protected virtual DbConnectionStringBuilder CreateConnectionStringBuilder()
		=> Fixture.Factory.CreateConnectionStringBuilder();

	/// <summary>
	/// A provider has a DataAdapter when its factory creates one, and must then support every member
	/// <c>DbDataAdapter.Update</c> reads (UpdatedRowSource, SourceColumn, SourceVersion, ...). Without one, those members
	/// do nothing, so this reports the test not implemented and returns false.
	/// </summary>
	/// <seealso href="https://learn.microsoft.com/dotnet/api/system.data.common.dbproviderfactory.createdataadapter" />
	protected bool ImplementsDataAdapter()
	{
		DbDataAdapter adapter;
		try
		{
			adapter = Fixture.Factory.CreateDataAdapter();
		}
		catch (NotSupportedException)
		{
			adapter = null;
		}

		using (adapter)
		{
			if (adapter is not null)
				return true;
		}

		ConformanceOutcome.NotImplemented("The provider has no DataAdapter: DbProviderFactory.CreateDataAdapter returns null.");
		return false;
	}

	/// <summary>
	/// A provider supports design-time components when a new command is DesignTimeVisible ("The default is true."), and
	/// must then support all of design time: the flag and the component Disposed event. A provider whose commands are
	/// hidden from designers has opted out, so this reports the test not implemented and returns false.
	/// </summary>
	/// <seealso href="https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.designtimevisible" />
	protected bool ImplementsDesignTime()
	{
		using (var command = Fixture.Factory.CreateCommand())
		{
			if (command.DesignTimeVisible)
				return true;
		}

		ConformanceOutcome.NotImplemented("The provider does not support design time: a new command is not DesignTimeVisible.");
		return false;
	}

	/// <summary>
	/// Returns a provider-valid query that produces one scalar value. Providers
	/// whose dialect requires a dummy table may override this hook in their
	/// provider-specific test project; shared tests must not assume that a
	/// bare <c>SELECT 1</c> is portable.
	/// </summary>
	/// <seealso href="https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executereader" />
	protected virtual string SelectSql(string projection) => Fixture is ISqlDialectFixture dialect
		? dialect.SelectStatement(projection) + dialect.StatementTerminator
		: $"SELECT {projection};";

	/// <summary>A text literal in the fixture's spelling (<see cref="ISqlLiteralFixture"/>), SQL-92 <c>'text'</c> by default.</summary>
	protected string Text(string text) => SqlLiterals.Text(Fixture, text);

	/// <summary>A quoted identifier in the fixture's spelling (<see cref="ISqlLiteralFixture"/>), SQL-92 <c>"name"</c> by default.</summary>
	protected string Quoted(string name) => SqlLiterals.Quoted(Fixture, name);

	/// <summary>Returns one scalar using the provider's portable SELECT shape.</summary>
	protected virtual string SelectOneSql => SelectSql("1");

	/// <summary>Returns one scalar with a stable column name.</summary>
	protected virtual string SelectOneAsIdSql => SelectSql("1 AS id");

	/// <summary>Returns one scalar with the command-test column name.</summary>
	protected virtual string SelectOneAsValueSql => SelectSql("1 AS value");

	/// <summary>Returns one text scalar.</summary>
	protected virtual string SelectTextSql => SelectSql(Text("test"));

	/// <summary>Returns one text scalar with a stable column name.</summary>
	protected virtual string SelectTextAsIdSql => SelectSql(Text("test") + " AS Id");

	/// <summary>Returns two result sets containing one scalar each.</summary>
	protected virtual string SelectTwoResultsSql => SelectSql("1") + " " + SelectSql("2");

	/// <summary>Returns three result sets containing one scalar each.</summary>
	protected virtual string SelectThreeResultsSql => SelectSql("1") + " " + SelectSql("2") + " " + SelectSql("3");

	/// <summary>Returns the rows 1 and 2, in that order, from a single result set (a distributed database such as TiDB returns a UNION in either order without ORDER BY).</summary>
	protected virtual string SelectTwoRowsSql => Fixture is ISqlDialectFixture dialect
		? $"{dialect.SelectStatement("1")} UNION {dialect.SelectStatement("2")} ORDER BY 1{dialect.StatementTerminator}"
		: "SELECT 1 UNION SELECT 2 ORDER BY 1;";

	/// <summary>Returns two known rows with the command-test value aliases.</summary>
	protected virtual string SelectTwoRows42Sql => Fixture is ISqlDialectFixture dialect
		? $"{dialect.SelectStatement("42 AS value")} UNION {dialect.SelectStatement("43 AS value")} ORDER BY value{dialect.StatementTerminator}"
		: "SELECT 42 AS value UNION SELECT 43 AS value ORDER BY value;";

	/// <summary>Returns an empty result followed by one result row.</summary>
	protected virtual string SelectNoRowsThenOneSql => Fixture is ISelectValueFixture select
		? select.SelectNoRows + SelectOneSql
		: SelectOneSql;

	/// <summary>
	/// Matches provider-neutral ADO.NET behavior: providers that
	/// explicitly advertise the positional format '?' use that placeholder; named
	/// providers receive their advertised marker plus the logical name. When
	/// DataSourceInformation is missing or unusable, the suite asks the database instead of
	/// guessing: a probe query binds a parameter as <c>:name</c> (SQL-92's named parameter)
	/// and then as <c>@name</c>, and the first that returns the bound value is the marker.
	/// The DataSourceInformation tests still report the missing metadata.
	/// The logical <see cref="DbParameter.ParameterName"/> remains independent of
	/// this SQL representation.
	/// The positional-versus-named decision follows the ADO.NET
	/// <see cref="DbMetaDataColumnNames.ParameterMarkerFormat"/> contract; the
	/// marker prefix may also come from <see cref="DbMetaDataColumnNames.ParameterMarkerPattern"/>.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbmetadatacolumnnames.parametermarkerformat.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbmetadatacolumnnames.parametermarkerpattern.
	/// </summary>
	protected virtual string MakeParameterName(DbConnection connection, string name)
	{
		if (Fixture is IParameterMarkerFixture explicitMarker)
		{
			Provenance.RecordParameterMarker(explicitMarker.ParameterMarker("p"), "fixture");
			return explicitMarker.ParameterMarker(name);
		}

		var marker = s_parameterMarkers.GetValue(Fixture, _ => FindParameterMarker(connection));
		return marker.IsPositional ? "?" : string.Concat(marker.Prefix, name.TrimStart('@', ':', '$', '?'));
	}

	private ParameterMarker FindParameterMarker(DbConnection connection)
	{
		var source = "metadata";
		var marker = ReadParameterMarker(connection);
		if (marker is null)
		{
			source = "probe";
			marker = ProbeParameterMarker();
		}

		Provenance.RecordParameterMarker(marker.IsPositional ? "?" : marker.Prefix + "p", source);
		return marker;
	}

	private static ParameterMarker ReadParameterMarker(DbConnection connection)
	{
		DataTable schema;
		try
		{
			schema = connection.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);
		}
		catch (Exception ex) when (ex is NotSupportedException or NotImplementedException or ArgumentException)
		{
			return null;
		}

		if (schema.Rows.Count == 0)
		{
			return null;
		}

		var row = schema.Rows[0];
		var format = schema.Columns.Contains(DbMetaDataColumnNames.ParameterMarkerFormat) ? row[DbMetaDataColumnNames.ParameterMarkerFormat] as string : null;
		var pattern = schema.Columns.Contains(DbMetaDataColumnNames.ParameterMarkerPattern) ? row[DbMetaDataColumnNames.ParameterMarkerPattern] as string : null;
		return ParameterMarkerDiscovery.Discover(format, pattern);
	}

	/// <summary>
	/// For a provider whose metadata does not say: SQL-92 writes a named parameter <c>:name</c>, and .NET providers
	/// commonly write <c>@name</c>. Each is tried on a connection of its own, so a failed attempt can't disturb a test's
	/// connection or transaction. When neither returns the bound value, <c>@</c> stays, and the parameter tests show it.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	private ParameterMarker ProbeParameterMarker()
	{
		foreach (var prefix in s_probedPrefixes)
		{
			try
			{
				using var connection = CreateOpenConnection();
				using var command = connection.CreateCommand();
				command.CommandText = SelectSql(prefix + "probe");
				var parameter = command.CreateParameter();
				parameter.ParameterName = "probe";
				parameter.Value = 7;
				command.Parameters.Add(parameter);
				if (Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) == 7)
				{
					return new ParameterMarker(false, prefix);
				}
			}
			catch (Exception)
			{
				// this database does not take the prefix; try the next
			}
		}

		return new ParameterMarker(false, "@");
	}

	private static readonly string[] s_probedPrefixes = [":", "@"];

	/// <summary>
	/// Formats a provider parameter from its logical name using the same named-versus-
	/// positional decision as <see cref="MakeParameterName(DbConnection, string)"/>.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbmetadatacolumnnames.parametermarkerformat.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbmetadatacolumnnames.parametermarkerpattern.
	/// </summary>
	protected virtual string MakeParameterName(DbConnection connection, DbParameter parameter)
		=> MakeParameterName(connection, parameter.ParameterName);

	protected virtual string ConnectionString
	{
		get
		{
			if (m_connectionString == null)
				m_connectionString = Environment.GetEnvironmentVariable("ConnectionString") ?? Fixture.ConnectionString;
			return m_connectionString;
		}
	}

	protected static Exception AssertThrowsAny<TException1, TException2>(Action action)
		where TException1 : Exception
		where TException2 : Exception
	{
		try
		{
			action();
			throw ThrowsException.ForNoException(typeof(TException1));
		}
		catch (TException1 ex)
		{
			return ex;
		}
		catch (TException2 ex)
		{
			return ex;
		}
		catch (Exception ex)
		{
			throw ThrowsException.ForIncorrectExceptionType(typeof(TException1), ex);
		}
	}

	protected static Exception AssertThrowsAny<TException1, TException2, TException3>(Action action)
		where TException1 : Exception
		where TException2 : Exception
		where TException3 : Exception
	{
		try
		{
			action();
			throw ThrowsException.ForNoException(typeof(TException1));
		}
		catch (TException1 ex)
		{
			return ex;
		}
		catch (TException2 ex)
		{
			return ex;
		}
		catch (TException3 ex)
		{
			return ex;
		}
		catch (Exception ex)
		{
			throw ThrowsException.ForIncorrectExceptionType(typeof(TException1), ex);
		}
	}

	readonly CancellationTokenSource m_cancellationTokenSource;
	string m_connectionString;
}
