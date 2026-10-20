using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace Repository.Tests;

/// <summary>
/// Every provider is measured against the same Required tests, so a provider project that leaves out a test base with
/// Required tests can never pass; it is reported INCOMPLETE however well its driver behaves.
/// </summary>
public class ProviderCoverageTests
{
	private static readonly string[] s_notProviders = { "Repository.Tests", "Runner.Tests" };

	internal static string RepositoryRoot()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);
		while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "pengdows.AdoNetApiTest.conformance.sln")))
		{
			directory = directory.Parent;
		}

		return directory?.FullName ?? throw new InvalidOperationException("The repository root was not found.");
	}

	/// <summary>The test bases with at least one Required test, by name without <c>TestBase</c>.</summary>
	private static string[] RequiredTestBases() =>
		typeof(DbFactoryTestBase<>).Assembly.GetTypes()
			.Where(t => t.IsGenericTypeDefinition && t.Name.Split('`')[0].EndsWith("TestBase", StringComparison.Ordinal))
			.Where(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
				.Any(m => m.GetCustomAttributes<RequiredFactAttribute>(inherit: false).Any()))
			.Select(t => t.Name.Split('`')[0])
			.OrderBy(name => name, StringComparer.Ordinal)
			.ToArray();

	/// <summary>The project's folder and the folders its <c>&lt;Compile Include="..\X\*.cs"&gt;</c> items link (the NuGet twins).</summary>
	private static string[] SourceDirectories(string directory)
	{
		var project = File.ReadAllText(Directory.EnumerateFiles(directory, "*.csproj").Single());
		return Regex.Matches(project, @"<Compile Include=""([^""]+)[\\/]\*\.cs""")
			.Select(m => Path.GetFullPath(Path.Combine(directory, m.Groups[1].Value.Replace('\\', Path.DirectorySeparatorChar))))
			.Prepend(directory)
			.ToArray();
	}

	[Fact]
	public void RequiredTestBases_AreFound()
	{
		Assert.Contains("DmlTestBase", RequiredTestBases());
		Assert.Contains("DataReaderTestBase", RequiredTestBases());
	}

	[Fact]
	public void EveryProviderProject_RunsEveryTestBaseWithRequiredTests()
	{
		var bases = RequiredTestBases();
		var offenders = Directory.EnumerateDirectories(Path.Combine(RepositoryRoot(), "tests"), "*.Tests")
			.Where(directory => !s_notProviders.Contains(Path.GetFileName(directory)))
			.Select(directory =>
			{
				var code = string.Join("\n", SourceDirectories(directory)
					.SelectMany(d => Directory.EnumerateFiles(d, "*.cs", SearchOption.TopDirectoryOnly))
					.Select(File.ReadAllText));
				var missing = bases.Where(b => !Regex.IsMatch(code, @"\b" + b + "<")).ToList();
				return (Provider: Path.GetFileName(directory), Missing: missing);
			})
			.Where(x => x.Missing.Count > 0)
			.Select(x => $"{x.Provider}: {string.Join(", ", x.Missing)}")
			.ToList();

		Assert.True(offenders.Count == 0, "Provider projects missing test bases with Required tests:\n" + string.Join("\n", offenders));
	}

	/// <summary>
	/// A fixture that declares a single writer (SQLite, DuckDB, pengdows.flatfile) only works when its test classes run one
	/// at a time, and a project for one of those drivers must declare it.
	/// </summary>
	[Fact]
	public void SingleWriterProjects_DeclareItAndRunTheirTestsOneAtATime()
	{
		var singleWriterDrivers = new[] { "Sqlite", "FlatFile", "DuckDB" };
		var offenders = Directory.EnumerateDirectories(Path.Combine(RepositoryRoot(), "tests"), "*.Tests")
			.Where(directory => !s_notProviders.Contains(Path.GetFileName(directory)))
			.Select(directory =>
			{
				var code = string.Join("\n", SourceDirectories(directory)
					.SelectMany(d => Directory.EnumerateFiles(d, "*.cs", SearchOption.TopDirectoryOnly))
					.Select(File.ReadAllText));
				var name = Path.GetFileName(directory);
				var declares = code.Contains("ISingleWriterFixture");
				var serial = code.Contains("DisableTestParallelization = true");
				var expected = singleWriterDrivers.Any(d => name.Contains(d, StringComparison.Ordinal));
				return declares == expected && serial == expected ? null : $"{name}: declares {declares}, serial {serial}, expected {expected}";
			})
			.Where(x => x is not null)
			.ToList();

		Assert.True(offenders.Count == 0, "Single-writer projects out of step:\n" + string.Join("\n", offenders));
	}

	/// <summary>
	/// The library's asynchronous tests exist in every build that has the asynchronous API, net10.0 included: the runner
	/// reads the Required catalog from that build, and most provider projects run it.
	/// </summary>
	[Theory]
	[InlineData(typeof(ConnectionTestBase<>), "CloseAsync_works")]
	[InlineData(typeof(ConnectionTestBase<>), "CloseAsync_can_be_called_before_open")]
	[InlineData(typeof(DataReaderTestBase<>), "IsDBNullAsync_returns_value_state")]
	public void AsynchronousTests_AreInTheNet10Build(Type testBase, string method) =>
		Assert.NotNull(testBase.GetMethod(method));
}
