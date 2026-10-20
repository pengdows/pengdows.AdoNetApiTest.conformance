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
				WriteMap(json, "provenance", provider.Provenance);
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
