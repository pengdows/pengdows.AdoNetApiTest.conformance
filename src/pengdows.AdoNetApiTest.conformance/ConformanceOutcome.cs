using System;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Records why a test ended without checking its full behavior, then lets the test pass. Nothing in the suite is
/// skipped: a skip would hide the reason from the results matrix. The outcome is written to the test's output as a
/// <c>[CONFORMANCE outcome] reason</c> line and attached to the test result, which the runner reads.
/// </summary>
public static class ConformanceOutcome
{
	/// <summary>The text every recorded outcome line starts with.</summary>
	public const string Prefix = "[CONFORMANCE ";

	/// <summary>The attachment name that carries the outcome.</summary>
	public const string AttachmentName = "conformance-outcome";

	/// <summary>An optional capability the provider does not implement. This passes.</summary>
	public static void NotImplemented(string reason) => Record("not-implemented", reason);

	/// <summary>The check does not apply, such as a data type the database does not have. This passes.</summary>
	public static void NotApplicable(string reason) => Record("not-applicable", reason);

	/// <summary>The provider departs from a common expectation the contract does not require. This passes.</summary>
	public static void Different(string reason) => Record("different", reason);

	private static void Record(string outcome, string reason)
	{
		var line = Prefix + outcome + "] " + reason;
		Console.WriteLine(line);
		TestContext.Current.TestOutputHelper?.WriteLine(line);
		TestContext.Current.AddAttachment(AttachmentName, outcome + ": " + reason);
	}
}
