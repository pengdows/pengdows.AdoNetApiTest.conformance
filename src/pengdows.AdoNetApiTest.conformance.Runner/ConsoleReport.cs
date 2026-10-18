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
			text.Append(provider.Id.PadRight(width)).Append("  ").Append(OutcomeInfo.Text(provider.Verdict).PadRight(7)).Append("  ");
			if (provider.Reason is not null)
			{
				text.Append(provider.Reason);
			}
			else
			{
				var c = provider.Counts;
				text.Append("Required ").Append(provider.RequiredPassed).Append('/').Append(provider.RequiredTotal)
					.Append("  |  Optional ").Append(c[Outcome.ImplementedPass]).Append(" implemented pass, ")
					.Append(c[Outcome.ImplementedFail]).Append(" implemented fail, ")
					.Append(c[Outcome.NotImplemented]).Append(" not implemented")
					.Append("  |  Common ").Append(c[Outcome.Different]).Append(" differ");
			}

			text.Append('\n');
		}

		text.Append("OVERALL: ").Append(OutcomeInfo.Text(matrix.Verdict));
		return text.ToString();
	}

	public static int ExitCode(Matrix matrix) => matrix.Verdict == Verdict.Pass ? 0 : 1;
}
