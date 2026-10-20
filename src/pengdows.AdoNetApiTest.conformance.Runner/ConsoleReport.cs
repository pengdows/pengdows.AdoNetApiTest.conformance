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

		AppendCertifications(text, matrix);
		AppendWorkarounds(text, matrix);
		text.Append("OVERALL: ").Append(OutcomeInfo.Text(matrix.Verdict));
		return text.ToString();
	}

	/// <summary>
	/// For each consumer's workarounds, the providers in this run that still need it: one that none needs can be deleted
	/// (as far as this run's providers go), and one no test covers yet is a gap in the suite.
	/// </summary>
	private static void AppendWorkarounds(StringBuilder text, Matrix matrix)
	{
		foreach (var profile in matrix.Profiles.Where(p => p.Workarounds.Count > 0))
		{
			var statuses = matrix.Workarounds.GetValueOrDefault(profile.Id) ?? [];
			text.Append(profile.Name).Append(" workarounds").Append('\n');
			foreach (var status in statuses)
			{
				text.Append("  ").Append(status.Workaround.Id).Append(": ").Append(
					!status.Workaround.Tested ? "no test covers it yet"
					: status.NeededBy.Count == 0 ? "no provider in this run needs it"
					: "still needed by " + string.Join(", ", status.NeededBy)).Append('\n');
			}

			text.Append('\n');
		}
	}

	/// <summary>One sentence per provider and profile: it certifies, or it does not and which tests stopped it.</summary>
	private static void AppendCertifications(StringBuilder text, Matrix matrix)
	{
		if (matrix.Providers.Count == 0)
		{
			return;
		}

		text.Append('\n').Append("Certification").Append('\n');
		foreach (var provider in matrix.Providers)
		{
			if (provider.Reason is not null)
			{
				text.Append("  ").Append(provider.Id).Append(" was not run, so it certifies nothing").Append('\n');
				continue;
			}

			foreach (var certification in provider.Certifications)
			{
				text.Append("  ").Append(certification.Sentence(provider.Id));
				if (!certification.Certified)
				{
					var failures = certification.Failures;
					text.Append(": ").Append(failures.Count).Append(" of ").Append(certification.Checked).Append(" tests (")
						.Append(string.Join(", ", failures.Take(3).Select(f => $"{f.Area}.{f.Test} {OutcomeInfo.CodeOf(f.Outcome)}")));
					if (failures.Count > 3)
					{
						text.Append(", +").Append(failures.Count - 3).Append(" more");
					}

					text.Append(')');
				}

				text.Append('\n');
			}
		}

		text.Append('\n');
	}

	public static int ExitCode(Matrix matrix) => matrix.Verdict == Verdict.Pass ? 0 : 1;
}
