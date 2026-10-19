namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>The group a shared test belongs to (its <c>Category</c> trait).</summary>
public enum Group
{
	Required,
	Optional,
	Common,
	Unclassified,
}

/// <summary>What one test did for one provider.</summary>
public enum Outcome
{
	Pass,
	Fail,
	ImplementedPass,
	ImplementedFail,
	NotImplemented,
	NotApplicable,
	Different,
	Deviation,
	NotRun,
	NotCovered,
}

/// <summary>How an outcome counts toward the provider's verdict.</summary>
public enum Effect
{
	Pass,
	Fail,
	Advisory,
}

/// <summary>How faithfully a value survived the trip through a provider.</summary>
public enum FidelityStatus
{
	Exact,
	Lossy,
	WrongType,
	Throws,
	NotApplicable,
}

public enum Verdict
{
	Pass,
	Fail,
	NotRun,
}

internal static class GroupParser
{
	public static Group Parse(string? value) =>
		Enum.TryParse<Group>(value, ignoreCase: false, out var group) && group != Group.Unclassified ? group : Group.Unclassified;
}

internal static class OutcomeInfo
{
	public static string CodeOf(Outcome outcome) => outcome switch
	{
		Outcome.Pass => "pass",
		Outcome.Fail => "fail",
		Outcome.ImplementedPass => "implemented-pass",
		Outcome.ImplementedFail => "implemented-fail",
		Outcome.NotImplemented => "not-implemented",
		Outcome.NotApplicable => "not-applicable",
		Outcome.Different => "different",
		Outcome.Deviation => "deviation",
		Outcome.NotRun => "not-run",
		Outcome.NotCovered => "not-covered",
		_ => throw new ArgumentOutOfRangeException(nameof(outcome)),
	};

	public static string MeaningOf(Outcome outcome) => outcome switch
	{
		Outcome.Pass => "The test ran and passed.",
		Outcome.Fail => "A required test failed: the provider does not meet the documented ADO.NET contract.",
		Outcome.ImplementedPass => "The provider implements this optional capability and the test passed.",
		Outcome.ImplementedFail => "The provider implements this optional capability and the test failed.",
		Outcome.NotImplemented => "The provider does not implement this optional capability. This is a pass.",
		Outcome.NotApplicable => "The check does not apply, such as a data type the database does not have. This is a pass.",
		Outcome.Different => "The provider departs from a common expectation that the contract does not require. Advisory only: it never fails the provider.",
		Outcome.Deviation => "The provider departs from documented behavior that the suite has waived as not really required (the reason is in the matrix). Reported, never hidden, and it never fails the provider.",
		Outcome.NotRun => "The test reported no result for this provider, so nothing is known. This is a failure.",
		Outcome.NotCovered => "The provider's test project has no tests for this area, so nothing is known about it. Not counted against the provider; the coverage line says how much is missing.",
		_ => throw new ArgumentOutOfRangeException(nameof(outcome)),
	};

	public static Effect EffectOf(Outcome outcome) => outcome switch
	{
		Outcome.Pass or Outcome.ImplementedPass or Outcome.NotImplemented or Outcome.NotApplicable => Effect.Pass,
		Outcome.Different or Outcome.Deviation or Outcome.NotCovered => Effect.Advisory,
		_ => Effect.Fail,
	};

	public static string CountsAs(Outcome outcome) => EffectOf(outcome).ToString().ToLowerInvariant();

	public static string Text(Verdict verdict) => verdict switch
	{
		Verdict.Pass => "PASS",
		Verdict.Fail => "FAIL",
		_ => "NOT RUN",
	};
}

internal static class FidelityInfo
{
	public static string CodeOf(FidelityStatus status) => status switch
	{
		FidelityStatus.Exact => "exact",
		FidelityStatus.Lossy => "lossy",
		FidelityStatus.WrongType => "wrong-type",
		FidelityStatus.Throws => "throws",
		FidelityStatus.NotApplicable => "not-applicable",
		_ => throw new ArgumentOutOfRangeException(nameof(status)),
	};

	public static string MeaningOf(FidelityStatus status) => status switch
	{
		FidelityStatus.Exact => "The value, and its CLR type, came back unchanged.",
		FidelityStatus.Lossy => "The type is right but the value changed: truncated, rounded, padded or shifted.",
		FidelityStatus.WrongType => "The value came back as a different CLR type than expected.",
		FidelityStatus.Throws => "The provider threw instead of returning the value.",
		FidelityStatus.NotApplicable => "The check does not apply to this type.",
		_ => throw new ArgumentOutOfRangeException(nameof(status)),
	};

	public static bool TryParse(string? text, out FidelityStatus status) =>
		Enum.TryParse(text, ignoreCase: false, out status) && Enum.IsDefined(status);
}
