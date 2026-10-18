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

	/// <summary>The prefix of the attachment name that carries each type-fidelity finding.</summary>
	public const string FindingAttachmentPrefix = "conformance-finding-";

	private static int s_findings;

	/// <summary>
	/// Records how one value of one type fared on one path (<c>read</c>, <c>write</c>, <c>roundtrip</c>, <c>type</c>,
	/// <c>where</c>, <c>metadata</c>). The runner turns the findings into the type-fidelity table.
	/// </summary>
	public static void Finding(string type, string value, string path, FidelityStatus status, string detail)
	{
		var fields = new[] { type, value, path, status.ToString(), detail ?? string.Empty };
		var encoded = string.Join("|", Array.ConvertAll(fields, Uri.EscapeDataString));
		var name = FindingAttachmentPrefix + System.Threading.Interlocked.Increment(ref s_findings).ToString("D6", System.Globalization.CultureInfo.InvariantCulture);
		TestContext.Current.AddAttachment(name, encoded);
	}

	private static void Record(string outcome, string reason)
	{
		var line = Prefix + outcome + "] " + reason;
		Console.WriteLine(line);
		TestContext.Current.TestOutputHelper?.WriteLine(line);
		TestContext.Current.AddAttachment(AttachmentName, outcome + ": " + reason);
	}
}
