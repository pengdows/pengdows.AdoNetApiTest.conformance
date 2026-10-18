using pengdows.AdoNetApiTest.conformance.Runner;
using Xunit;

namespace Runner.Tests;

/// <summary>The README is how someone learns to run the check, so it must name every outcome and command the runner has.</summary>
public class ReadmeTests
{
	private static string Readme()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);
		while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
		{
			directory = directory.Parent;
		}

		return File.ReadAllText(Path.Combine(directory!.FullName, "README.md"));
	}

	[Fact]
	public void Every_outcome_is_explained_with_what_it_counts_as()
	{
		var readme = Readme();

		foreach (var outcome in Enum.GetValues<Outcome>())
		{
			var row = readme.Split('\n').SingleOrDefault(l => l.StartsWith($"| `{OutcomeInfo.CodeOf(outcome)}` |", StringComparison.Ordinal));
			Assert.True(row is not null, $"README has no table row for the outcome '{OutcomeInfo.CodeOf(outcome)}'.");
			Assert.Contains($"| {OutcomeInfo.CountsAs(outcome)} |", row);
		}
	}

	[Theory]
	[InlineData("-- run Sqlite")]
	[InlineData("-- report out/Sqlite.xml")]
	[InlineData("--no-build")]
	[InlineData("--json")]
	[InlineData("--xml")]
	[InlineData("NOT RUN")]
	[InlineData("OVERALL:")]
	public void The_commands_and_results_are_shown(string text) => Assert.Contains(text, Readme());

	[Fact]
	public void The_usage_text_documents_every_option_the_parser_accepts()
	{
		foreach (var option in new[] { "--json", "--xml", "--config", "--framework", "--no-build", "--tests", "--timeout" })
		{
			Assert.Contains(option, CommandLine.Usage);
		}
	}
}
