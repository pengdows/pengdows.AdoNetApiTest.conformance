using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace Repository.Tests;

/// <summary>
/// Every shared test belongs to exactly one group, so a run can report a hard pass or failure for what the ADO.NET
/// contract requires without common-behavior differences or unsupported optional capabilities leaking into it.
/// </summary>
public class GroupTests
{
	private static readonly string[] s_groups = { "Required", "Optional", "Common" };

	private const string GeneratedFile = "GetValueConversionTestBase.g.cs";

	private static IEnumerable<(Type Type, MethodInfo Method, FactAttribute Fact)> SharedTests() =>
		typeof(DbFactoryTestBase<>).Assembly.GetTypes()
			.Where(t => t.IsAbstract && t.Name.Split('`')[0].EndsWith("TestBase", StringComparison.Ordinal))
			.SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
				.SelectMany(m => m.GetCustomAttributes<FactAttribute>(inherit: false).Select(f => (t, m, f))));

	private static string GroupOf(FactAttribute fact) =>
		s_groups.FirstOrDefault(g => fact.GetType().Name == g + "FactAttribute");

	[Fact]
	public void SharedTests_AreFound()
	{
		Assert.True(SharedTests().Count() > 300, "Reflection found too few shared tests to check anything.");
	}

	[Fact]
	public void EveryTest_HasExactlyOneGroupAttribute()
	{
		var offenders = SharedTests()
			.Where(x => GroupOf(x.Fact) is null)
			.Select(x => $"{x.Type.Name}.{x.Method.Name} uses [{x.Fact.GetType().Name.Replace("Attribute", "")}]")
			.ToList();

		Assert.True(offenders.Count == 0, $"{offenders.Count} tests have no group attribute: " + string.Join("; ", offenders.Take(10)));
	}

	[Fact]
	public void EveryTest_LivesInTheFolderOfItsGroup()
	{
		var offenders = SharedTests()
			.Select(x => (x, group: GroupOf(x.Fact)))
			.Where(x => x.group is not null)
			.Where(x => !Path.GetDirectoryName(x.x.Fact.SourceFilePath)!.EndsWith(Path.DirectorySeparatorChar + x.group, StringComparison.Ordinal))
			.Select(x => $"{x.x.Type.Name}.{x.x.Method.Name} is [{x.group}] in {Path.GetFileName(x.x.Fact.SourceFilePath)}")
			.ToList();

		Assert.True(offenders.Count == 0, $"{offenders.Count} tests are in the wrong folder: " + string.Join("; ", offenders.Take(10)));
	}

	[Fact]
	public void EveryTest_NamesItsDocumentedContract()
	{
		var offenders = SharedTests()
			.Where(x => GroupOf(x.Fact) is not null && Path.GetFileName(x.Fact.SourceFilePath) != GeneratedFile)
			.Where(x => !(x.Fact.GetType().GetProperty("Contract")?.GetValue(x.Fact) is string { Length: > 0 } url
				&& url.StartsWith("https://", StringComparison.Ordinal)))
			.Select(x => $"{x.Type.Name}.{x.Method.Name}")
			.ToList();

		Assert.True(offenders.Count == 0, $"{offenders.Count} tests have no Contract URL: " + string.Join("; ", offenders.Take(10)));
	}

	[Fact]
	public void GroupAttributes_EmitTheGroupTheContractAndTheReasonAsTraits()
	{
		var attribute = new RequiredFactAttribute { Contract = "https://example.test/contract", Because = "a reason" };

		var traits = ((Xunit.v3.ITraitAttribute) attribute).GetTraits().ToDictionary(t => t.Key, t => t.Value);

		Assert.Equal("Required", traits["Category"]);
		Assert.Equal("https://example.test/contract", traits["Contract"]);
		Assert.Equal("a reason", traits["Because"]);
	}

	[Fact]
	public void GroupAttributes_OmitTraitsThatAreNotSet()
	{
		var traits = ((Xunit.v3.ITraitAttribute) new CommonFactAttribute()).GetTraits().Select(t => t.Key).ToList();

		Assert.Equal(new[] { "Category" }, traits);
	}

	[Fact]
	public void RequiredTests_NeverReportSoftWarnings()
	{
		var required = Path.Combine(LibraryDirectory(), "Required");
		Assert.True(Directory.Exists(required), "The Required folder does not exist.");

		var offenders = Directory.EnumerateFiles(required, "*.cs")
			.Where(f => File.ReadAllText(f).Contains("SoftWarning.Report", StringComparison.Ordinal))
			.Select(Path.GetFileName)
			.ToList();

		Assert.True(offenders.Count == 0, "Required tests report soft warnings in: " + string.Join(", ", offenders));
	}

	private static string LibraryDirectory()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);
		while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
		{
			directory = directory.Parent;
		}

		return Path.Combine(directory!.FullName, "src", "pengdows.AdoNetApiTest.conformance");
	}
}
