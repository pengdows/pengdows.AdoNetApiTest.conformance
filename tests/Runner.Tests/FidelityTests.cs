using System.Text.Json;
using System.Xml.Linq;
using pengdows.AdoNetApiTest.conformance.Runner;
using Xunit;

namespace Runner.Tests;

public class FidelityTests
{
	private static readonly (string, string, string, string, string)[] Good =
	{
		("int", "max", "read", "Exact", null!),
		("int", "max", "write", "Exact", null!),
		("time(7)", "midnight", "roundtrip", "Exact", null!),
	};

	private static readonly (string, string, string, string, string)[] Bad =
	{
		("int", "max", "read", "Exact", null!),
		("int", "max", "write", "Lossy", "the database renders the literal as \"1\" but the parameter as \"2\""),
		("time(7)", "midnight", "roundtrip", "Throws", "InvalidCastException: Failed to convert parameter value from a TimeSpan to a DateTime."),
		("tinyint", "tinyint", "metadata", "WrongType", "DataTypes says System.SByte but the type returns System.Byte"),
	};

	private static ProviderInput Provider(string id, (string, string, string, string, string)[] findings) => new(id, id + ".Tests",
		XunitXml.Parse(new XmlBuilder().Test("Types_round_trip", "Pass", "Common", XmlBuilder.Base("TypeFidelity", "Common"), findings: findings).ToString()), null);

	[Fact]
	public void The_findings_a_test_attached_are_read_back_with_their_detail()
	{
		var raw = Assert.Single(XunitXml.Parse(new XmlBuilder().Test("Types_round_trip", "Pass", "Common", findings: Bad).ToString()));

		Assert.Equal(4, raw.Findings.Count);
		var lossy = raw.Findings.Single(f => f.Status == FidelityStatus.Lossy);
		Assert.Equal(("int", "max", "write"), (lossy.Type, lossy.Value, lossy.Path));
		Assert.Contains("renders the literal as \"1\"", lossy.Detail);
	}

	[Fact]
	public void A_finding_with_an_unknown_status_is_ignored_rather_than_guessed()
	{
		var xml = new XmlBuilder().Test("T", "Pass", "Common", findings: new[] { ("int", "max", "read", "Surprising", (string?) null!) }).ToString();

		Assert.Empty(Assert.Single(XunitXml.Parse(xml)).Findings);
	}

	[Fact]
	public void The_matrix_has_one_fidelity_row_per_type_value_and_path_with_a_cell_per_provider()
	{
		var matrix = Matrix.Build(new[] { Provider("Bad", Bad), Provider("Good", Good) });

		var write = matrix.Fidelity.Single(r => r.Type == "int" && r.Value == "max" && r.Path == "write");
		Assert.Equal(FidelityStatus.Lossy, write.Results["Bad"].Status);
		Assert.Equal(FidelityStatus.Exact, write.Results["Good"].Status);
		Assert.Equal(4, matrix.Fidelity.Count);
	}

	[Fact]
	public void A_provider_that_did_not_report_a_finding_has_no_cell_for_it()
	{
		var matrix = Matrix.Build(new[] { Provider("Bad", Bad), Provider("Good", Good) });

		var metadata = matrix.Fidelity.Single(r => r.Path == "metadata");
		Assert.True(metadata.Results.ContainsKey("Bad"));
		Assert.False(metadata.Results.ContainsKey("Good"));
	}

	[Fact]
	public void Each_provider_counts_its_findings_by_status()
	{
		var matrix = Matrix.Build(new[] { Provider("Bad", Bad), Provider("Good", Good) });

		var bad = matrix.Providers.Single(p => p.Id == "Bad").Fidelity;
		Assert.Equal(1, bad[FidelityStatus.Exact]);
		Assert.Equal(1, bad[FidelityStatus.Lossy]);
		Assert.Equal(1, bad[FidelityStatus.Throws]);
		Assert.Equal(1, bad[FidelityStatus.WrongType]);
		Assert.Equal(Enum.GetValues<FidelityStatus>().Length, bad.Count);
		Assert.Equal(3, matrix.Providers.Single(p => p.Id == "Good").Fidelity[FidelityStatus.Exact]);
	}

	[Fact]
	public void Findings_never_change_the_verdict_because_fidelity_is_advisory()
	{
		var matrix = Matrix.Build(new[] { Provider("Bad", Bad) });

		Assert.Equal(Verdict.Pass, matrix.Providers.Single().Verdict);
	}

	[Fact]
	public void Json_carries_the_fidelity_rows_providers_counts_and_a_legend()
	{
		using var json = JsonDocument.Parse(MatrixJson.Write(Matrix.Build(new[] { Provider("Bad", Bad), Provider("Good", Good) })));
		var root = json.RootElement;

		Assert.Equal(Enum.GetValues<FidelityStatus>().Length, root.GetProperty("fidelity-legend").GetArrayLength());
		var row = root.GetProperty("fidelity").EnumerateArray().Single(r => r.GetProperty("type").GetString() == "time(7)");
		Assert.Equal("roundtrip", row.GetProperty("path").GetString());
		Assert.Equal("throws", row.GetProperty("results").GetProperty("Bad").GetProperty("status").GetString());
		Assert.Contains("TimeSpan", row.GetProperty("results").GetProperty("Bad").GetProperty("detail").GetString());
		var bad = root.GetProperty("providers").EnumerateArray().Single(p => p.GetProperty("id").GetString() == "Bad");
		Assert.Equal(1, bad.GetProperty("fidelity").GetProperty("lossy").GetInt32());
		Assert.Equal(1, bad.GetProperty("fidelity").GetProperty("wrong-type").GetInt32());
	}

	[Fact]
	public void Xml_carries_the_same_fidelity_rows()
	{
		var root = XDocument.Parse(MatrixXml.Write(Matrix.Build(new[] { Provider("Bad", Bad), Provider("Good", Good) }))).Root!;

		Assert.Equal(Enum.GetValues<FidelityStatus>().Length, root.Element("fidelity-legend")!.Elements("status").Count());
		var row = root.Element("fidelity")!.Elements("finding").Single(f => (string?) f.Attribute("type") == "time(7)");
		var cell = row.Elements("result").Single(r => (string?) r.Attribute("provider") == "Bad");
		Assert.Equal("throws", (string?) cell.Attribute("status"));
		Assert.Contains("TimeSpan", cell.Value);
	}

	[Fact]
	public void The_console_line_adds_the_type_counts_when_a_provider_reported_any()
	{
		var text = ConsoleReport.Render(Matrix.Build(new[] { Provider("Bad", Bad), Provider("Good", Good) }));

		var bad = text.Split('\n').Single(l => l.StartsWith("Bad", StringComparison.Ordinal));
		Assert.Contains("Types 1 exact, 1 lossy, 1 wrong type, 1 throws", bad);
		Assert.Contains("Types 3 exact, 0 lossy, 0 wrong type, 0 throws", text.Split('\n').Single(l => l.StartsWith("Good", StringComparison.Ordinal)));
	}

	[Fact]
	public void The_console_line_is_unchanged_for_a_provider_with_no_findings()
	{
		var xml = new XmlBuilder().Test("A", "Pass", "Required", XmlBuilder.Base("Connection", "Required")).ToString();
		var text = ConsoleReport.Render(Matrix.Build(new[] { new ProviderInput("P", "P.Tests", XunitXml.Parse(xml), null) }));

		Assert.DoesNotContain("Types", text);
	}
}
