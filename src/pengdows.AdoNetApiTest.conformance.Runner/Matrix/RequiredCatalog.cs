using System.Reflection;

namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>A Required test as the shared library declares it, whether or not any provider in the run reported it.</summary>
internal sealed record SuiteTest(string Area, string Method, string? Contract, string? Because);

/// <summary>
/// Every Required test in the shared library, read from its <see cref="RequiredFactAttribute"/>s. It is the same for every
/// provider, so a provider's Required total does not depend on which areas it has tests for or which providers it runs
/// with: a Required test it never ran is counted as not covered, never left out.
/// </summary>
internal static class RequiredCatalog
{
	private const string Suffix = "TestBase`1";

	public static IReadOnlyList<SuiteTest> Load() => Load(typeof(RequiredFactAttribute).Assembly);

	internal static IReadOnlyList<SuiteTest> Load(Assembly library) =>
		library.GetTypes()
			.Where(type => type.IsClass && type.Name.EndsWith(Suffix, StringComparison.Ordinal))
			.SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
				.Select(method => (method, attribute: method.GetCustomAttribute<RequiredFactAttribute>()))
				.Where(x => x.attribute is not null)
				.Select(x => new SuiteTest(type.Name[..^Suffix.Length], x.method.Name, x.attribute!.Contract, x.attribute.Because)))
			.OrderBy(test => test.Area, StringComparer.Ordinal)
			.ThenBy(test => test.Method, StringComparer.Ordinal)
			.ToList();
}
