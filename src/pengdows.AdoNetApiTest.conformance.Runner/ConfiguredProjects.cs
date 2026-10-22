using System.Security;
using System.Text;
using System.Text.Json;

namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>
/// Turns each provider configuration (<c>providers/*.json</c> beside <c>tests/</c>) into a test project the runner runs
/// like any other: a <c>PackageReference</c> to the configured driver, the configuration copied beside the assembly, and
/// one thin class per test base over the configured fixture. The projects live in <c>tests/obj/providers</c>, which git
/// ignores, and are rewritten only when they change.
/// </summary>
internal static class ConfiguredProjects
{
	private static readonly JsonDocumentOptions s_json = new() { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };

	private static readonly string[] s_testBases =
	{
		"Command", "ConnectionString", "Connection", "DataReader", "DbProviderFactory", "Dml", "GetValueConversion", "Parameter", "Transaction",
		"DataTypesFidelity",
	};

	/// <summary>The test bases that use tables the fixture creates first: the select values and the DML table.</summary>
	private static readonly string[] s_valueTestBases = { "DataReader", "Dml", "GetValueConversion", "Transaction" };

	public static IReadOnlyList<ProviderProject> Discover(string testsDirectory)
		=> Discover(testsDirectory, Path.GetDirectoryName(Path.GetFullPath(testsDirectory).TrimEnd(Path.DirectorySeparatorChar))!, packageMode: false, packageVersion: null);

	public static IReadOnlyList<ProviderProject> Discover(string testsDirectory, string root, bool packageMode, string? packageVersion)
	{
		var providers = Path.Combine(root, "providers");
		if (!Directory.Exists(providers))
		{
			return Array.Empty<ProviderProject>();
		}

		return Directory.EnumerateFiles(providers, "*.json")
			.OrderBy(path => path, StringComparer.Ordinal)
			.Select(path => Generate(path, root, testsDirectory, packageMode, packageVersion))
			.ToList();
	}

	public static IReadOnlyList<ProviderProject> DiscoverEmbedded(string root, string testsDirectory, string packageVersion)
	{
		var providers = Path.Combine(root, "providers");
		Directory.CreateDirectory(providers);
		var assembly = typeof(ConfiguredProjects).Assembly;
		var paths = new List<string>();
		foreach (var resource in assembly.GetManifestResourceNames().Where(name => name.StartsWith("providers/", StringComparison.Ordinal)))
		{
			var name = resource["providers/".Length..];
			var path = Path.Combine(providers, name);
			using var input = assembly.GetManifestResourceStream(resource)!;
			using var output = File.Create(path);
			input.CopyTo(output);
			paths.Add(path);
		}

		return paths
			.OrderBy(path => path, StringComparer.Ordinal)
			.Select(path => Generate(path, root, testsDirectory, packageMode: true, packageVersion))
			.ToList();
	}

	public static ProviderProject Generate(string configPath, string root, string testsDirectory, bool packageMode = false, string? packageVersion = null)
	{
		var config = Read(configPath);
		if (packageMode && config.DriverProject is not null)
		{
			return new ProviderProject(config.Name, string.Empty, $"requires a source checkout ({config.DriverProject})", ConfigPath: configPath);
		}

		var directory = Path.Combine(testsDirectory, "obj", "providers", config.Name + ".Tests");
		Directory.CreateDirectory(directory);
		var project = Path.Combine(directory, config.Name + ".Tests.csproj");
		WriteIfChanged(project, Project(config, root, packageMode, packageVersion));
		WriteIfChanged(Path.Combine(directory, "Generated.cs"), Code(config));
		WriteIfChanged(Path.Combine(directory, "provider.json"), File.ReadAllText(configPath));
		return new ProviderProject(config.Name, project, NotRunReason(config), config.ProcessEnvironment, Path.GetFullPath(configPath));
	}

	/// <param name="DriverProject">A driver built from source instead of a package: its project file, relative to the repository root or absolute.</param>
	/// <param name="SingleWriter">The database allows one writer at a time: <c>target.singleWriter</c>, true by default for a file target.</param>
	internal sealed record Config(string Name, IReadOnlyList<(string Package, string Version)> Packages, bool StatesMarker,
		string? RequiredOs = null, IReadOnlyList<string>? RequiredEnv = null, IReadOnlyDictionary<string, string>? ProcessEnvironment = null,
		string? DriverProject = null, bool SingleWriter = false);

