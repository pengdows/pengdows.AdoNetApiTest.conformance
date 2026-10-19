using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace pengdows.AdoNetApiTest.conformance.Databases;

/// <summary>
/// A provider described by a file in <c>providers/</c> instead of a test project: the driver package, where the database
/// comes from (a container, a cloud service or a file), how to connect, the SQL dialect where it differs from SQL-92, and
/// which types to test. The runner generates a test project from it.
/// </summary>
public sealed class ProviderConfig
{
	private static readonly JsonSerializerOptions s_options = new()
	{
		PropertyNameCaseInsensitive = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		AllowTrailingCommas = true,
	};

	/// <summary>The provider's name in the matrix, such as <c>Npgsql9Postgres17</c>.</summary>
	public string Name { get; set; }

	/// <summary>What the configuration is for, shown nowhere but useful in the file.</summary>
	public string Description { get; set; }

	public DriverConfig Driver { get; set; }

	public TargetConfig Target { get; set; }

	/// <summary>
	/// The connection string, with <c>${host}</c> and <c>${port}</c> (the container's), <c>${tempdir}</c> (a directory
	/// removed after the run) and <c>${NAME}</c> for any environment variable.
	/// </summary>
	public string ConnectionString { get; set; }

	/// <summary>Environment variables set for the test process before the first test.</summary>
	public Dictionary<string, string> Environment { get; set; } = new();

	/// <summary>
	/// Environment variables the runner sets when it starts the test process, for a driver that reads them before any
	/// test code runs (Informix's native client needs LD_LIBRARY_PATH and INFORMIXDIR at process start). <c>${output}</c>
	/// is the test assembly's folder. Read by the runner, not here.
	/// </summary>
	public Dictionary<string, string> ProcessEnvironment { get; set; } = new();

	/// <summary>
	/// Files written on the test machine once the target is up, by path, with their text; both may use the connection
	/// string's placeholders and <c>${output}</c> (the test assembly's folder). Informix's client looks the server up in
	/// an INFORMIXSQLHOSTS file.
	/// </summary>
	public Dictionary<string, string> HostFiles { get; set; } = new();

	/// <summary>What the provider needs to run at all; when it is missing, the runner reports it as not run. Read by the runner.</summary>
	public RequiresConfig Requires { get; set; } = new();

	public DialectConfig Dialect { get; set; } = new();

	public TypesConfig Types { get; set; } = new();

	public static ProviderConfig Load(string path)
	{
		var config = JsonSerializer.Deserialize<ProviderConfig>(File.ReadAllText(path), s_options)
			?? throw new InvalidOperationException($"{path} is empty.");
		if (string.IsNullOrEmpty(config.Name) || config.Driver?.Package is null || config.Driver.Factory is null || config.ConnectionString is null)
		{
			throw new InvalidOperationException($"{path} needs name, driver.package, driver.factory and connectionString.");
		}

		config.Target ??= new TargetConfig { Kind = "cloud" };
		return config;
	}

	/// <summary>Replaces <c>${...}</c> in <paramref name="text"/>; an unknown name is an error that says which.</summary>
	public static string Expand(string text, IReadOnlyDictionary<string, string> values) =>
		Regex.Replace(text ?? string.Empty, @"\$\{([A-Za-z_][A-Za-z0-9_]*)\}", match =>
		{
			var name = match.Groups[1].Value;
			if (values.TryGetValue(name, out var value))
			{
				return value;
			}

			return System.Environment.GetEnvironmentVariable(name)
				?? throw new InvalidOperationException($"The connection string needs the environment variable {name}.");
		});
}

public sealed class DriverConfig
{
	/// <summary>The NuGet package of the driver.</summary>
	public string Package { get; set; }

	/// <summary>The package version: exact (<c>9.0.3</c>) or floating (<c>9.*</c>).</summary>
	public string Version { get; set; } = "*";

	/// <summary>The <c>DbProviderFactory</c> type, assembly-qualified; its static <c>Instance</c> is used, or a new one.</summary>
	public string Factory { get; set; }

	/// <summary>Other packages the driver needs, by name and version (a native SQLite bundle).</summary>
	public Dictionary<string, string> Packages { get; set; } = new();
}

public sealed class TargetConfig
{
	/// <summary><c>docker</c>, <c>cloud</c> (the connection string names environment variables) or <c>file</c>.</summary>
	public string Kind { get; set; } = "docker";

	public string Image { get; set; }

	/// <summary>The port the server listens on in the container; Docker chooses the host port.</summary>
	public int Port { get; set; }

