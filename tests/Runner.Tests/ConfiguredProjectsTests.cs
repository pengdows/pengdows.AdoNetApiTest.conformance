using pengdows.AdoNetApiTest.conformance.Runner;
using Xunit;

namespace Runner.Tests;

/// <summary>A provider configuration becomes a test project the runner runs like a hand-written one.</summary>
public sealed class ConfiguredProjectsTests : IDisposable
{
	private readonly string m_root = Path.Combine(Path.GetTempPath(), "conformance-config-" + Guid.NewGuid().ToString("N"));

	public ConfiguredProjectsTests()
	{
		Directory.CreateDirectory(Path.Combine(m_root, "providers"));
		Directory.CreateDirectory(Path.Combine(m_root, "tests"));
	}

	public void Dispose() => Directory.Delete(m_root, recursive: true);

	private string Write(string fileName, string json)
	{
		var path = Path.Combine(m_root, "providers", fileName);
		File.WriteAllText(path, json);
		return path;
	}

	[Fact]
	public void A_configuration_becomes_a_project_with_its_driver_package()
	{
		Write("Example.json", """
			{
			  // comments and trailing commas are allowed
			  "name": "Example",
			  "driver": { "package": "Npgsql", "version": "9.0.3", "factory": "Npgsql.NpgsqlFactory, Npgsql",
			              "packages": { "Extra.Package": "1.2.3" } },
			  "connectionString": "Host=${host}",
			}
			""");

		var provider = Assert.Single(ConfiguredProjects.Discover(Path.Combine(m_root, "tests")));

		Assert.Equal("Example", provider.Id);
		Assert.Equal(Path.Combine(m_root, "tests", "obj", "providers", "Example.Tests", "Example.Tests.csproj"), provider.Project);
		var project = File.ReadAllText(provider.Project);
		Assert.Contains("<PackageReference Include=\"Npgsql\" Version=\"9.0.3\" />", project);
		Assert.Contains("<PackageReference Include=\"Extra.Package\" Version=\"1.2.3\" />", project);
		Assert.Contains("provider.json", project);
		Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(provider.Project)!, "provider.json")));
	}

	[Fact]
	public void The_generated_code_has_a_class_per_test_base_over_the_configured_fixture()
	{
		Write("Example.json", """{ "name": "Example", "driver": { "package": "P", "factory": "F" }, "connectionString": "x" }""");

		var provider = Assert.Single(ConfiguredProjects.Discover(Path.Combine(m_root, "tests")));
		var code = File.ReadAllText(Path.Combine(Path.GetDirectoryName(provider.Project)!, "Generated.cs"));

		Assert.Contains("[assembly: Xunit.AssemblyFixture(typeof(ConfiguredServer))]", code);
		Assert.Contains("public sealed class Fixture : ConfiguredFixture", code);
		Assert.Contains("public sealed class ExampleCommandTests : CommandTestBase<Fixture>", code);
		Assert.Contains("public sealed class ExampleDataTypesFidelityTests : DataTypesFidelityTestBase<Fixture>", code);
		Assert.Contains("public sealed class ExampleDataReaderTests : DataReaderTestBase<ValueFixture>", code);
		Assert.Contains("public sealed class ExampleGetValueConversionTests : GetValueConversionTestBase<ValueFixture>", code);
		Assert.Contains("public sealed class ExampleDmlTests : DmlTestBase<ValueFixture>", code);
	}

	[Fact]
	public void The_generated_project_uses_the_requested_framework()
	{
		Write("Example.json", "{ \"name\": \"Example\", \"driver\": { \"package\": \"P\" } }");

		var provider = Assert.Single(ConfiguredProjects.Discover(Path.Combine(m_root, "tests"), "net8.0"));
		var project = File.ReadAllText(provider.Project);

		Assert.Contains("<TargetFramework>net8.0</TargetFramework>", project);
	}

	/// <summary>
	/// The reader tests' value tables are created before any test opens a connection: a file database that allows one
	/// writer per folder (pengdows.flatfile) refuses them while a test holds its connection.
	/// </summary>
	[Fact]
	public void The_reader_tests_use_a_fixture_that_creates_its_values_first()
	{
		Write("Example.json", """{ "name": "Example", "driver": { "package": "P", "factory": "F" }, "connectionString": "x" }""");

		var provider = Assert.Single(ConfiguredProjects.Discover(Path.Combine(m_root, "tests")));
		var code = File.ReadAllText(Path.Combine(Path.GetDirectoryName(provider.Project)!, "Generated.cs"));

		Assert.Contains("public sealed class ValueFixture : ConfiguredFixture", code);
		Assert.Contains("public ValueFixture() => CreateValues();", code);
	}

	/// <summary>
	/// A file database (SQLite, DuckDB, Access, pengdows.flatfile) allows one writer at a time: its test classes run one
	/// after another, and its fixtures say so, so a test that needs two writers at once is not applicable.
	/// </summary>
	[Theory]
	[InlineData("""{ "kind": "file" }""", true)]
	[InlineData("""{ "kind": "file", "singleWriter": false }""", false)]
	[InlineData("""{ "kind": "docker", "image": "i", "port": 1 }""", false)]
	[InlineData("""{ "kind": "docker", "image": "i", "port": 1, "singleWriter": true }""", true)]
	public void A_single_writer_target_runs_its_tests_one_at_a_time(string target, bool singleWriter)
	{
		Write("Example.json", $$"""{ "name": "Example", "driver": { "package": "P", "factory": "F" }, "connectionString": "x", "target": {{target}} }""");

		var provider = Assert.Single(ConfiguredProjects.Discover(Path.Combine(m_root, "tests")));
		var code = File.ReadAllText(Path.Combine(Path.GetDirectoryName(provider.Project)!, "Generated.cs"));

		Assert.Equal(singleWriter, code.Contains("[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]"));
		Assert.Equal(singleWriter, code.Contains("public sealed class Fixture : ConfiguredFixture, ISingleWriterFixture"));
		Assert.Equal(singleWriter, code.Contains("public sealed class ValueFixture : ConfiguredFixture, ISingleWriterFixture"));
	}

	[Fact]
	public void A_stated_parameter_marker_uses_the_marker_fixture()
	{
		Write("Example.json", """{ "name": "Example", "driver": { "package": "P", "factory": "F" }, "connectionString": "x", "dialect": { "parameterMarker": "?" } }""");

		var provider = Assert.Single(ConfiguredProjects.Discover(Path.Combine(m_root, "tests")));

		Assert.Contains("public sealed class Fixture : ConfiguredMarkerFixture", File.ReadAllText(Path.Combine(Path.GetDirectoryName(provider.Project)!, "Generated.cs")));
	}

	[Theory]
	[InlineData("""{ "driver": { "package": "P" } }""", "name")]
	[InlineData("""{ "name": "Bad Name", "driver": { "package": "P" } }""", "name")]
	[InlineData("""{ "name": "Example" }""", "driver.package")]
	[InlineData("""{ "name": "Example", "driver": { "factory": "F" } }""", "driver.project")]
	public void An_incomplete_configuration_says_what_is_missing(string json, string field)
	{
		var path = Write("Bad.json", json);

		var error = Assert.Throws<ArgumentException>(() => ConfiguredProjects.Read(path));

		Assert.Contains(field, error.Message);
	}

	[Fact]
	public void No_providers_folder_means_no_configured_providers() =>
		Assert.Empty(ConfiguredProjects.Discover(Path.Combine(m_root, "elsewhere", "tests")));

	[Fact]
	public void A_missing_requirement_is_a_reason_not_to_run()
	{
		var config = new ConfiguredProjects.Config("X", Array.Empty<(string, string)>(), false, null, new[] { "INCLUDE_X", "SET_ONE" });

		Assert.Equal("needs the environment variable INCLUDE_X", ConfiguredProjects.NotRunReason(config, name => name == "SET_ONE" ? "yes" : null));
		Assert.Null(ConfiguredProjects.NotRunReason(config, _ => "yes"));
		Assert.Equal("needs plan9", ConfiguredProjects.NotRunReason(new ConfiguredProjects.Config("X", Array.Empty<(string, string)>(), false, "plan9")));
	}

	[Fact]
	public void Requirements_and_process_environment_are_read()
	{
		var path = Write("Example.json", """
			{ "name": "Example", "driver": { "package": "P", "factory": "F" }, "connectionString": "x",
			  "requires": { "os": "windows", "env": [ "INCLUDE_EXAMPLE" ] },
			  "processEnvironment": { "LD_LIBRARY_PATH": "${output}/native/lib" } }
			""");

		var config = ConfiguredProjects.Read(path);

		Assert.Equal("windows", config.RequiredOs);
		Assert.Equal(new[] { "INCLUDE_EXAMPLE" }, config.RequiredEnv);
		Assert.Equal("${output}/native/lib", config.ProcessEnvironment!["LD_LIBRARY_PATH"]);
	}

	[Fact]
	public void A_driver_built_from_source_is_a_project_reference()
	{
		Write("Local.json", """{ "name": "Local", "driver": { "project": "../driver/Driver.csproj", "factory": "F" }, "connectionString": "x" }""");

		var provider = Assert.Single(ConfiguredProjects.Discover(Path.Combine(m_root, "tests")));
		var project = File.ReadAllText(provider.Project);

		Assert.Contains($"<ProjectReference Include=\"{Path.GetFullPath(Path.Combine(m_root, "../driver/Driver.csproj"))}\" />", project);
		Assert.DoesNotContain("<PackageReference Include=\"\"", project);
	}
}
