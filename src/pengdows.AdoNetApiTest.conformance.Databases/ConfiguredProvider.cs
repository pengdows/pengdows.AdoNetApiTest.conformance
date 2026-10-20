using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance.Databases;

/// <summary>
/// The provider a generated test project runs: its <see cref="ProviderConfig"/> (<c>provider.json</c> beside the test
/// assembly), the driver's factory, and the connection string once the target is up.
/// </summary>
public static class ConfiguredProvider
{
	private static readonly Lazy<ProviderConfig> s_config = new(() => ProviderConfig.Load(
		System.Environment.GetEnvironmentVariable("CONFORMANCE_PROVIDER_CONFIG") ?? Path.Combine(AppContext.BaseDirectory, "provider.json")));

	private static readonly Lazy<DbProviderFactory> s_factory = new(LoadFactory);

	internal static readonly Dictionary<string, string> Values = new(StringComparer.Ordinal);

	public static ProviderConfig Config => s_config.Value;

	public static DbProviderFactory Factory => s_factory.Value;

	public static string ConnectionString => ProviderConfig.Expand(Config.ConnectionString, Values);

	private static DbProviderFactory LoadFactory()
	{
		var type = Type.GetType(Config.Driver.Factory, throwOnError: false)
			?? throw new InvalidOperationException($"The factory type {Config.Driver.Factory} could not be loaded from package {Config.Driver.Package}.");
		const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
		var instance = type.GetField("Instance", flags)?.GetValue(null) ?? type.GetProperty("Instance", flags)?.GetValue(null) ?? Activator.CreateInstance(type);
		return instance as DbProviderFactory
			?? throw new InvalidOperationException($"{Config.Driver.Factory} is not a DbProviderFactory.");
	}
}

/// <summary>
/// Starts the configured target once per test run, as an assembly fixture: a container on a Docker-assigned port
/// (removed afterwards, by Testcontainers' reaper if the process dies), a temporary directory for a file database, or
/// nothing for a cloud service. It waits until the driver can open a connection, then runs the setup statements.
/// </summary>
public sealed class ConfiguredServer : IAsyncLifetime
{
	private IContainer m_container;
	private readonly List<IContainer> m_sidecars = new();
	private INetwork m_network;
	private string m_tempDirectory;

	public async ValueTask InitializeAsync()
	{
		var config = ConfiguredProvider.Config;
		foreach (var pair in config.Environment)
		{
			System.Environment.SetEnvironmentVariable(pair.Key, pair.Value);
		}

		m_tempDirectory = Path.Combine(Path.GetTempPath(), "conformance-" + config.Name + "-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(m_tempDirectory);
		ConfiguredProvider.Values["tempdir"] = m_tempDirectory;
		// The server-level checks (waiting for a connection) have no fixture; each fixture gets its own folder (ConfiguredFixture).
		ConfiguredProvider.Values["instancedir"] = m_tempDirectory;
		ConfiguredProvider.Values["output"] = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

		var target = config.Target;
		if (string.Equals(target.Kind, "docker", StringComparison.OrdinalIgnoreCase) && System.Environment.GetEnvironmentVariable("ConnectionString") is null)
		{
			if (target.Sidecars.Count > 0)
			{
				m_network = new NetworkBuilder().Build();
				await m_network.CreateAsync().ConfigureAwait(false);
				foreach (var sidecar in target.Sidecars)
				{
					var side = new ContainerBuilder(sidecar.Image).WithNetwork(m_network).WithNetworkAliases(sidecar.Alias);
					foreach (var pair in sidecar.Env)
					{
						side = side.WithEnvironment(pair.Key, pair.Value);
					}

					if (sidecar.Command.Count > 0)
					{
						side = side.WithCommand(sidecar.Command.ToArray());
					}

					if (!string.IsNullOrEmpty(sidecar.Tmpfs))
					{
						side = side.WithTmpfsMount(sidecar.Tmpfs);
					}

					if (!string.IsNullOrEmpty(sidecar.WaitForLog))
					{
						side = side.WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(sidecar.WaitForLog));
					}

					var started = side.Build();
					m_sidecars.Add(started);
					await started.StartAsync().ConfigureAwait(false);
				}
			}

			var builder = new ContainerBuilder(target.Image ?? throw new InvalidOperationException($"{config.Name}: target.image is required for a docker target."))
				.WithPortBinding(target.Port, assignRandomHostPort: true)
				.WithPrivileged(target.Privileged);
			if (m_network is not null)
			{
				builder = builder.WithNetwork(m_network);
			}

			foreach (var file in target.Files)
			{
				builder = builder.WithResourceMapping(System.Text.Encoding.UTF8.GetBytes(file.Value), file.Key);
			}
			foreach (var pair in target.Env)
			{
				builder = builder.WithEnvironment(pair.Key, pair.Value);
			}

			if (target.Command.Count > 0)
			{
				builder = builder.WithCommand(target.Command.ToArray());
			}

			if (!string.IsNullOrEmpty(target.Tmpfs))
			{
				builder = builder.WithTmpfsMount(target.Tmpfs);
			}

			if (!string.IsNullOrEmpty(target.WaitForLog))
			{
				builder = builder.WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(target.WaitForLog));
			}
			else if (target.WaitForPort)
			{
				builder = builder.WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(target.Port));
			}

