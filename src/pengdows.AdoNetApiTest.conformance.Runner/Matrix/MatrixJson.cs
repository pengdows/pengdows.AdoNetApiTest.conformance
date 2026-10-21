using System.Text.Json;

namespace pengdows.AdoNetApiTest.conformance.Runner;

internal static class MatrixJson
{
	public const string Schema = "pengdows.AdoNetApiTest.conformance/matrix/1";

	private static void WriteMap(Utf8JsonWriter json, string name, IReadOnlyDictionary<string, string> values)
	{
		json.WriteStartObject(name);
		foreach (var (key, value) in values.OrderBy(pair => pair.Key, StringComparer.Ordinal))
		{
			json.WriteString(key, value);
		}

		json.WriteEndObject();
	}

	public static string Write(Matrix matrix)
	{
		using var stream = new MemoryStream();
		using (var json = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
		{
			json.WriteStartObject();
			json.WriteString("schema", Schema);
			json.WriteString("generated", DateTimeOffset.UtcNow.ToString("O"));
			json.WriteString("verdict", OutcomeInfo.Text(matrix.Verdict));
			WriteMap(json, "suite", matrix.Suite);

			json.WriteStartArray("legend");
			foreach (var outcome in Enum.GetValues<Outcome>())
			{
				json.WriteStartObject();
				json.WriteString("outcome", OutcomeInfo.CodeOf(outcome));
				json.WriteString("counts-as", OutcomeInfo.CountsAs(outcome));
				json.WriteString("meaning", OutcomeInfo.MeaningOf(outcome));
				json.WriteEndObject();
			}

			json.WriteEndArray();

			json.WriteStartArray("fidelity-legend");
			foreach (var status in Enum.GetValues<FidelityStatus>())
			{
				json.WriteStartObject();
				json.WriteString("status", FidelityInfo.CodeOf(status));
				json.WriteString("meaning", FidelityInfo.MeaningOf(status));
				json.WriteEndObject();
			}

			json.WriteEndArray();

			json.WriteStartArray("providers");
			foreach (var provider in matrix.Providers)
			{
				json.WriteStartObject();
				json.WriteString("id", provider.Id);
				json.WriteString("project", provider.Project);
				json.WriteString("verdict", OutcomeInfo.Text(provider.Verdict));
				if (provider.Reason is not null)
				{
					json.WriteString("reason", provider.Reason);
				}

				json.WriteStartObject("counts");
				foreach (var (outcome, count) in provider.Counts)
				{
					json.WriteNumber(OutcomeInfo.CodeOf(outcome), count);
				}

				json.WriteEndObject();
				json.WriteStartObject("fidelity");
				foreach (var (status, count) in provider.Fidelity)
				{
					json.WriteNumber(FidelityInfo.CodeOf(status), count);
				}

				json.WriteEndObject();
				json.WriteStartObject("coverage");
				json.WriteNumber("areas-covered", provider.AreasCovered);
				json.WriteNumber("areas-total", provider.AreasTotal);
				json.WriteEndObject();
				json.WriteStartObject("required");
				json.WriteNumber("passed", provider.RequiredPassed);
				json.WriteNumber("total", provider.RequiredTotal);
				json.WriteNumber("failed", provider.RequiredFailed);
				json.WriteNumber("not-run", provider.RequiredNotRun);
				json.WriteNumber("not-covered", provider.RequiredNotCovered);
				json.WriteEndObject();
				json.WriteStartArray("certifications");
				foreach (var certification in provider.Certifications)
				{
					json.WriteStartObject();
					json.WriteString("id", certification.Id);
					json.WriteString("name", certification.Name);
					json.WriteBoolean("certified", certification.Certified);
					json.WriteString("statement", certification.Sentence(provider.Id));
					json.WriteNumber("checked", certification.Checked);
					json.WriteStartArray("failures");
					foreach (var failure in certification.Failures)
					{
						json.WriteStartObject();
						json.WriteString("area", failure.Area);
						json.WriteString("test", failure.Test);
						json.WriteString("outcome", OutcomeInfo.CodeOf(failure.Outcome));
						json.WriteEndObject();
					}

					json.WriteEndArray();
					json.WriteEndObject();
				}

				json.WriteEndArray();
				WriteMap(json, "provenance", provider.Provenance);
				json.WriteEndObject();
			}

			json.WriteEndArray();

			json.WriteStartArray("certification-profiles");
			json.WriteStartObject();
			json.WriteString("id", Certifier.FullContractId);
			json.WriteString("name", Certifier.FullContractName);
			json.WriteString("statement", Certifier.FullContractStatement);
			json.WriteString("rule", "The provider's verdict is PASS: every Required test ran and passed, and no Optional capability it implements failed.");
			json.WriteEndObject();
			foreach (var profile in matrix.Profiles)
			{
				json.WriteStartObject();
				json.WriteString("id", profile.Id);
				json.WriteString("name", profile.Name);
				json.WriteString("statement", profile.Statement);
				json.WriteString("consumer", profile.Consumer);
				json.WriteString("source", profile.Source);
				json.WriteString("method", profile.Method);
				json.WriteStartArray("tests");
				foreach (var entry in profile.Tests)
				{
					json.WriteStartObject();
					json.WriteString("test", entry.Test);
					json.WriteString("member", entry.Member);
					json.WriteStartArray("sites");
					foreach (var site in entry.Sites)
					{
						json.WriteStringValue(site);
					}

					json.WriteEndArray();
					if (entry.Because is not null)
					{
						json.WriteString("because", entry.Because);
					}

					if (entry.IfNotImplemented is not null)
					{
						json.WriteString("if-not-implemented", entry.IfNotImplemented);
					}

					json.WriteEndObject();
				}

				json.WriteEndArray();
				json.WriteStartArray("workarounds");
				foreach (var status in matrix.Workarounds.GetValueOrDefault(profile.Id) ?? [])
				{
					var workaround = status.Workaround;
					json.WriteStartObject();
					json.WriteString("id", workaround.Id);
					json.WriteString("what", workaround.What);
					json.WriteBoolean("tested", workaround.Tested);
					json.WriteStartArray("code");
					workaround.Code.ToList().ForEach(json.WriteStringValue);
					json.WriteEndArray();
					json.WriteStartArray("tests");
					workaround.Tests.ToList().ForEach(json.WriteStringValue);
					json.WriteEndArray();
					json.WriteStartArray("fidelity");
					foreach (var criterion in workaround.Fidelity)
					{
						json.WriteStartObject();
						json.WriteString("type", criterion.TypePattern);
						json.WriteStartArray("statuses");
						criterion.Statuses.ToList().ForEach(json.WriteStringValue);
						json.WriteEndArray();
						json.WriteStartArray("paths");
						criterion.Paths.ToList().ForEach(json.WriteStringValue);
						json.WriteEndArray();
						json.WriteEndObject();
					}

					json.WriteEndArray();
					json.WriteStartArray("needed-by");
					status.NeededBy.ToList().ForEach(json.WriteStringValue);
					json.WriteEndArray();
					json.WriteEndObject();
				}

				json.WriteEndArray();
				json.WriteStartArray("excluded");
				foreach (var exclusion in profile.Excluded)
				{
					json.WriteStartObject();
					json.WriteString("test", exclusion.Test);
					json.WriteString("because", exclusion.Because);
					json.WriteEndObject();
				}

				json.WriteEndArray();
				json.WriteEndObject();
			}

			json.WriteEndArray();

			json.WriteStartArray("tests");
			foreach (var test in matrix.Tests)
			{
				json.WriteStartObject();
				json.WriteString("area", test.Area);
				json.WriteString("name", test.Name);
				json.WriteString("group", test.Group.ToString());
				if (test.Contract is not null)
				{
					json.WriteString("contract", test.Contract);
				}

				if (test.Because is not null)
				{
					json.WriteString("because", test.Because);
				}

				json.WriteStartObject("results");
				foreach (var (provider, cell) in test.Results)
				{
					json.WriteStartObject(provider);
					json.WriteString("outcome", OutcomeInfo.CodeOf(cell.Outcome));
					if (!string.IsNullOrWhiteSpace(cell.Detail))
					{
						json.WriteString("detail", cell.Detail);
					}

					json.WriteEndObject();
				}

				json.WriteEndObject();
				json.WriteEndObject();
			}

			json.WriteEndArray();

			json.WriteStartArray("fidelity");
			foreach (var row in matrix.Fidelity)
			{
				json.WriteStartObject();
				json.WriteString("type", row.Type);
				json.WriteString("value", row.Value);
				json.WriteString("path", row.Path);
				json.WriteStartObject("results");
				foreach (var (provider, cell) in row.Results)
				{
					json.WriteStartObject(provider);
					json.WriteString("status", FidelityInfo.CodeOf(cell.Status));
					if (!string.IsNullOrWhiteSpace(cell.Detail))
					{
						json.WriteString("detail", cell.Detail);
					}

					json.WriteEndObject();
				}

				json.WriteEndObject();
				json.WriteEndObject();
			}

			json.WriteEndArray();
			json.WriteEndObject();
		}

		return System.Text.Encoding.UTF8.GetString(stream.ToArray());
	}
}
