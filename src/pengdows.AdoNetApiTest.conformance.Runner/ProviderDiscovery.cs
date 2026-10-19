namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <param name="NotRunReason">Why the provider cannot run here (an operating system, an unset variable); it is reported as not run without building.</param>
/// <param name="Environment">Variables for the test process, with <c>${output}</c> for its assembly's folder.</param>
internal sealed record ProviderProject(string Id, string Project, string? NotRunReason = null, IReadOnlyDictionary<string, string>? Environment = null);

internal static class ProviderDiscovery
{
	private static readonly string[] s_notProviders = { "Repository.Tests", "Runner.Tests" };

	/// <summary>The provider test projects: every <c>X.Tests</c> folder that holds <c>X.Tests.csproj</c>.</summary>
	public static IReadOnlyList<ProviderProject> Discover(string testsDirectory) =>
		Directory.EnumerateDirectories(testsDirectory, "*.Tests")
			.Select(directory => (name: Path.GetFileName(directory), directory))
			.Where(x => !s_notProviders.Contains(x.name, StringComparer.Ordinal))
			.Select(x => (x.name, project: Path.Combine(x.directory, x.name + ".csproj")))
			.Where(x => File.Exists(x.project))
			.Select(x => new ProviderProject(x.name[..^".Tests".Length], x.project))
			.OrderBy(p => p.Id, StringComparer.Ordinal)
			.ToList();

	/// <summary>The named providers, or all of them when none are named. An unknown name is an error.</summary>
	public static IReadOnlyList<ProviderProject> Select(IReadOnlyList<ProviderProject> all, IReadOnlyList<string> names)
	{
		if (names.Count == 0)
		{
			return all;
		}

		var chosen = new List<ProviderProject>();
		foreach (var name in names)
		{
			var match = all.FirstOrDefault(p => string.Equals(p.Id, name, StringComparison.OrdinalIgnoreCase))
				?? throw new ArgumentException($"No provider named '{name}'. Providers: {string.Join(", ", all.Select(p => p.Id))}.");
			chosen.Add(match);
		}

		return chosen;
	}
}