	public Dictionary<string, string> Env { get; set; } = new();

	/// <summary>The container's command, when the image needs arguments.</summary>
	public List<string> Command { get; set; } = new();

	/// <summary>A directory in the container to keep in memory (the data directory), when the image allows it.</summary>
	public string Tmpfs { get; set; }

	/// <summary>A line the server logs once it is up; the fixture also waits until a connection opens.</summary>
	public string WaitForLog { get; set; }

	/// <summary>False for an image whose server is not up until after <see cref="Exec"/> (Sybase ASE); only a connection that opens is waited for.</summary>
	public bool WaitForPort { get; set; } = true;

	/// <summary>Restarts the container after <see cref="Exec"/>, for a change the server reads at start (Sybase's -T11889 trace flag).</summary>
	public bool RestartAfterExec { get; set; }

	/// <summary>How long to wait for the first connection to open.</summary>
	public int ReadySeconds { get; set; } = 180;

	/// <summary>
	/// Statements run once the server accepts connections, before any test (creating a database). A statement that
	/// fails is retried until <see cref="ReadySeconds"/> runs out: some servers refuse CREATE DATABASE while they finish
	/// starting (Sybase ASE while it copies its model database).
	/// </summary>
	public List<string> Setup { get; set; } = new();

	/// <summary>Shell commands run in the container once it has started (Informix creates its database with dbaccess); each must exit 0, and is retried like <see cref="Setup"/>.</summary>
	public List<string> Exec { get; set; } = new();

	/// <summary>Files to put in the container before it starts, by path, with their text (SAP HANA's password file).</summary>
	public Dictionary<string, string> Files { get; set; } = new();

	/// <summary>Runs the container privileged (IBM Db2 needs it).</summary>
	public bool Privileged { get; set; }

	/// <summary>
	/// Containers the target needs beside it, started first on a network of their own (Spanner Omni behind PGAdapter).
	/// The target reaches each by its <see cref="SidecarConfig.Alias"/>.
	/// </summary>
	public List<SidecarConfig> Sidecars { get; set; } = new();

	/// <summary>The connection string the setup statements use, when it differs (no database yet); same placeholders.</summary>
	public string SetupConnectionString { get; set; }
}

public sealed class SidecarConfig
{
	public string Image { get; set; }

	/// <summary>The host name the target uses for it on the shared network.</summary>
	public string Alias { get; set; }

	public Dictionary<string, string> Env { get; set; } = new();

	public List<string> Command { get; set; } = new();

	public string Tmpfs { get; set; }

	public string WaitForLog { get; set; }
}

public sealed class RequiresConfig
{
	/// <summary><c>windows</c>, <c>linux</c> or <c>osx</c>, when the driver runs only there.</summary>
	public string Os { get; set; }

	/// <summary>Environment variables that must be set, such as an opt-in switch or a cloud account's credentials.</summary>
	public List<string> Env { get; set; } = new();
}

public sealed class DialectConfig
{
	/// <summary>A one-row SELECT of <c>{0}</c> without a terminator, such as <c>SELECT {0} FROM dual</c>.</summary>
	public string Select { get; set; } = "SELECT {0}";

	public string Terminator { get; set; } = ";";

	/// <summary>The parameter as written in SQL with <c>{0}</c> for its name, when DataSourceInformation does not say (<c>@{0}</c>, <c>?</c>).</summary>
	public string ParameterMarker { get; set; }

	/// <summary>SQL that renders a stored value as text, with <c>{0}</c> for the column.</summary>
	public string RenderAsText { get; set; } = "CAST({0} AS VARCHAR(4000))";

	/// <summary>The type-test table, with <c>{0}</c> for its name and <c>{1}</c> for the column type.</summary>
	public string CreateTable { get; set; } = "create table {0} (Id integer not null primary key, V {1})";

	public string DropTable { get; set; } = "drop table {0}";
}

public sealed class TypesConfig
{
	/// <summary>A query whose first column names each type the server supports; types DataTypes leaves out are findings.</summary>
	public string Query { get; set; }

	/// <summary>Types to test besides those DataTypes lists.</summary>
	public List<IncludedType> Include { get; set; } = new();

	/// <summary>Types not to test, by name, each with the reason.</summary>
	public Dictionary<string, string> Exclude { get; set; } = new();
}

public sealed class IncludedType
{
	public string Name { get; set; }

	/// <summary>The CLR type a faithful provider returns, such as <c>System.Boolean</c>.</summary>
	public string ClrType { get; set; }

	/// <summary>The column definition, when it is not the name.</summary>
	public string Column { get; set; }
}
