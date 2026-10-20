using System.Text;

namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>The simple pass or fail a command line wants: one line per provider and an overall verdict.</summary>
internal static class ConsoleReport
{
	public static string Render(Matrix matrix)
	{
		var text = new StringBuilder();
		var width = Math.Max(8, matrix.Providers.Select(p => p.Id.Length).DefaultIfEmpty(0).Max());
		foreach (var provider in matrix.Providers)
		{
			text.Append(provider.Id.PadRight(width)).Append("  ").Append(OutcomeInfo.Text(provider.Verdict).PadRight(10)).Append("  ");
			if (provider.Reason is not null)
			{
				text.Append(provider.Reason);
			}
			else
			{
				var c = provider.Counts;
				text.Append("Required ").Append(provider.RequiredPassed).Append('/').Append(provider.RequiredTotal);
				var lost = new List<string>();
				if (provider.RequiredFailed > 0)
				{
					lost.Add(provider.RequiredFailed + " failed");
				}

				if (provider.RequiredNotRun > 0)
				{
					lost.Add(provider.RequiredNotRun + " not run");
				}

				if (provider.RequiredNotCovered > 0)
				{
					lost.Add(provider.RequiredNotCovered + " not covered");
				}

				if (lost.Count > 0)
				{
					text.Append(" (").Append(string.Join(", ", lost)).Append(')');
				}

				text.Append("  |  Optional ")
					.Append(c[Outcome.ImplementedPass]).Append(" implemented pass, ")
					.Append(c[Outcome.ImplementedFail]).Append(" implemented fail, ")
					.Append(c[Outcome.NotImplemented]).Append(" not implemented")
					.Append("  |  Common ").Append(c[Outcome.Different]).Append(" differ");
				if (provider.CommonNotRun > 0)
				{
					text.Append(", ").Append(provider.CommonNotRun).Append(" not run");
				}

				if (provider.AreasCovered < provider.AreasTotal)
				{
					text.Append("  |  Coverage ").Append(provider.AreasCovered).Append(" of ").Append(provider.AreasTotal).Append(" areas");
				}

				var f = provider.Fidelity;
				if (f.Values.Sum() > 0)
				{
					text.Append("  |  Types ").Append(f[FidelityStatus.Exact]).Append(" exact, ")
						.Append(f[FidelityStatus.Lossy]).Append(" lossy, ")
						.Append(f[FidelityStatus.WrongType]).Append(" wrong type, ")
						.Append(f[FidelityStatus.Throws]).Append(" throws");
				}
			}

			text.Append('\n');
		}

		text.Append("OVERALL: ").Append(OutcomeInfo.Text(matrix.Verdict));
		return text.ToString();
	}

	public static int ExitCode(Matrix matrix) => matrix.Verdict == Verdict.Pass ? 0 : 1;
}
