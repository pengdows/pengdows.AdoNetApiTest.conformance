using System.Reflection;
using System.Text.RegularExpressions;

namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>A test whose run killed the test process, as the log of that run names it.</summary>
internal sealed record CrashedTest(string Type, string Method, string Reason)
{
	/// <summary>The test as a failed result, grouped and described by the shared library's own attribute for it.</summary>
	public RawResult AsResult()
	{
		var traits = new Dictionary<string, string>(StringComparer.Ordinal);
		string? sourceFile = null;
		foreach (var type in typeof(RequiredFactAttribute).Assembly.GetTypes().Where(t => t.IsClass && t.Name.EndsWith("TestBase`1", StringComparison.Ordinal)))
		{
			var method = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
				.FirstOrDefault(m => m.Name == Method && m.GetCustomAttribute<ConformanceFactAttribute>() is not null);
			if (method?.GetCustomAttribute<ConformanceFactAttribute>() is not { } fact)
			{
				continue;
			}

			foreach (var trait in fact.GetTraits())
			{
				traits[trait.Key] = trait.Value;
			}

			sourceFile = type.Name[..^"TestBase`1".Length] + "TestBase.cs";
			break;
		}

		return new RawResult(Type, Method, sourceFile, "Fail", traits, string.Empty, Reason, null, null, null, []);
	}
}

/// <summary>Finds which test was running when the test process died in native code.</summary>
internal static partial class CrashDiagnosis
{
	public static CrashedTest? FindCrashedTest(string logPath)
	{
		var lines = File.ReadAllLines(logPath);
		var fatal = Array.FindLastIndex(lines, line => line.StartsWith("Fatal error.", StringComparison.Ordinal) || line.StartsWith("Stack overflow.", StringComparison.Ordinal));
		if (fatal < 0)
		{
			return null;
		}

		// The test method is the frame just above the reflection call that invoked it.
		var invoke = Array.FindIndex(lines, fatal, line => line.Contains("MethodBaseInvoker", StringComparison.Ordinal));
		if (invoke <= fatal)
		{
			return null;
		}

		var match = Frame().Match(lines[invoke - 1]);
		if (!match.Success)
		{
			return null;
		}

		var what = lines[fatal].TrimEnd('.');
		var code = fatal + 1 < lines.Length && lines[fatal + 1].StartsWith("0x", StringComparison.Ordinal) ? " " + lines[fatal + 1].Trim() : string.Empty;
		return new CrashedTest(
			match.Groups["type"].Value,
			match.Groups["method"].Value,
			$"{what}{code}: the provider crashed the test process in native code while running this test; it was excluded so the rest of the run could finish");
	}

	[GeneratedRegex(@"^\s*at (?<type>.+)\.(?<method>\w+)\([^)]*\)\s*$")]
	private static partial Regex Frame();
}
