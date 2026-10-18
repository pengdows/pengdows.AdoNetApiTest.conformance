using pengdows.AdoNetApiTest.conformance.Runner;
using Xunit;

namespace Runner.Tests;

public class MatrixTests
{
	private static ProviderInput Provider(string id, XmlBuilder xml) => new(id, id + ".Tests", XunitXml.Parse(xml.ToString()), null);

	private static XmlBuilder PassingProvider() => new XmlBuilder()
		.Test("Connection_opens", "Pass", "Required", XmlBuilder.Base("Connection", "Required"), contract: "https://learn.microsoft.com/c")
		.Test("Batch_works", "Pass", "Optional", XmlBuilder.Base("Command", "Optional"), outcome: "not-implemented: no batches")
		.Test("Defaults_match", "Pass", "Common", XmlBuilder.Base("Parameter", "Common"));

	[Fact]
	public void A_provider_with_no_required_or_optional_failures_passes()
	{
		var matrix = Matrix.Build(new[] { Provider("Sqlite", PassingProvider()) });

		Assert.Equal(Verdict.Pass, matrix.Providers.Single().Verdict);
		Assert.Equal(Verdict.Pass, matrix.Verdict);
	}

	[Fact]
	public void A_required_failure_fails_the_provider_and_the_matrix()
	{
		var xml = PassingProvider().Test("Prepare_throws", "Fail", "Required", XmlBuilder.Base("Command", "Required"), failure: "no throw");

		var other = PassingProvider().Test("Prepare_throws", "Pass", "Required", XmlBuilder.Base("Command", "Required"));

		var matrix = Matrix.Build(new[] { Provider("Sqlite", xml), Provider("Other", other) });

		Assert.Equal(Verdict.Fail, matrix.Providers.Single(p => p.Id == "Sqlite").Verdict);
		Assert.Equal(Verdict.Pass, matrix.Providers.Single(p => p.Id == "Other").Verdict);
		Assert.Equal(Verdict.Fail, matrix.Verdict);
	}

	[Fact]
	public void An_implemented_optional_failure_fails_the_provider()
	{
		var xml = PassingProvider().Test("Schema_works", "Fail", "Optional", XmlBuilder.Base("Connection", "Optional"), failure: "bad");

		Assert.Equal(Verdict.Fail, Matrix.Build(new[] { Provider("P", xml) }).Providers.Single().Verdict);
	}

	[Fact]
	public void A_common_difference_does_not_fail_the_provider()
	{
		var xml = PassingProvider().Test("Reader_differs", "Fail", "Common", XmlBuilder.Base("DataReader", "Common"), failure: "different");

		var provider = Matrix.Build(new[] { Provider("P", xml) }).Providers.Single();

		Assert.Equal(Verdict.Pass, provider.Verdict);
		Assert.Equal(1, provider.Counts[Outcome.Different]);
	}

	[Fact]
	public void A_provider_that_could_not_run_is_not_run_and_fails_the_matrix()
	{
		var matrix = Matrix.Build(new[]
		{
			Provider("Ok", PassingProvider()),
			new ProviderInput("Down", "Down.Tests", null, "no result file (exit code 1)"),
		});

		var down = matrix.Providers.Single(p => p.Id == "Down");
		Assert.Equal(Verdict.NotRun, down.Verdict);
		Assert.Equal("no result file (exit code 1)", down.Reason);
		Assert.Equal(Verdict.Fail, matrix.Verdict);
	}

	[Fact]
	public void A_provider_with_no_results_is_not_run()
	{
		var matrix = Matrix.Build(new[] { new ProviderInput("Empty", "Empty.Tests", new List<RawResult>(), null) });

		Assert.Equal(Verdict.NotRun, matrix.Providers.Single().Verdict);
	}

	[Fact]
	public void Rows_are_shared_across_providers_by_area_and_test()
	{
		var matrix = Matrix.Build(new[] { Provider("A", PassingProvider()), Provider("B", PassingProvider()) });

		Assert.Equal(3, matrix.Tests.Count);
		var row = matrix.Tests.Single(t => t.Name == "Connection_opens");
		Assert.Equal("Connection", row.Area);
		Assert.Equal(Group.Required, row.Group);
		Assert.Equal("https://learn.microsoft.com/c", row.Contract);
		Assert.Equal(Outcome.Pass, row.Results["A"].Outcome);
		Assert.Equal(Outcome.Pass, row.Results["B"].Outcome);
	}

	private static XmlBuilder FullProvider() => PassingProvider()
		.Test("Connection_closes", "Pass", "Required", XmlBuilder.Base("Connection", "Required"));

	[Fact]
	public void A_test_in_an_area_the_provider_covers_but_did_not_report_is_not_run_and_fails()
	{
		var partial = new XmlBuilder().Test("Connection_opens", "Pass", "Required", XmlBuilder.Base("Connection", "Required"));

		var matrix = Matrix.Build(new[] { Provider("Full", FullProvider()), Provider("Partial", partial) });

		var row = matrix.Tests.Single(t => t.Name == "Connection_closes");
		Assert.Equal(Outcome.NotRun, row.Results["Partial"].Outcome);
		Assert.Equal(Verdict.Fail, matrix.Providers.Single(p => p.Id == "Partial").Verdict);
	}

