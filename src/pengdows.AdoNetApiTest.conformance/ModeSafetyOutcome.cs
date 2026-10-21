using System;
using System.Threading;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>Writes advisory database-mode diagnostics without affecting certification outcomes.</summary>
public static class ModeSafetyOutcome
{
	public const string Prefix = "[MODE SAFETY]";
	private static int s_sequence;

	public static void Record(string database, string mode, string status, string detail)
	{
		var line = $"{Prefix} {database} {mode}: {status} — {detail}";
		Console.WriteLine(line);
		TestContext.Current.TestOutputHelper?.WriteLine(line);
		TestContext.Current.AddAttachment("mode-safety-" + Interlocked.Increment(ref s_sequence).ToString("D4"), line);
	}
}
