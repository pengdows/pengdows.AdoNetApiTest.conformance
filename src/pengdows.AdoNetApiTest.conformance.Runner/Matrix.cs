using System.Text.RegularExpressions;

namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>What one provider's test project produced, or why it produced nothing.</summary>
internal sealed record ProviderInput(string Id, string Project, IReadOnlyList<RawResult>? Results, string? Error)
{
	/// <summary>What the provider's test process recorded about the driver, server and runtime, and its configuration's checksum.</summary>
	public IReadOnlyDictionary<string, string>? Provenance { get; init; }
}

internal sealed record ProviderSummary(
	string Id,
	string Project,
	Verdict Verdict,
	string? Reason,
	IReadOnlyDictionary<Outcome, int> Counts,
	int RequiredPassed,
	int RequiredTotal,
	IReadOnlyDictionary<FidelityStatus, int> Fidelity,
	int AreasCovered,
	int AreasTotal)
{
	/// <summary>Required tests whose result was a failure.</summary>
	public int RequiredFailed { get; init; }

	/// <summary>Required tests in an area the provider covers that reported nothing (a crashed host, a missing result). Counted in <see cref="RequiredTotal"/>.</summary>
	public int RequiredNotRun { get; init; }

	/// <summary>Required tests in an area the provider has no tests for. Counted in <see cref="RequiredTotal"/>; any makes the verdict at best INCOMPLETE.</summary>
	public int RequiredNotCovered { get; init; }

	/// <summary>Common tests that reported nothing. Advisory: a Common test cannot hide a Required result, so it never fails the provider.</summary>
	public int CommonNotRun { get; init; }

	/// <summary>The provenance its run recorded; empty when it recorded none.</summary>
	public IReadOnlyDictionary<string, string> Provenance { get; init; } = new Dictionary<string, string>();
}

internal sealed record FidelityCell(FidelityStatus Status, string? Detail);