	[Fact]
	public void An_area_the_provider_has_no_tests_for_is_not_covered_and_does_not_fail_it()
	{
		var partial = new XmlBuilder().Test("Connection_opens", "Pass", "Required", XmlBuilder.Base("Connection", "Required"))
			.Test("Connection_closes", "Pass", "Required", XmlBuilder.Base("Connection", "Required"));

		var matrix = Matrix.Build(new[] { Provider("Full", FullProvider()), Provider("Partial", partial) });

		var row = matrix.Tests.Single(t => t.Name == "Batch_works");
		Assert.Equal(Outcome.NotCovered, row.Results["Partial"].Outcome);
		Assert.Contains("Command", row.Results["Partial"].Detail);
		Assert.Equal(Verdict.Pass, matrix.Providers.Single(p => p.Id == "Partial").Verdict);
	}

	[Fact]
	public void A_provider_verdict_does_not_depend_on_the_providers_it_is_run_with()
	{
		var partial = new XmlBuilder().Test("Connection_opens", "Pass", "Required", XmlBuilder.Base("Connection", "Required"))
			.Test("Connection_closes", "Pass", "Required", XmlBuilder.Base("Connection", "Required"));

		var alone = Matrix.Build(new[] { Provider("Partial", partial) }).Providers.Single();
		var together = Matrix.Build(new[] { Provider("Full", FullProvider()), Provider("Partial", partial) }).Providers.Single(p => p.Id == "Partial");

		Assert.Equal(alone.Verdict, together.Verdict);
		Assert.Equal(alone.RequiredPassed, together.RequiredPassed);
		Assert.Equal(alone.RequiredTotal, together.RequiredTotal);
	}

	[Fact]
	public void Required_tests_are_accounted_for_as_passed_failed_not_run_or_not_covered()
	{
		var full = new XmlBuilder()
			.Test("Connection_opens", "Pass", "Required", XmlBuilder.Base("Connection", "Required"))
			.Test("Connection_closes", "Pass", "Required", XmlBuilder.Base("Connection", "Required"))
			.Test("Connection_state", "Pass", "Required", XmlBuilder.Base("Connection", "Required"))
			.Test("Batch_works", "Pass", "Required", XmlBuilder.Base("Command", "Required"));
		var provider = new XmlBuilder()
			.Test("Connection_opens", "Pass", "Required", XmlBuilder.Base("Connection", "Required"))
			.Test("Connection_closes", "Fail", "Required", XmlBuilder.Base("Connection", "Required"), failure: "bad");

		var summary = Matrix.Build(new[] { Provider("Full", full), Provider("P", provider) }).Providers.Single(p => p.Id == "P");

		Assert.Equal(1, summary.RequiredPassed);
		Assert.Equal(1, summary.RequiredFailed);
		Assert.Equal(1, summary.RequiredNotRun);
		Assert.Equal(1, summary.RequiredNotCovered);
		Assert.Equal(3, summary.RequiredTotal);
		Assert.Equal(summary.RequiredTotal, summary.RequiredPassed + summary.RequiredFailed + summary.RequiredNotRun);
	}

	[Fact]
	public void Coverage_counts_the_areas_the_provider_has_tests_for()
	{
		var partial = new XmlBuilder().Test("Connection_opens", "Pass", "Required", XmlBuilder.Base("Connection", "Required"));

		var summary = Matrix.Build(new[] { Provider("Full", FullProvider()), Provider("Partial", partial) }).Providers.Single(p => p.Id == "Partial");

		Assert.Equal(1, summary.AreasCovered);
		Assert.Equal(3, summary.AreasTotal);
	}

	[Fact]
	public void An_overridden_test_takes_its_group_and_area_from_another_provider()
	{
		var overridden = new XmlBuilder().Test("Connection_opens", "Pass", null, sourceFile: "/repo/tests/P.Tests/PConnectionTests.cs");

		var matrix = Matrix.Build(new[] { Provider("Base", PassingProvider()), Provider("Over", overridden) });

		var row = matrix.Tests.Single(t => t.Name == "Connection_opens");
		Assert.Equal(Group.Required, row.Group);
		Assert.Equal("Connection", row.Area);
		Assert.Equal(Outcome.Pass, row.Results["Over"].Outcome);
	}

	[Fact]
	public void Counts_are_kept_per_outcome_and_every_outcome_has_a_count()
	{
		var provider = Matrix.Build(new[] { Provider("P", PassingProvider()) }).Providers.Single();

		Assert.Equal(2, provider.Counts[Outcome.Pass]);
		Assert.Equal(1, provider.Counts[Outcome.NotImplemented]);
		Assert.Equal(0, provider.Counts[Outcome.Fail]);
		Assert.Equal(Enum.GetValues<Outcome>().Length, provider.Counts.Count);
	}
}
