using System.Text.RegularExpressions;

namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>What one provider's test project produced, or why it produced nothing.</summary>
internal sealed record ProviderInput(string Id, string Project, IReadOnlyList<RawResult>? Results, string? Error);

internal sealed record ProviderSummary(
	string Id,
	string Project,
	Verdict Verdict,
	string? Reason,
	IReadOnlyDictionary<Outcome, int> Counts,
	int RequiredPassed,
	int RequiredTotal);

internal sealed record TestRow(
	string Area,
	string Name,
	Group Group,
	string? Contract,
	string? Because,
	IReadOnlyDictionary<string, Cell> Results);

/// <summary>Every test against every provider, with a verdict per provider and overall.</summary>
internal sealed partial class Matrix
{
	private Matrix(IReadOnlyList<ProviderSummary> providers, IReadOnlyList<TestRow> tests)
	{
		Providers = providers;
		Tests = tests;
		Verdict = providers.Count > 0 && providers.All(p => p.Verdict == Verdict.Pass) ? Verdict.Pass : Verdict.Fail;
	}

	public IReadOnlyList<ProviderSummary> Providers { get; }

	public IReadOnlyList<TestRow> Tests { get; }

	/// <summary>Pass only when every provider passed. A provider that did not run fails the matrix.</summary>
	public Verdict Verdict { get; }

	public static Matrix Build(IEnumerable<ProviderInput> inputs)
	{
		var providers = inputs.ToList();
		var known = LearnFromTheBaseTests(providers);

		var rows = new Dictionary<string, RowBuilder>(StringComparer.Ordinal);
		foreach (var provider in providers.Where(p => p.Results is { Count: > 0 }))
		{
			foreach (var raw in provider.Results!)
			{
				var info = Describe(raw, known);
				var key = (info.Area ?? "Other") + "\u0001" + raw.Method;
				if (!rows.TryGetValue(key, out var row))
				{
					rows[key] = row = new RowBuilder(info);
				}

				row.Raw[provider.Id] = raw;
			}
		}

		var tests = new List<TestRow>();
		foreach (var row in rows.Values.OrderBy(r => r.Info.Area ?? "Other", StringComparer.Ordinal).ThenBy(r => r.Info.Group).ThenBy(r => r.Info.Method, StringComparer.Ordinal))
		{
			var cells = new Dictionary<string, Cell>(StringComparer.Ordinal);
			foreach (var provider in providers)
			{
				if (provider.Error is not null || provider.Results is not { Count: > 0 })
				{
					cells[provider.Id] = new Cell(Outcome.NotRun, provider.Error ?? "no tests reported");
				}
				else if (row.Raw.TryGetValue(provider.Id, out var raw))
				{
					cells[provider.Id] = Classifier.Classify(raw, row.Info.Group);
				}
				else if (row.Info.Group != Group.Unclassified)
				{
					cells[provider.Id] = new Cell(Outcome.NotRun, "the provider's result file has no result for this test");
				}
			}

			tests.Add(new TestRow(row.Info.Area ?? "Other", row.Info.Method, row.Info.Group, row.Info.Contract, row.Info.Because, cells));
		}

		return new Matrix(providers.Select(p => Summarize(p, tests)).ToList(), tests);
	}

	private static ProviderSummary Summarize(ProviderInput provider, IReadOnlyList<TestRow> tests)
	{
		var counts = Enum.GetValues<Outcome>().ToDictionary(o => o, _ => 0);
		var requiredPassed = 0;
		var requiredTotal = 0;
		foreach (var row in tests)
		{
			if (!row.Results.TryGetValue(provider.Id, out var cell))
			{
				continue;
			}

			counts[cell.Outcome]++;
			if (row.Group == Group.Required)
			{
				requiredTotal++;
				if (OutcomeInfo.EffectOf(cell.Outcome) == Effect.Pass)
				{
					requiredPassed++;
				}
			}
		}

		string? reason = provider.Error ?? (provider.Results is { Count: > 0 } ? null : "no tests reported");
		var verdict = reason is not null
			? Verdict.NotRun
			: counts.Any(c => OutcomeInfo.EffectOf(c.Key) == Effect.Fail && c.Value > 0) ? Verdict.Fail : Verdict.Pass;

		return new ProviderSummary(provider.Id, provider.Project, verdict, reason, counts, requiredPassed, requiredTotal);
	}

	// A test the provider overrides loses its group trait and source file, so those are learned from any provider that has the base test.
	private static Dictionary<string, TestInfo> LearnFromTheBaseTests(IEnumerable<ProviderInput> providers)
	{
		var learned = new Dictionary<string, TestInfo>(StringComparer.Ordinal);
		foreach (var raw in providers.Where(p => p.Results is not null).SelectMany(p => p.Results!))
		{
			var info = Read(raw);
			if (info.Area is not null && info.Group != Group.Unclassified && !learned.ContainsKey(raw.Method))
			{
				learned[raw.Method] = info;
			}
		}

		return learned;
	}

	private static TestInfo Describe(RawResult raw, Dictionary<string, TestInfo> known)
	{
		var own = Read(raw);
		if (own.Area is not null && own.Group != Group.Unclassified)
		{
			return own;
		}

		return known.TryGetValue(raw.Method, out var learned)
			? learned
			: own with { Area = own.Area ?? "Other" };
	}

	private static TestInfo Read(RawResult raw)
	{
		string? area = null;
		if (raw.SourceFile is not null)
		{
			var match = BaseTestFile().Match(Path.GetFileName(raw.SourceFile.Replace('\\', '/')));
			area = match.Success ? match.Groups[1].Value : null;
		}

		raw.Traits.TryGetValue("Category", out var category);
		raw.Traits.TryGetValue("Contract", out var contract);
		raw.Traits.TryGetValue("Because", out var because);
		return new TestInfo(area, raw.Method, GroupParser.Parse(category), contract, because);
	}

	[GeneratedRegex(@"^(.+?)TestBase\.")]
	private static partial Regex BaseTestFile();

	private sealed record TestInfo(string? Area, string Method, Group Group, string? Contract, string? Because);

	private sealed class RowBuilder(TestInfo info)
	{
		public TestInfo Info { get; } = info;

		public Dictionary<string, RawResult> Raw { get; } = new(StringComparer.Ordinal);
	}
}
