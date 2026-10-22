using System.Reflection;
using System.Text.Json;

namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>
/// One test a consumer relies on: the member it checks, where the consumer calls that member, and, for a capability the
/// consumer can do without, how it copes when the provider does not implement it.
/// </summary>
internal sealed record ProfileTest(string Test, string Member, IReadOnlyList<string> Sites, string? Because, string? IfNotImplemented);

/// <summary>
/// The type-fidelity findings a data-mapping workaround covers: types matching a pattern, on these paths (read, type,
/// typed, write, roundtrip, probe; none listed means any), with these statuses, and optionally only the values whose
/// label matches a pattern (the probes share one path) and only the providers whose id matches one (a workaround written
/// for one driver).
/// </summary>
internal sealed record FidelityCriterion(string TypePattern, IReadOnlyList<string> Statuses, IReadOnlyList<string> Paths, string? ValuePattern = null, string? ProviderPattern = null)
{
	public bool Matches(string provider, FidelityRow row, FidelityCell cell) =>
		(ProviderPattern is null || System.Text.RegularExpressions.Regex.IsMatch(provider, ProviderPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) &&
		Statuses.Contains(FidelityInfo.CodeOf(cell.Status)) &&
		(Paths.Count == 0 || Paths.Contains(row.Path)) &&
		System.Text.RegularExpressions.Regex.IsMatch(row.Type, TypePattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase) &&
		(ValuePattern is null || System.Text.RegularExpressions.Regex.IsMatch(row.Value, ValuePattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase));
}

/// <summary>
/// Code the consumer carries only because some provider breaks a rule, and the tests (or type-fidelity findings) that
/// show whether a provider still does. A provider that needs a workaround does not certify; once no provider needs one,
/// the consumer can delete it.
/// </summary>
internal sealed record ProfileWorkaround(string Id, string What, IReadOnlyList<string> Code, IReadOnlyList<string> Tests, IReadOnlyList<FidelityCriterion> Fidelity)
{
	public bool Tested => Tests.Count > 0 || Fidelity.Count > 0;
}

/// <summary>Which of the providers that ran still need a workaround.</summary>
internal sealed record WorkaroundStatus(ProfileWorkaround Workaround, IReadOnlyList<string> NeededBy);

/// <summary>A test on a member the consumer calls that the profile leaves out, because the consumer never creates its situation.</summary>
internal sealed record ProfileExclusion(string Test, string Because);

/// <summary>
/// What one consumer of ADO.NET (pengdows.crud, EF Core) needs from a provider, as a list of the suite's tests
/// (<c>profiles/*.json</c>, embedded in the runner), and the tests on members it calls that it leaves out, with why.
/// </summary>
internal sealed record CertificationProfile(string Id, string Name, string Statement, string Consumer, string Source, string Method, IReadOnlyList<ProfileTest> Tests, IReadOnlyList<ProfileExclusion> Excluded)
{
	public IReadOnlyList<ProfileWorkaround> Workarounds { get; init; } = [];

	private const string ResourcePrefix = "profiles/";

	public static IReadOnlyList<CertificationProfile> Load()
	{
		var assembly = typeof(CertificationProfile).Assembly;
		return assembly.GetManifestResourceNames()
			.Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal) && name.EndsWith(".json", StringComparison.Ordinal))
			.OrderBy(name => name, StringComparer.Ordinal)
			.Select(name =>
			{
				using var stream = assembly.GetManifestResourceStream(name)!;
				using var reader = new StreamReader(stream);
				return Parse(reader.ReadToEnd());
			})
			.ToList();
	}

	public static CertificationProfile Parse(string json)
	{
		using var document = JsonDocument.Parse(json);
		var root = document.RootElement;
		string Text(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString()! : null!;
		var tests = root.GetProperty("tests").EnumerateArray().Select(t => new ProfileTest(
			Text(t, "test"),
			Text(t, "member"),
			t.TryGetProperty("sites", out var sites) ? sites.EnumerateArray().Select(s => s.GetString()!).ToList() : [],
			Text(t, "because"),
			Text(t, "ifNotImplemented"))).ToList();
		var excluded = root.TryGetProperty("excluded", out var e)
			? e.EnumerateArray().Select(x => new ProfileExclusion(Text(x, "test"), Text(x, "because"))).ToList()
			: [];
		List<string> Strings(JsonElement element, string name) =>
			element.TryGetProperty(name, out var values) ? values.EnumerateArray().Select(v => v.GetString()!).ToList() : [];
		var workarounds = root.TryGetProperty("workarounds", out var w)
			? w.EnumerateArray().Select(x => new ProfileWorkaround(Text(x, "id"), Text(x, "what"), Strings(x, "code"), Strings(x, "tests"),
				x.TryGetProperty("fidelity", out var f) ? f.EnumerateArray().Select(c => new FidelityCriterion(Text(c, "type"), Strings(c, "statuses"), Strings(c, "paths"), c.TryGetProperty("value", out var v) ? v.GetString() : null,
					c.TryGetProperty("provider", out var pr) ? pr.GetString() : null)).ToList() : [])).ToList()
			: [];
		return new CertificationProfile(Text(root, "id"), Text(root, "name"), Text(root, "statement"), Text(root, "consumer"), Text(root, "source"), Text(root, "method"), tests, excluded)
		{
			Workarounds = workarounds,
		};
	}
}

