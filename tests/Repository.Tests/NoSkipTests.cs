using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Repository.Tests;

/// <summary>
/// Nothing is skipped. A provider that does not implement an optional capability passes that test as "not implemented"
/// (recorded through <c>ConformanceOutcome</c>); a test that cannot run for any other reason fails. A skip would hide
/// either result from the matrix.
/// </summary>
public class NoSkipTests
{
	private static readonly Regex s_skip = new(@"SkipException|Assert\.Skip|\bSkip\s*=|SkippableFact|Skip\.If|Skip\.IfNot", RegexOptions.Compiled);

	[Fact]
	public void SharedSuite_NeverSkips() => AssertNoSkips(Path.Combine(Root(), "src"));

	[Fact]
	public void ProviderProjects_NeverSkip()
	{
		var tests = Path.Combine(Root(), "tests");
		AssertNoSkips(tests, exclude: "Repository.Tests");
	}

	private static void AssertNoSkips(string directory, string exclude = null)
	{
		var offenders = Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
			.Where(f => exclude is null || !f.Split(Path.DirectorySeparatorChar).Contains(exclude))
			.Where(f => !f.Split(Path.DirectorySeparatorChar).Any(s => s is "bin" or "obj"))
			.SelectMany(f => File.ReadLines(f).Select((line, i) => (f, line, n: i + 1)))
			.Where(x => !x.line.TrimStart().StartsWith("//", StringComparison.Ordinal) && s_skip.IsMatch(x.line))
			.Select(x => $"{Path.GetRelativePath(Root(), x.f)}:{x.n}")
			.ToList();

		Assert.True(offenders.Count == 0, $"{offenders.Count} skips remain: " + string.Join(", ", offenders.Take(12)));
	}

	private static string Root()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);
		while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
		{
			directory = directory.Parent;
		}

		return directory!.FullName;
	}
}
