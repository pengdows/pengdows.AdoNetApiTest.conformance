using System.Text.Json;

namespace pengdows.AdoNetApiTest.conformance.Runner;

internal static class MatrixJson
{
	public const string Schema = "pengdows.AdoNetApiTest.conformance/matrix/1";

	public static string Write(Matrix matrix)
	{
		using var stream = new MemoryStream();
		using (var json = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
		{
			json.WriteStartObject();
			json.WriteString("schema", Schema);
			json.WriteString("generated", DateTimeOffset.UtcNow.ToString("O"));
			json.WriteString("verdict", OutcomeInfo.Text(matrix.Verdict));

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
				json.WriteStartObject("required");
				json.WriteNumber("passed", provider.RequiredPassed);
				json.WriteNumber("total", provider.RequiredTotal);
				json.WriteEndObject();
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
			json.WriteEndObject();
		}

		return System.Text.Encoding.UTF8.GetString(stream.ToArray());
	}
}
