namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>One cell of the matrix: what a test did for a provider, and the detail behind it.</summary>
internal sealed record Cell(Outcome Outcome, string? Detail);

internal static class Classifier
{
	/// <summary>
	/// Turns a raw result into an outcome. Nothing may be skipped, so a skip is a failure in the groups that can fail.
	/// A Common test never fails the provider: a failure there is a difference. A Required failure is always a failure.
	/// </summary>
	public static Cell Classify(RawResult raw, Group group)
	{
		switch (raw.Result)
		{
			case "Fail":
				return new Cell(Failed(group), raw.Failure);
			case "Skip":
			case "NotRun":
				return new Cell(Failed(group), "skipped" + (string.IsNullOrWhiteSpace(raw.SkipReason) ? string.Empty : ": " + raw.SkipReason));
			default:
				return Passed(raw, group);
		}
	}

	private static Outcome Failed(Group group) => group switch
	{
		Group.Required => Outcome.Fail,
		Group.Optional => Outcome.ImplementedFail,
		_ => Outcome.Different,
	};

	private static Cell Passed(RawResult raw, Group group) => raw.MarkerOutcome switch
	{
		"not-implemented" => new Cell(Outcome.NotImplemented, raw.MarkerReason),
		"not-applicable" => new Cell(Outcome.NotApplicable, raw.MarkerReason),
		"different" => new Cell(Outcome.Different, raw.MarkerReason),
		_ => new Cell(group == Group.Optional ? Outcome.ImplementedPass : Outcome.Pass, null),
	};
}