/// <summary>A test that kept a provider from certifying, and what it did.</summary>
internal sealed record CertificationFailure(string Area, string Test, Outcome Outcome, string? Detail);

/// <summary>Whether a provider certifies against one profile, and the tests that stopped it.</summary>
internal sealed record Certification(string Id, string Name, string Statement, bool Certified, int Checked, IReadOnlyList<CertificationFailure> Failures)
{
	public string Sentence(string provider) => Certified
		? $"{provider} {Statement}"
		: $"{provider} does not {Statement.Replace("certifies", "certify", StringComparison.Ordinal)}";
}

internal static class Certifier
{
	public const string FullContractId = "ado.net";

	public const string FullContractName = "ADO.NET";

	public const string FullContractStatement = "certifies as fully compliant against the entire ADO.NET DbProvider contract";

	/// <summary>
	/// The full contract is the provider's verdict: every Required test ran and passed, and no Optional capability it
	/// implements failed. A profile asks only for its own tests: each must pass, implement and pass, or not apply. A
	/// required behavior may not be marked not implemented; only an optional capability may use that marker, and only when
	/// the profile explicitly records how its consumer copes without it.
	/// </summary>
	public static IReadOnlyList<Certification> Certify(ProviderSummary provider, IReadOnlyList<TestRow> tests, IReadOnlyList<CertificationProfile> profiles, IReadOnlyList<FidelityRow>? fidelity = null)
	{
		var certifications = new List<Certification>();
		var mine = tests.Where(t => t.Results.ContainsKey(provider.Id)).ToList();
		var contractFailures = mine
			.Where(t => t.Group != Group.Common && (OutcomeInfo.EffectOf(t.Results[provider.Id].Outcome) == Effect.Fail || t.Results[provider.Id].Outcome == Outcome.NotCovered))
			.Select(t => new CertificationFailure(t.Area, t.Name, t.Results[provider.Id].Outcome, t.Results[provider.Id].Detail))
			.ToList();
		certifications.Add(new Certification(FullContractId, FullContractName, FullContractStatement, provider.Verdict == Verdict.Pass,
			mine.Count(t => t.Group != Group.Common), contractFailures));

		var byName = tests.ToDictionary(t => t.Area + "." + t.Name, StringComparer.Ordinal);
		foreach (var profile in profiles)
		{
			var failures = new List<CertificationFailure>();
			foreach (var entry in profile.Tests)
			{
				if (!byName.TryGetValue(entry.Test, out var row) || !row.Results.TryGetValue(provider.Id, out var cell))
				{
					var dot = entry.Test.IndexOf('.', StringComparison.Ordinal);
					failures.Add(new CertificationFailure(entry.Test[..dot], entry.Test[(dot + 1)..], Outcome.NotRun, "no result for this provider"));
					continue;
				}

				if (!Accepts(cell.Outcome, row.Group, entry))
				{
					failures.Add(new CertificationFailure(row.Area, row.Name, cell.Outcome, cell.Detail));
				}
			}

			// a type-fidelity finding a data-mapping workaround covers means the provider still needs that workaround
			foreach (var (row, cell) in FidelityNeedingWorkarounds(provider.Id, profile, fidelity))
			{
				failures.Add(new CertificationFailure("TypeFidelity", $"{row.Type} {row.Value} ({row.Path})", Outcome.Different, FidelityInfo.CodeOf(cell.Status)));
			}

			certifications.Add(new Certification(profile.Id, profile.Name, profile.Statement, provider.Reason is null && failures.Count == 0, profile.Tests.Count, failures));
		}

		return certifications;
	}

	private static IEnumerable<(FidelityRow Row, FidelityCell Cell)> FidelityNeedingWorkarounds(string provider, CertificationProfile profile, IReadOnlyList<FidelityRow>? fidelity) =>
		(fidelity ?? [])
			.Where(r => r.Results.ContainsKey(provider))
			.Select(r => (Row: r, Cell: r.Results[provider]))
			.Where(x => profile.Workarounds.SelectMany(w => w.Fidelity).Any(c => c.Matches(provider, x.Row, x.Cell)));

	/// <summary>For each workaround, the providers that ran and still fail one of its tests or show one of its findings.</summary>
	public static IReadOnlyList<WorkaroundStatus> Workarounds(CertificationProfile profile, IReadOnlyList<ProviderSummary> providers, IReadOnlyList<FidelityRow> fidelity)
	{
		var ran = providers.Where(p => p.Reason is null).ToList();
		return profile.Workarounds.Select(w => new WorkaroundStatus(w, ran
			.Where(p =>
			{
				var failed = p.Certifications.FirstOrDefault(c => c.Id == profile.Id)?.Failures ?? [];
				return failed.Any(f => w.Tests.Contains(f.Area + "." + f.Test)) ||
					fidelity.Any(r => r.Results.TryGetValue(p.Id, out var cell) && w.Fidelity.Any(c => c.Matches(p.Id, r, cell)));
			})
			.Select(p => p.Id)
			.ToList())).ToList();
	}

	private static bool Accepts(Outcome outcome, Group group, ProfileTest entry) => outcome switch
	{
		Outcome.Pass or Outcome.ImplementedPass or Outcome.NotApplicable => true,
		Outcome.NotImplemented => group != Group.Required && entry.IfNotImplemented is not null,
		_ => false,
	};
}
