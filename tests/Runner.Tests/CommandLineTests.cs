using pengdows.AdoNetApiTest.conformance.Runner;
using Xunit;

namespace Runner.Tests;

public class CommandLineTests
{
	[Fact]
	public void Run_with_no_providers_means_all_of_them()
	{
		var options = CommandLine.Parse(new[] { "run" });

		Assert.Equal(Command.Run, options.Command);
		Assert.Empty(options.Providers);
		Assert.Null(options.Error);
	}

	[Fact]
	public void Run_takes_provider_names_and_options()
	{
		var options = CommandLine.Parse(new[] { "run", "Sqlite", "DuckDB", "--json", "a.json", "--xml", "a.xml", "--config", "Debug", "--framework", "net8.0", "--no-build", "--timeout", "5" });

		Assert.Equal(new[] { "Sqlite", "DuckDB" }, options.Providers);
		Assert.Equal("a.json", options.JsonPath);
		Assert.Equal("a.xml", options.XmlPath);
		Assert.Equal("Debug", options.Configuration);
		Assert.Equal("net8.0", options.Framework);
		Assert.True(options.NoBuild);
		Assert.Equal(TimeSpan.FromMinutes(5), options.Timeout);
	}

	[Fact]
	public void Defaults_are_release_net10_and_a_results_folder()
	{
		var options = CommandLine.Parse(new[] { "run" });

		Assert.Equal("Release", options.Configuration);
		Assert.Equal("net10.0", options.Framework);
		Assert.False(options.NoBuild);
		Assert.Equal(Path.Combine("conformance-results", "matrix.json"), options.JsonPath);
		Assert.Equal(Path.Combine("conformance-results", "matrix.xml"), options.XmlPath);
	}

	[Fact]
	public void Report_reads_existing_xunit_result_files_and_names_the_provider_from_the_file()
	{
		var options = CommandLine.Parse(new[] { "report", "out/Sqlite.xml", "out/DuckDB.xml" });

		Assert.Equal(Command.Report, options.Command);
		Assert.Equal(new[] { "out/Sqlite.xml", "out/DuckDB.xml" }, options.Files);
		Assert.Equal("Sqlite", CommandLine.ProviderIdFromFile("out/Sqlite.xml"));
	}

	[Fact]
	public void No_arguments_or_help_shows_the_usage()
	{
		Assert.Equal(Command.Help, CommandLine.Parse(Array.Empty<string>()).Command);
		Assert.Equal(Command.Help, CommandLine.Parse(new[] { "--help" }).Command);
		Assert.Contains("conformance run", CommandLine.Usage);
	}

	[Theory]
	[InlineData("explode")]
	[InlineData("run --nope")]
	[InlineData("run --json")]
	[InlineData("run --timeout abc")]
	[InlineData("report")]
	public void Bad_input_is_reported_not_guessed(string line)
	{
		var options = CommandLine.Parse(line.Split(' '));

		Assert.NotNull(options.Error);
	}
}

public class ProviderDiscoveryTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "conformance-" + Guid.NewGuid().ToString("N"));

	public ProviderDiscoveryTests()
	{
		foreach (var name in new[] { "Sqlite.Tests", "DuckDB.Tests", "Repository.Tests", "Runner.Tests" })
		{
			Directory.CreateDirectory(Path.Combine(_root, name));
			File.WriteAllText(Path.Combine(_root, name, name + ".csproj"), "<Project />");
		}

		Directory.CreateDirectory(Path.Combine(_root, "NoProject.Tests"));
		Directory.CreateDirectory(Path.Combine(_root, "Other"));
	}

	public void Dispose() => Directory.Delete(_root, recursive: true);

	[Fact]
	public void Finds_provider_projects_and_leaves_out_the_repository_and_runner_tests()
	{
		var providers = ProviderDiscovery.Discover(_root);

		Assert.Equal(new[] { "DuckDB", "Sqlite" }, providers.Select(p => p.Id).ToArray());
		Assert.EndsWith("Sqlite.Tests.csproj", providers.Single(p => p.Id == "Sqlite").Project);
	}

	[Fact]
	public void Selection_is_by_name_ignoring_case_and_unknown_names_are_an_error()
	{
		var all = ProviderDiscovery.Discover(_root);

		Assert.Equal(new[] { "Sqlite" }, ProviderDiscovery.Select(all, new[] { "sqlite" }).Select(p => p.Id).ToArray());
		Assert.Equal(2, ProviderDiscovery.Select(all, Array.Empty<string>()).Count);
		var error = Assert.Throws<ArgumentException>(() => ProviderDiscovery.Select(all, new[] { "Oracle" }));
		Assert.Contains("Oracle", error.Message);
		Assert.Contains("DuckDB", error.Message);
	}
}
