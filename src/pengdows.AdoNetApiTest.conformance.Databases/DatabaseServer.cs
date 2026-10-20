using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Testcontainers.FirebirdSql;
using Testcontainers.MsSql;
using Testcontainers.MySql;
using Testcontainers.Oracle;
using Testcontainers.PostgreSql;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance.Databases;

/// <summary>
/// A database server in a container of its own for one test run. A provider test project registers it as an assembly
/// fixture (<c>[assembly: AssemblyFixture(typeof(PostgreSqlServer))]</c>), so it starts before the first test and is
/// removed, with its data, when the run ends; if the process dies first, Testcontainers' reaper removes it. The host
/// port is chosen by Docker, so any number of runs, of the same server or not, can share a machine.
/// Setting the <c>ConnectionString</c> environment variable uses that server instead, and no container is started.
/// </summary>
public abstract class DatabaseServer<TServer> : IAsyncLifetime
	where TServer : DatabaseServer<TServer>
{
	private static string s_host;
	private static int s_port;
	private static string s_connectionString;
	private IContainer m_container;

	/// <summary>The host to connect to.</summary>
	public static string Host => s_host ?? throw NotStarted();

	/// <summary>The host port Docker mapped to the server's port.</summary>
	public static int Port => s_host is null ? throw NotStarted() : s_port;

	/// <summary>The connection string the Testcontainers module builds, for a server whose module has one.</summary>
	public static string ContainerConnectionString => s_connectionString ?? throw NotStarted();

	/// <summary>The port the server listens on inside the container.</summary>
	protected abstract ushort ContainerPort { get; }

	protected abstract IContainer Build();

	/// <summary>Runs once the server accepts connections, before any test.</summary>
	protected virtual Task OnStartedAsync(IContainer container) => Task.CompletedTask;

	public async ValueTask InitializeAsync()
	{
		if (Environment.GetEnvironmentVariable("ConnectionString") is not null)
		{
			return;
		}

		m_container = Build();
		await m_container.StartAsync().ConfigureAwait(false);
		await OnStartedAsync(m_container).ConfigureAwait(false);
		s_port = m_container.GetMappedPublicPort(ContainerPort);
		s_connectionString = (m_container as IDatabaseContainer)?.GetConnectionString();
		s_host = m_container.Hostname;
		Provenance.Image = m_container.Image.FullName;
	}

	public async ValueTask DisposeAsync()
	{
		s_host = null;
		s_connectionString = null;
		if (m_container is not null)
		{
			await m_container.DisposeAsync().ConfigureAwait(false);
		}
	}

	private static InvalidOperationException NotStarted() =>
		new($"{typeof(TServer).Name} is not running. Register it with [assembly: AssemblyFixture(typeof({typeof(TServer).Name}))], or set the ConnectionString environment variable.");
}

public sealed class PostgreSqlServer : DatabaseServer<PostgreSqlServer>
{
	public const string User = "root";
	public const string Password = "test";
	public const string Database = "conformance";

	protected override ushort ContainerPort => PostgreSqlBuilder.PostgreSqlPort;

	protected override IContainer Build() =>
		new PostgreSqlBuilder("postgres:17-alpine")
			.WithUsername(User)
			.WithPassword(Password)
			.WithDatabase(Database)
			.WithTmpfsMount("/var/lib/postgresql/data")
			.Build();
}

public sealed class MySqlServer : DatabaseServer<MySqlServer>
{
	public const string User = "root";
	public const string Password = "test";
	public const string Database = "conformance";

	protected override ushort ContainerPort => MySqlBuilder.MySqlPort;

	// utf8mb4_bin: the tests compare strings byte for byte, so the database must not fold case or accents.
	protected override IContainer Build() =>
		new MySqlBuilder("mysql:8.4")
			.WithUsername(User)
			.WithPassword(Password)
			.WithDatabase(Database)
			.WithCommand("--character-set-server=utf8mb4", "--collation-server=utf8mb4_bin")
			.WithTmpfsMount("/var/lib/mysql")
			.Build();
}

public sealed class SqlServerServer : DatabaseServer<SqlServerServer>
{
	public const string User = "sa";
	public const string Password = "P@ssw0rd";
	public const string Database = "conformance";

	protected override ushort ContainerPort => MsSqlBuilder.MsSqlPort;

	protected override IContainer Build() =>
		new MsSqlBuilder("mcr.microsoft.com/mssql/server:2017-CU17-ubuntu")
			.WithPassword(Password)
			.Build();

	protected override async Task OnStartedAsync(IContainer container)
	{
		var result = await ((MsSqlContainer) container).ExecScriptAsync($"CREATE DATABASE {Database};").ConfigureAwait(false);
		if (result.ExitCode != 0)
		{
			throw new InvalidOperationException($"Could not create the {Database} database: {result.Stderr}{result.Stdout}");
		}
	}
}

public sealed class OracleServer : DatabaseServer<OracleServer>
{
	protected override ushort ContainerPort => OracleBuilder.OraclePort;

	// The faststart image ships a ready database inside the image, so its data directory cannot be a tmpfs.
	protected override IContainer Build() =>
		new OracleBuilder("gvenzl/oracle-free:23.26.2-slim-faststart")
			.Build();
}

public sealed class FirebirdServer : DatabaseServer<FirebirdServer>
{
	protected override ushort ContainerPort => FirebirdSqlBuilder.FirebirdSqlPort;

	protected override IContainer Build() =>
		new FirebirdSqlBuilder("jacobalberty/firebird:v4.0")
			.Build();
}

public sealed class TiDbServer : DatabaseServer<TiDbServer>
{
	public const ushort TiDbPort = 4000;
	public const string User = "root";
	public const string Database = "test";

	protected override ushort ContainerPort => TiDbPort;

	// TiDB accepts its port before its SQL layer is ready; this line says the SQL layer is up. Its store is in memory
	// (unistore) by default, so nothing is written to disk.
	protected override IContainer Build() =>
		new ContainerBuilder("pingcap/tidb:v8.5.7")
			.WithPortBinding(TiDbPort, true)
			.WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("server is running MySQL protocol"))
			.Build();
}