			m_container = builder.Build();
			await m_container.StartAsync().ConfigureAwait(false);

			foreach (var exec in target.Exec)
			{
				await RetryAsync(target.ReadySeconds, async () =>
				{
					var result = await m_container.ExecAsync(new[] { "sh", "-c", exec }).ConfigureAwait(false);
					if (result.ExitCode != 0)
					{
						throw new InvalidOperationException($"'{exec}' exited with {result.ExitCode}: {result.Stdout}{result.Stderr}");
					}
				}).ConfigureAwait(false);
			}

			if (target.RestartAfterExec)
			{
				await m_container.StopAsync().ConfigureAwait(false);
				await m_container.StartAsync().ConfigureAwait(false);
			}

			// Read after any restart: Docker may map the port anew.
			Provenance.Image = m_container.Image.FullName;

			ConfiguredProvider.Values["host"] = m_container.Hostname;
			ConfiguredProvider.Values["port"] = m_container.GetMappedPublicPort((ushort) target.Port).ToString(System.Globalization.CultureInfo.InvariantCulture);
		}

		// Written once the container is up, so their text can name its host and port (Informix's sqlhosts entry).
		foreach (var file in config.HostFiles)
		{
			var path = ProviderConfig.Expand(file.Key, ConfiguredProvider.Values);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, ProviderConfig.Expand(file.Value, ConfiguredProvider.Values));
		}

		if (target.Setup.Count > 0)
		{
			var setup = ProviderConfig.Expand(target.SetupConnectionString ?? config.ConnectionString, ConfiguredProvider.Values);
			await WaitForConnectionAsync(setup, target.ReadySeconds).ConfigureAwait(false);
			foreach (var statement in target.Setup)
			{
				await RetryAsync(target.ReadySeconds, () =>
				{
					using var connection = Open(setup);
					using var command = connection.CreateCommand();
					command.CommandText = statement;
					command.ExecuteNonQuery();
					return Task.CompletedTask;
				}).ConfigureAwait(false);
			}
		}

		await WaitForConnectionAsync(ConfiguredProvider.ConnectionString, target.ReadySeconds).ConfigureAwait(false);
	}

	public async ValueTask DisposeAsync()
	{
		if (m_container is not null)
		{
			await m_container.DisposeAsync().ConfigureAwait(false);
		}

		foreach (var sidecar in m_sidecars)
		{
			await sidecar.DisposeAsync().ConfigureAwait(false);
		}

		if (m_network is not null)
		{
			await m_network.DeleteAsync().ConfigureAwait(false);
		}

		try
		{
			if (m_tempDirectory is not null && Directory.Exists(m_tempDirectory))
			{
				Directory.Delete(m_tempDirectory, recursive: true);
			}
		}
		catch (IOException)
		{
		}
	}

	private static DbConnection Open(string connectionString)
	{
		var connection = ConfiguredProvider.Factory.CreateConnection();
		connection.ConnectionString = connectionString;
		connection.Open();
		return connection;
	}

	private static async Task RetryAsync(int seconds, Func<Task> step)
	{
		var deadline = DateTime.UtcNow.AddSeconds(seconds);
		while (true)
		{
			try
			{
				await step().ConfigureAwait(false);
				return;
			}
			catch (Exception) when (DateTime.UtcNow < deadline)
			{
				await Task.Delay(2000).ConfigureAwait(false);
			}
		}
	}

	/// <summary>Many servers accept their port before they accept a login; a connection that opens is the only sure sign.</summary>
	private static async Task WaitForConnectionAsync(string connectionString, int seconds)
	{
		var deadline = DateTime.UtcNow.AddSeconds(seconds);
		while (true)
		{
			try
			{
				using var connection = Open(connectionString);
				return;
			}
			catch (Exception) when (DateTime.UtcNow < deadline)
			{
				await Task.Delay(1000).ConfigureAwait(false);
			}
		}
	}
}

