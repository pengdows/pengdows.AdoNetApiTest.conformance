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
		"Command", "ConnectionString", "Connection", "DbProviderFactory", "Parameter", "Transaction", "DataTypesFidelity",
	};

	public static IReadOnlyList<ProviderProject> Discover(string testsDirectory)
	{
		var root = Path.GetDirectoryName(Path.GetFullPath(testsDirectory).TrimEnd(Path.DirectorySeparatorChar))!;
		var providers = Path.Combine(root, "providers");
		if (!Directory.Exists(providers))
		{
			return Array.Empty<ProviderProject>();
		}

		return Directory.EnumerateFiles(providers, "*.json")
			.OrderBy(path => path, StringComparer.Ordinal)
			.Select(path => Generate(path, root, testsDirectory))
			.ToList();
	}

	public static ProviderProject Generate(string configPath, string root, string testsDirectory)
	{
		var config = Read(configPath);
		var directory = Path.Combine(testsDirectory, "obj", "providers", config.Name + ".Tests");
		Directory.CreateDirectory(directory);
		var project = Path.Combine(directory, config.Name + ".Tests.csproj");
		WriteIfChanged(project, Project(config, root));
		WriteIfChanged(Path.Combine(directory, "Generated.cs"), Code(config));
		WriteIfChanged(Path.Combine(directory, "provider.json"), File.ReadAllText(configPath));
		return new ProviderProject(config.Name, project, NotRunReason(config), config.ProcessEnvironment);
	}

	/// <param name="DriverProject">A driver built from source instead of a package: its project file, relative to the repository root or absolute.</param>
	internal sealed record Config(string Name, IReadOnlyList<(string Package, string Version)> Packages, bool StatesMarker,
		string? RequiredOs = null, IReadOnlyList<string>? RequiredEnv = null, IReadOnlyDictionary<string, string>? ProcessEnvironment = null,
		string? DriverProject = null);

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

		return new Config(name, packages, marker, os, env, process, Text(driver, "project"));
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

	internal static string Project(Config config, string root)
	{
		var src = Path.Combine(root, "src");
		var text = new StringBuilder();
		text.AppendLine("<!-- Generated by the conformance runner from providers/" + config.Name + ".json; edit that file, not this one. -->");
		text.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
		text.AppendLine();
		text.AppendLine("  <PropertyGroup>");
		text.AppendLine("    <TargetFramework>net10.0</TargetFramework>");
		text.AppendLine("  </PropertyGroup>");
		text.AppendLine();
		text.AppendLine("  <ItemGroup>");
		foreach (var (package, version) in config.Packages)
		{
			text.AppendLine($"    <PackageReference Include=\"{SecurityElement.Escape(package)}\" Version=\"{SecurityElement.Escape(version)}\" />");
		}

		text.AppendLine("  </ItemGroup>");
		text.AppendLine();
		text.AppendLine("  <ItemGroup>");
		if (config.DriverProject is { } driverProject)
		{
			text.AppendLine($"    <ProjectReference Include=\"{SecurityElement.Escape(Path.GetFullPath(Path.Combine(root, driverProject)))}\" />");
		}

		text.AppendLine($"    <ProjectReference Include=\"{Path.Combine(src, "pengdows.AdoNetApiTest.conformance.Databases", "pengdows.AdoNetApiTest.conformance.Databases.csproj")}\" />");
		text.AppendLine($"    <ProjectReference Include=\"{Path.Combine(src, "pengdows.AdoNetApiTest.conformance", "pengdows.AdoNetApiTest.conformance.csproj")}\" />");
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
		text.AppendLine();
		text.AppendLine("namespace Configured." + config.Name + ";");
		text.AppendLine();
		text.AppendLine($"public sealed class Fixture : {(config.StatesMarker ? "ConfiguredMarkerFixture" : "ConfiguredFixture")}");
		text.AppendLine("{");
		text.AppendLine("}");
		foreach (var testBase in s_testBases)
		{
			var name = config.Name + (testBase == "ConnectionString" ? "ConnectionStringBuilder" : testBase) + "Tests";
			text.AppendLine();
			text.AppendLine($"public sealed class {name} : {testBase}TestBase<Fixture>");
			text.AppendLine("{");
			text.AppendLine($"\tpublic {name}(Fixture fixture)");
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
