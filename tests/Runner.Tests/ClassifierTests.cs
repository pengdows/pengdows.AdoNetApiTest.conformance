using pengdows.AdoNetApiTest.conformance.Runner;
using Xunit;

namespace Runner.Tests;

public class ClassifierTests
{
	private static Outcome Classify(string method, string result, string? group, string? outcome = null, string? output = null,
		string? failure = null, string? skipReason = null)
	{
		var xml = new XmlBuilder().Test(method, result, group, outcome: outcome, output: output, failure: failure, skipReason: skipReason).ToString();
		var raw = Assert.Single(XunitXml.Parse(xml));
		return Classifier.Classify(raw, GroupParser.Parse(group)).Outcome;
	}

	[Fact]
	public void Required_pass_is_pass() => Assert.Equal(Outcome.Pass, Classify("A", "Pass", "Required"));

	[Fact]
	public void Required_fail_is_fail() => Assert.Equal(Outcome.Fail, Classify("A", "Fail", "Required", failure: "boom"));

	[Fact]
	public void Optional_pass_without_a_marker_is_implemented_pass() => Assert.Equal(Outcome.ImplementedPass, Classify("A", "Pass", "Optional"));

	[Fact]
	public void Optional_pass_with_not_implemented_is_not_implemented() =>
		Assert.Equal(Outcome.NotImplemented, Classify("A", "Pass", "Optional", outcome: "not-implemented: no batches"));

	[Fact]
	public void Optional_fail_is_implemented_fail() => Assert.Equal(Outcome.ImplementedFail, Classify("A", "Fail", "Optional", failure: "boom"));

	[Fact]
	public void Common_pass_is_pass() => Assert.Equal(Outcome.Pass, Classify("A", "Pass", "Common"));

	[Fact]
	public void Common_pass_with_a_difference_is_different() =>
		Assert.Equal(Outcome.Different, Classify("A", "Pass", "Common", outcome: "different: provider throws"));

	[Fact]
	public void Common_fail_is_different_not_fail() => Assert.Equal(Outcome.Different, Classify("A", "Fail", "Common", failure: "boom"));

	[Fact]
	public void Not_applicable_marker_is_not_applicable() =>
		Assert.Equal(Outcome.NotApplicable, Classify("A", "Pass", "Common", outcome: "not-applicable: no such type"));

	[Fact]
	public void The_marker_in_the_output_counts_when_there_is_no_attachment() =>
		Assert.Equal(Outcome.NotImplemented, Classify("A", "Pass", "Optional", output: "[CONFORMANCE not-implemented] no timeouts\n"));

	[Fact]
	public void A_skip_is_a_failure_because_nothing_may_be_skipped()
	{
		Assert.Equal(Outcome.Fail, Classify("A", "Skip", "Required", skipReason: "later"));
		Assert.Equal(Outcome.ImplementedFail, Classify("A", "Skip", "Optional", skipReason: "later"));
	}

	[Fact]
	public void A_test_with_no_group_is_advisory_when_it_fails() => Assert.Equal(Outcome.Different, Classify("A", "Fail", null, failure: "boom"));

	[Fact]
	public void The_failure_message_is_kept_as_the_detail()
	{
		var xml = new XmlBuilder().Test("A", "Fail", "Required", failure: "Assert.Equal() Failure").ToString();

		var cell = Classifier.Classify(Assert.Single(XunitXml.Parse(xml)), Group.Required);

		Assert.Equal("Assert.Equal() Failure", cell.Detail);
	}

	[Theory]
	[InlineData(Outcome.Pass, Effect.Pass)]
	[InlineData(Outcome.ImplementedPass, Effect.Pass)]
	[InlineData(Outcome.NotImplemented, Effect.Pass)]
	[InlineData(Outcome.NotApplicable, Effect.Pass)]
	[InlineData(Outcome.Fail, Effect.Fail)]
	[InlineData(Outcome.ImplementedFail, Effect.Fail)]
	[InlineData(Outcome.Different, Effect.Advisory)]
	[InlineData(Outcome.NotRun, Effect.Fail)]
	public void Every_outcome_counts_one_way(Outcome outcome, Effect effect) => Assert.Equal(effect, OutcomeInfo.EffectOf(outcome));

	[Fact]
	public void Every_outcome_has_a_code_and_a_meaning()
	{
		foreach (var outcome in Enum.GetValues<Outcome>())
		{
			Assert.False(string.IsNullOrWhiteSpace(OutcomeInfo.CodeOf(outcome)));
			Assert.False(string.IsNullOrWhiteSpace(OutcomeInfo.MeaningOf(outcome)));
		}
	}
}
