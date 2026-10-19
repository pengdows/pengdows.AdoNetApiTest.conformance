using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Repository.Tests;

/// <summary>
/// The documentation names an exception type; a provider that throws a subclass (ObjectDisposedException is an
/// InvalidOperationException) honors it. An exact-type assertion would call that a failure.
/// </summary>
public class AssertionStyleTests
{
	private static readonly Regex s_exact = new(@"Assert\.Throws(Async)?<", RegexOptions.Compiled);

	[Fact]
	public void Shared_tests_accept_a_subclass_of_the_documented_exception()
	{
		var library = Path.Combine(Root(), "src", "pengdows.AdoNetApiTest.conformance");
		var offenders = Directory.EnumerateFiles(library, "*.cs", SearchOption.AllDirectories)
			.Where(f => !f.Split(Path.DirectorySeparatorChar).Any(s => s is "bin" or "obj"))
			.SelectMany(f => File.ReadLines(f).Select((line, i) => (f, line, n: i + 1)))
			.Where(x => !x.line.TrimStart().StartsWith("//", StringComparison.Ordinal) && s_exact.IsMatch(x.line))
			.Select(x => $"{Path.GetRelativePath(Root(), x.f)}:{x.n}")
			.ToList();

		Assert.True(offenders.Count == 0, "Use Assert.ThrowsAny / ThrowsAnyAsync: " + string.Join(", ", offenders.Take(10)));
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
