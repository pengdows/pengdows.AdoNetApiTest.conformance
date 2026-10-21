using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace Repository.Tests;

/// <summary>
/// A certification profile (<c>profiles/*.json</c>) lists the tests a consumer of ADO.NET relies on. Every entry must name
/// a shared test that exists and cite where the consumer calls the member, or the profile certifies against nothing.
/// </summary>
public class ProfileTests
{
	public static TheoryData<string> Profiles()
	{
		var data = new TheoryData<string>();
		foreach (var file in Directory.EnumerateFiles(Path.Combine(ProviderCoverageTests.RepositoryRoot(), "profiles"), "*.json").OrderBy(f => f, StringComparer.Ordinal))
		{
			data.Add(Path.GetFileName(file));
		}

		return data;
	}

	private static JsonElement Load(string file) =>
		JsonDocument.Parse(File.ReadAllText(Path.Combine(ProviderCoverageTests.RepositoryRoot(), "profiles", file))).RootElement;

	// "Area.Name" for every shared test, the area taken from the file the way the runner takes it:
	// Required/DataReaderTestBase.Required.cs is the DataReader area.
	private static HashSet<string> SharedTests()
	{
		var library = Path.Combine(ProviderCoverageTests.RepositoryRoot(), "src", "pengdows.AdoNetApiTest.conformance");
		var tests = new HashSet<string>(StringComparer.Ordinal);
		foreach (var group in new[] { "Required", "Optional", "Common" })
		{
			foreach (var file in Directory.EnumerateFiles(Path.Combine(library, group), "*TestBase*.cs"))
			{
				var area = Path.GetFileName(file)[..Path.GetFileName(file).IndexOf("TestBase", StringComparison.Ordinal)];
				foreach (Match match in s_testMethod.Matches(File.ReadAllText(file)))
				{
					tests.Add(area + "." + match.Groups[1].Value);
				}
			}
		}

		return tests;
	}

	private static readonly Regex s_testMethod = new(@"\[(?:Required|Optional|Common)(?:Fact|Theory)[^\]]*\]\s*public\s+(?:virtual\s+)?(?:async\s+)?[\w<>]+\s+(\w+)\s*\(", RegexOptions.Compiled);

	[Fact]
	public void Profiles_AreFound() => Assert.True(Profiles().Count() >= 2, "Expected the pengdows.crud and EF Core profiles.");

	[Theory]
	[MemberData(nameof(Profiles))]
	public void Profile_SaysWhatItCertifies(string file)
	{
		var profile = Load(file);
		foreach (var name in new[] { "id", "name", "statement", "consumer", "source", "method" })
		{
			Assert.False(string.IsNullOrWhiteSpace(profile.GetProperty(name).GetString()), $"{file} has no {name}");
		}

		Assert.StartsWith("certifies as ", profile.GetProperty("statement").GetString(), StringComparison.Ordinal);
		Assert.StartsWith("https://", profile.GetProperty("source").GetString(), StringComparison.Ordinal);
	}

	[Theory]
	[MemberData(nameof(Profiles))]
	public void Profile_NamesOnlyTestsThatExist(string file)
	{
		var shared = SharedTests();
		var profile = Load(file);
		var listed = profile.GetProperty("tests").EnumerateArray().Select(t => t.GetProperty("test").GetString()!).ToList();
		var excluded = profile.TryGetProperty("excluded", out var e) ? e.EnumerateArray().Select(t => t.GetProperty("test").GetString()!).ToList() : [];
		var missing = listed.Concat(excluded).Where(t => !shared.Contains(t)).ToList();
		var both = listed.Intersect(excluded).ToList();

		Assert.True(missing.Count == 0, $"{file} names tests the suite does not have: {string.Join(", ", missing)}");
		Assert.True(both.Count == 0, $"{file} both lists and excludes: {string.Join(", ", both)}");
	}

	[Theory]
	[MemberData(nameof(Profiles))]
	public void Profile_WorkaroundsPointAtCodeAndAtListedTests(string file)
	{
		var profile = Load(file);
		if (!profile.TryGetProperty("workarounds", out var workarounds))
		{
			return;
		}

		var listed = profile.GetProperty("tests").EnumerateArray().Select(t => t.GetProperty("test").GetString()!).ToHashSet(StringComparer.Ordinal);
		var offenders = new List<string>();
		foreach (var workaround in workarounds.EnumerateArray())
		{
			var id = workaround.GetProperty("id").GetString();
			if (workaround.GetProperty("code").EnumerateArray().Any(c => !c.GetString()!.StartsWith("https://github.com/", StringComparison.Ordinal) || !c.GetString()!.Contains("#L", StringComparison.Ordinal)))
			{
				offenders.Add($"{id}: a code link without a line");
			}

			// a provider that needs a workaround must not certify, so its tests have to be in the profile
			offenders.AddRange(workaround.GetProperty("tests").EnumerateArray().Select(t => t.GetString()!).Where(t => !listed.Contains(t)).Select(t => $"{id}: {t} is not in the profile"));
		}

		Assert.True(offenders.Count == 0, string.Join("; ", offenders));
	}

	[Theory]
	[MemberData(nameof(Profiles))]
	public void Profile_EntriesCiteTheConsumersCallSites(string file)
	{
		var entries = Load(file).GetProperty("tests").EnumerateArray().ToList();
		var offenders = entries
			.Where(t => !t.TryGetProperty("sites", out var sites) || sites.GetArrayLength() == 0
				|| sites.EnumerateArray().Any(s => !s.GetString()!.StartsWith("https://github.com/", StringComparison.Ordinal) || !s.GetString()!.Contains("#L", StringComparison.Ordinal)))
			.Select(t => t.GetProperty("test").GetString())
			.ToList();
		var duplicates = entries.GroupBy(t => t.GetProperty("test").GetString()).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

		Assert.True(offenders.Count == 0, $"{file}: entries without a line-level call site: {string.Join(", ", offenders)}");
		Assert.True(duplicates.Count == 0, $"{file}: tests listed twice: {string.Join(", ", duplicates)}");
	}
}