internal sealed record FidelityRow(string Type, string Value, string Path, IReadOnlyDictionary<string, FidelityCell> Results);

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
	private Matrix(IReadOnlyList<ProviderSummary> providers, IReadOnlyList<TestRow> tests, IReadOnlyList<FidelityRow> fidelity)
	{
		Providers = providers;
		Tests = tests;
		Fidelity = fidelity;
		Verdict = providers.Count > 0 && providers.All(p => p.Verdict == Verdict.Pass) ? Verdict.Pass : Verdict.Fail;
	}

	public IReadOnlyList<ProviderSummary> Providers { get; }

	public IReadOnlyList<TestRow> Tests { get; }

	/// <summary>Every type-fidelity finding against every provider that reported it. Advisory: it never changes a verdict.</summary>
	public IReadOnlyList<FidelityRow> Fidelity { get; }

	/// <summary>Pass only when every provider passed. A provider that did not run fails the matrix.</summary>
	public Verdict Verdict { get; }

	/// <summary>The suite the run used: its commit, and whether the working tree had changes.</summary>
	public IReadOnlyDictionary<string, string> Suite { get; set; } = new Dictionary<string, string>();

	/// <param name="suite">
	/// Every Required test the shared library declares (<see cref="RequiredCatalog"/>). A test no provider reported still
	/// gets a row, so a provider's Required total is the suite's, whichever providers run with it.
	/// </param>
	public static Matrix Build(IEnumerable<ProviderInput> inputs, IReadOnlyCollection<SuiteTest>? suite = null)
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

		foreach (var test in suite ?? Array.Empty<SuiteTest>())
		{
			var key = test.Area + "\u0001" + test.Method;
			if (!rows.ContainsKey(key))
			{
				rows[key] = new RowBuilder(new TestInfo(test.Area, test.Method, Group.Required, test.Contract, test.Because));
			}
		}

		var allAreas = rows.Values.Select(r => r.Info.Area ?? "Other").Distinct(StringComparer.Ordinal).ToList();
		var covered = providers.ToDictionary(
			p => p.Id,
			p => rows.Values.Where(r => r.Raw.ContainsKey(p.Id)).Select(r => r.Info.Area ?? "Other").ToHashSet(StringComparer.Ordinal));

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
					var area = row.Info.Area ?? "Other";
					cells[provider.Id] = covered[provider.Id].Contains(area)
						? new Cell(Outcome.NotRun, "the provider's result file has no result for this test")
						: new Cell(Outcome.NotCovered, $"the provider has no tests for the {area} area");
				}
			}

			tests.Add(new TestRow(row.Info.Area ?? "Other", row.Info.Method, row.Info.Group, row.Info.Contract, row.Info.Because, cells));
		}

		var fidelity = BuildFidelity(providers);
		return new Matrix(providers.Select(p => Summarize(p, tests, fidelity, covered[p.Id].Count, allAreas.Count)).ToList(), tests, fidelity);
	}

	private static List<FidelityRow> BuildFidelity(IEnumerable<ProviderInput> providers)
	{
		var rows = new Dictionary<(string, string, string), Dictionary<string, FidelityCell>>();
		var order = new List<(string, string, string)>();
		foreach (var provider in providers.Where(p => p.Results is not null))
		{
			foreach (var finding in provider.Results!.SelectMany(r => r.Findings))
			{
				var key = (finding.Type, finding.Value, finding.Path);
				if (!rows.TryGetValue(key, out var cells))
				{
					rows[key] = cells = new Dictionary<string, FidelityCell>(StringComparer.Ordinal);
					order.Add(key);
				}

				cells[provider.Id] = new FidelityCell(finding.Status, finding.Detail);
			}
		}

		return order.Select(key => new FidelityRow(key.Item1, key.Item2, key.Item3, rows[key])).ToList();
	}

	private static ProviderSummary Summarize(ProviderInput provider, IReadOnlyList<TestRow> tests, IReadOnlyList<FidelityRow> fidelityRows, int areasCovered, int areasTotal)
	{
		var fidelity = Enum.GetValues<FidelityStatus>().ToDictionary(s => s, _ => 0);
		foreach (var cell in fidelityRows.Select(r => r.Results.GetValueOrDefault(provider.Id)).Where(c => c is not null))
		{
			fidelity[cell!.Status]++;
		}

		var counts = Enum.GetValues<Outcome>().ToDictionary(o => o, _ => 0);
		var requiredPassed = 0;
		var requiredTotal = 0;
		var requiredFailed = 0;
		var requiredNotRun = 0;
		var requiredNotCovered = 0;
		var commonNotRun = 0;
		var hardFailure = false;
		foreach (var row in tests)
		{
			if (!row.Results.TryGetValue(provider.Id, out var cell))
			{
				continue;
			}

			counts[cell.Outcome]++;
			if (row.Group == Group.Common && cell.Outcome == Outcome.NotRun)
			{
				commonNotRun++;
			}
			else if (OutcomeInfo.EffectOf(cell.Outcome) == Effect.Fail)
			{
				hardFailure = true;
			}

			if (row.Group == Group.Required)
			{
				requiredTotal++;
				if (cell.Outcome == Outcome.NotCovered)
				{
					requiredNotCovered++;
				}
				else if (OutcomeInfo.EffectOf(cell.Outcome) == Effect.Pass)
				{
					requiredPassed++;
				}
				else if (cell.Outcome == Outcome.NotRun)
				{
					requiredNotRun++;
				}
				else
				{
					requiredFailed++;
				}
			}
		}

		string? reason = provider.Error ?? (provider.Results is { Count: > 0 } ? null : "no tests reported");
		var verdict = reason is not null
			? Verdict.NotRun
			: hardFailure ? Verdict.Fail
			: requiredNotCovered > 0 ? Verdict.Incomplete
			: Verdict.Pass;

		return new ProviderSummary(provider.Id, provider.Project, verdict, reason, counts, requiredPassed, requiredTotal, fidelity, areasCovered, areasTotal)
		{
			RequiredFailed = requiredFailed,
			RequiredNotRun = requiredNotRun,
			RequiredNotCovered = requiredNotCovered,
			CommonNotRun = commonNotRun,
			Provenance = provider.Provenance ?? new Dictionary<string, string>(),
		};
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