	/// <summary>Why the configuration cannot run on this machine, or <c>null</c>.</summary>
	internal static string? NotRunReason(Config config, Func<string, string?>? getEnv = null)
	{
		getEnv ??= Environment.GetEnvironmentVariable;
		if (config.RequiredOs is { } os && !OperatingSystem.IsOSPlatform(os))
		{
			return $"needs {os}";
		}

		var missing = (config.RequiredEnv ?? Array.Empty<string>()).Where(name => string.IsNullOrEmpty(getEnv(name))).ToList();
		return missing.Count == 0 ? null : $"needs the environment variable{(missing.Count == 1 ? string.Empty : "s")} {string.Join(", ", missing)}";
	}

	internal static Config Read(string path)
	{
		using var document = JsonDocument.Parse(File.ReadAllText(path), s_json);
		var json = document.RootElement;
		string? Text(JsonElement element, string name) =>
			element.ValueKind == JsonValueKind.Object && TryGet(element, name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

		var name = Text(json, "name");
		if (string.IsNullOrWhiteSpace(name) || !name.All(c => char.IsLetterOrDigit(c) || c == '_'))
		{
			throw new ArgumentException($"{path}: \"name\" must be letters, digits or underscores; it names the provider and its project.");
		}

		if (!TryGet(json, "driver", out var driver) || (Text(driver, "package") is null && Text(driver, "project") is null))
		{
			throw new ArgumentException($"{path}: \"driver.package\" (a NuGet package) or \"driver.project\" (a project to build) is required.");
		}

		var packages = new List<(string, string)>();
		if (Text(driver, "package") is { } package)
		{
			packages.Add((package, Text(driver, "version") ?? "*"));
		}

		if (TryGet(driver, "packages", out var more) && more.ValueKind == JsonValueKind.Object)
		{
			packages.AddRange(more.EnumerateObject().Select(p => (p.Name, p.Value.GetString() ?? "*")));
		}

		var marker = TryGet(json, "dialect", out var dialect) && Text(dialect, "parameterMarker") is not null;
		string? os = null;
		var env = new List<string>();
		if (TryGet(json, "requires", out var requires) && requires.ValueKind == JsonValueKind.Object)
		{
			os = Text(requires, "os");
			if (TryGet(requires, "env", out var names) && names.ValueKind == JsonValueKind.Array)
			{
				env.AddRange(names.EnumerateArray().Select(n => n.GetString()).OfType<string>());
			}
		}

		var process = new Dictionary<string, string>(StringComparer.Ordinal);
		if (TryGet(json, "processEnvironment", out var vars) && vars.ValueKind == JsonValueKind.Object)
		{
			foreach (var pair in vars.EnumerateObject())
			{
				process[pair.Name] = pair.Value.GetString() ?? string.Empty;
			}
		}

		var singleWriter = false;
		if (TryGet(json, "target", out var target) && target.ValueKind == JsonValueKind.Object)
		{
			singleWriter = TryGet(target, "singleWriter", out var flag) && flag.ValueKind is JsonValueKind.True or JsonValueKind.False
				? flag.GetBoolean()
				: string.Equals(Text(target, "kind"), "file", StringComparison.OrdinalIgnoreCase);
		}

		return new Config(name, packages, marker, os, env, process, Text(driver, "project"), singleWriter);
	}

	private static bool TryGet(JsonElement element, string name, out JsonElement value)
	{
		foreach (var property in element.EnumerateObject())
		{
			if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
			{
				value = property.Value;
				return true;
			}
		}

		value = default;
		return false;
	}

	internal static string Project(Config config, string root, bool packageMode = false, string? packageVersion = null)
	{
		var src = Path.Combine(root, "src");
		var text = new StringBuilder();
		text.AppendLine("<!-- Generated by the conformance runner from providers/" + config.Name + ".json; edit that file, not this one. -->");
		text.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
		text.AppendLine();
		text.AppendLine("  <PropertyGroup>");
		text.AppendLine("    <TargetFramework>net10.0</TargetFramework>");
		if (packageMode)
		{
			text.AppendLine("    <OutputType>Exe</OutputType>");
			text.AppendLine("    <UseAppHost>true</UseAppHost>");
		}
		text.AppendLine("  </PropertyGroup>");
		text.AppendLine();
		text.AppendLine("  <ItemGroup>");
		foreach (var (package, version) in config.Packages)
		{
			text.AppendLine($"    <PackageReference Include=\"{SecurityElement.Escape(package)}\" Version=\"{SecurityElement.Escape(version)}\" />");
		}

		if (packageMode)
		{
			text.AppendLine("    <PackageReference Include=\"Microsoft.NET.Test.Sdk\" Version=\"18.0.1\" />");
			text.AppendLine("    <PackageReference Include=\"xunit.v3\" Version=\"3.2.1\" />");
			text.AppendLine("    <PackageReference Include=\"xunit.runner.visualstudio\" Version=\"3.1.5\" />");
		}

		text.AppendLine("  </ItemGroup>");
		text.AppendLine();
		text.AppendLine("  <ItemGroup>");
		if (!packageMode && config.DriverProject is { } driverProject)
		{
			text.AppendLine($"    <ProjectReference Include=\"{SecurityElement.Escape(Path.GetFullPath(Path.Combine(root, driverProject)))}\" />");
		}

		if (packageMode)
		{
			var version = SecurityElement.Escape(packageVersion ?? throw new ArgumentNullException(nameof(packageVersion)));
			text.AppendLine($"    <PackageReference Include=\"pengdows.AdoNetApiTest.conformance.Databases\" Version=\"{version}\" />");
			text.AppendLine($"    <PackageReference Include=\"pengdows.AdoNetApiTest.conformance\" Version=\"{version}\" />");
		}
		else
		{
			text.AppendLine($"    <ProjectReference Include=\"{Path.Combine(src, "pengdows.AdoNetApiTest.conformance.Databases", "pengdows.AdoNetApiTest.conformance.Databases.csproj")}\" />");
			text.AppendLine($"    <ProjectReference Include=\"{Path.Combine(src, "pengdows.AdoNetApiTest.conformance", "pengdows.AdoNetApiTest.conformance.csproj")}\" />");
		}
		text.AppendLine("    <None Include=\"provider.json\" CopyToOutputDirectory=\"PreserveNewest\" />");
		text.AppendLine("  </ItemGroup>");
		text.AppendLine();
		text.AppendLine("</Project>");
		return text.ToString();
	}

	internal static string Code(Config config)
	{
		var text = new StringBuilder();
		text.AppendLine("// Generated by the conformance runner from providers/" + config.Name + ".json; edit that file, not this one.");
		text.AppendLine("using pengdows.AdoNetApiTest.conformance;");
		text.AppendLine("using pengdows.AdoNetApiTest.conformance.Databases;");
		text.AppendLine();
		text.AppendLine("[assembly: Xunit.AssemblyFixture(typeof(ConfiguredServer))]");
		if (config.SingleWriter)
		{
			text.AppendLine("// The database allows one writer at a time, so its test classes run one after another.");
			text.AppendLine("[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]");
		}

		text.AppendLine();
		text.AppendLine("namespace Configured." + config.Name + ";");
		text.AppendLine();
		var baseFixture = (config.StatesMarker ? "ConfiguredMarkerFixture" : "ConfiguredFixture") + (config.SingleWriter ? ", ISingleWriterFixture" : string.Empty);
		text.AppendLine($"public sealed class Fixture : {baseFixture}");
		text.AppendLine("{");
		text.AppendLine("}");
		text.AppendLine();
		text.AppendLine("// The reader and DML tests' tables exist before a test opens its connection (pengdows.flatfile allows one writer).");
		text.AppendLine($"public sealed class ValueFixture : {baseFixture}");
		text.AppendLine("{");
		text.AppendLine("\tpublic ValueFixture() => CreateValues();");
		text.AppendLine("}");
		foreach (var testBase in s_testBases)
		{
			var name = config.Name + (testBase == "ConnectionString" ? "ConnectionStringBuilder" : testBase) + "Tests";
			var fixture = s_valueTestBases.Contains(testBase) ? "ValueFixture" : "Fixture";
			text.AppendLine();
			text.AppendLine($"public sealed class {name} : {testBase}TestBase<{fixture}>");
			text.AppendLine("{");
			text.AppendLine($"\tpublic {name}({fixture} fixture)");
			text.AppendLine("\t\t: base(fixture)");
			text.AppendLine("\t{");
			text.AppendLine("\t}");
			text.AppendLine("}");
		}

		return text.ToString();
	}

	private static void WriteIfChanged(string path, string content)
	{
		if (!File.Exists(path) || File.ReadAllText(path) != content)
		{
			File.WriteAllText(path, content);
		}
	}
}