/// <summary>The fixture a generated test project's classes share, built from <see cref="ConfiguredProvider.Config"/>.</summary>
public class ConfiguredFixture : IDbFactoryFixture, ISqlDialectFixture, IDataTypesFidelityFixture, IServerTypesFixture
{
	public DbProviderFactory Factory => ConfiguredProvider.Factory;

	private readonly Lazy<string> m_instanceDirectory = new(() =>
	{
		var directory = Path.Combine(ConfiguredProvider.Values.TryGetValue("tempdir", out var temp) ? temp : Path.GetTempPath(), "instance-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(directory);
		return directory;
	});

	/// <summary>
	/// The configured connection string; <c>${instancedir}</c> in it is a folder of this fixture's own, for a file-based
	/// provider that allows one writer per folder (pengdows.flatfile), since each test class has its own fixture.
	/// </summary>
	public string ConnectionString => System.Environment.GetEnvironmentVariable("ConnectionString")
		?? ProviderConfig.Expand(ConfiguredProvider.Config.ConnectionString,
			new Dictionary<string, string>(ConfiguredProvider.Values) { ["instancedir"] = m_instanceDirectory.Value });

	private static DialectConfig Dialect => ConfiguredProvider.Config.Dialect;

	public string SelectStatement(string projection) => string.Format(System.Globalization.CultureInfo.InvariantCulture, Dialect.Select, projection);

	public string StatementTerminator => Dialect.Terminator ?? string.Empty;

	public string CreateTableSql(string table, string columnSql) => string.Format(System.Globalization.CultureInfo.InvariantCulture, Dialect.CreateTable, table, columnSql);

	public string DropTableSql(string table) => string.Format(System.Globalization.CultureInfo.InvariantCulture, Dialect.DropTable, table);

	public string RenderAsText => Dialect.RenderAsText;

	public string ServerTypesSql => ConfiguredProvider.Config.Types.Query;

	public IReadOnlyList<ExtraType> ExtraTypes => ConfiguredProvider.Config.Types.Include.Select(t => new ExtraType(t.Name, t.ClrType, t.Column)).ToList();

	public IReadOnlyDictionary<string, string> ExcludedTypes => ConfiguredProvider.Config.Types.Exclude;
}

/// <summary>A configured fixture whose provider does not advertise its parameter marker, so the configuration states it.</summary>
public class ConfiguredMarkerFixture : ConfiguredFixture, IParameterMarkerFixture
{
	public string ParameterMarker(string name) => string.Format(System.Globalization.CultureInfo.InvariantCulture, ConfiguredProvider.Config.Dialect.ParameterMarker, name);
}
