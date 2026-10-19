using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Repository.Tests;

/// <summary>
/// A waiver turns a Required failure into an advisory <c>deviation</c>. It must say why, and only a Required test may carry one.
/// </summary>
public class WaiverTests
{
	private static readonly Regex s_attribute = new(@"\[(Required|Optional|Common)Fact\((?<args>[^\]]*)\)\]", RegexOptions.Compiled);
	private static readonly Regex s_waiver = new(@"Waiver\s*=\s*""(?<text>(?:[^""\\]|\\.)*)""", RegexOptions.Compiled);

	private static System.Collections.Generic.IEnumerable<(string File, int Line, string Group, string Args)> Attributes()
	{
		var library = Path.Combine(Root(), "src", "pengdows.AdoNetApiTest.conformance");
		foreach (var file in Directory.EnumerateFiles(library, "*.cs", SearchOption.AllDirectories)
			.Where(f => !f.Split(Path.DirectorySeparatorChar).Any(s => s is "bin" or "obj")))
		{
			var lines = File.ReadAllLines(file);
			for (var i = 0; i < lines.Length; i++)
			{
				var match = s_attribute.Match(lines[i]);
				if (match.Success)
				{
					yield return (Path.GetRelativePath(Root(), file), i + 1, match.Groups[1].Value, match.Groups["args"].Value);
				}
			}
		}
	}

	[Fact]
	public void Only_Required_tests_carry_a_waiver()
	{
		var offenders = Attributes().Where(a => a.Group != "Required" && s_waiver.IsMatch(a.Args)).Select(a => $"{a.File}:{a.Line}").ToList();

		Assert.True(offenders.Count == 0, "Waiver is for Required tests only: " + string.Join(", ", offenders));
	}

	[Fact]
	public void Every_waiver_says_why()
	{
		var offenders = Attributes()
			.Select(a => (a, m: s_waiver.Match(a.Args)))
			.Where(x => x.m.Success && x.m.Groups["text"].Value.Trim().Length < 20)
			.Select(x => $"{x.a.File}:{x.a.Line}")
			.ToList();

		Assert.True(offenders.Count == 0, "A waiver needs a reason of at least 20 characters: " + string.Join(", ", offenders));
	}

	[Fact]
	public void A_waived_test_still_cites_the_documentation()
	{
		var offenders = Attributes()
			.Where(a => s_waiver.IsMatch(a.Args) && !a.Args.Contains("Contract = \"https://learn.microsoft.com/"))
			.Select(a => $"{a.File}:{a.Line}")
			.ToList();

		Assert.True(offenders.Count == 0, "A waiver excuses documented behavior, so it must cite the page: " + string.Join(", ", offenders));
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
