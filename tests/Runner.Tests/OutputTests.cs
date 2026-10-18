using System.Text.Json;
using System.Xml.Linq;
using pengdows.AdoNetApiTest.conformance.Runner;
using Xunit;

namespace Runner.Tests;

public class OutputTests
{
	private static Matrix Sample()
	{
		var failing = new XmlBuilder()
			.Test("Connection_opens", "Pass", "Required", XmlBuilder.Base("Connection", "Required"), contract: "https://learn.microsoft.com/c", because: "docs say so")
			.Test("Batch_works", "Pass", "Optional", XmlBuilder.Base("Command", "Optional"), outcome: "not-implemented: no batches")
			.Test("Prepare_throws", "Fail", "Required", XmlBuilder.Base("Command", "Required"), failure: "no throw")
			.Test("Defaults_match", "Pass", "Common", XmlBuilder.Base("Parameter", "Common"), outcome: "different: returns null");
		var passing = new XmlBuilder()
			.Test("Connection_opens", "Pass", "Required", XmlBuilder.Base("Connection", "Required"))
			.Test("Batch_works", "Pass", "Optional", XmlBuilder.Base("Command", "Optional"))
			.Test("Prepare_throws", "Pass", "Required", XmlBuilder.Base("Command", "Required"))
			.Test("Defaults_match", "Pass", "Common", XmlBuilder.Base("Parameter", "Common"));
		return Matrix.Build(new[]
		{
			new ProviderInput("Bad", "Bad.Tests", XunitXml.Parse(failing.ToString()), null),
			new ProviderInput("Good", "Good.Tests", XunitXml.Parse(passing.ToString()), null),
		});
	}

	[Fact]
	public void Json_has_a_legend_providers_and_tests_with_one_cell_per_provider()
	{
		using var json = JsonDocument.Parse(MatrixJson.Write(Sample()));
		var root = json.RootElement;

		Assert.Equal("FAIL", root.GetProperty("verdict").GetString());
		Assert.Equal(Enum.GetValues<Outcome>().Length, root.GetProperty("legend").GetArrayLength());
		Assert.Equal(2, root.GetProperty("providers").GetArrayLength());
		var tests = root.GetProperty("tests");
		Assert.Equal(4, tests.GetArrayLength());

		var connection = tests.EnumerateArray().Single(t => t.GetProperty("name").GetString() == "Connection_opens");
		Assert.Equal("Connection", connection.GetProperty("area").GetString());
		Assert.Equal("Required", connection.GetProperty("group").GetString());
		Assert.Equal("https://learn.microsoft.com/c", connection.GetProperty("contract").GetString());
		Assert.Equal("docs say so", connection.GetProperty("because").GetString());
		Assert.Equal("pass", connection.GetProperty("results").GetProperty("Bad").GetProperty("outcome").GetString());
	}

	[Fact]
	public void Json_cells_name_the_outcome_and_keep_failure_detail()
	{
		using var json = JsonDocument.Parse(MatrixJson.Write(Sample()));
		var prepare = json.RootElement.GetProperty("tests").EnumerateArray().Single(t => t.GetProperty("name").GetString() == "Prepare_throws");

		var bad = prepare.GetProperty("results").GetProperty("Bad");
		Assert.Equal("fail", bad.GetProperty("outcome").GetString());
		Assert.Equal("no throw", bad.GetProperty("detail").GetString());
		Assert.Equal("pass", prepare.GetProperty("results").GetProperty("Good").GetProperty("outcome").GetString());

		var batch = json.RootElement.GetProperty("tests").EnumerateArray().Single(t => t.GetProperty("name").GetString() == "Batch_works");
		Assert.Equal("not-implemented", batch.GetProperty("results").GetProperty("Bad").GetProperty("outcome").GetString());
		Assert.Equal("implemented-pass", batch.GetProperty("results").GetProperty("Good").GetProperty("outcome").GetString());
	}

	[Fact]
	public void Json_providers_carry_the_verdict_and_counts()
	{
		using var json = JsonDocument.Parse(MatrixJson.Write(Sample()));
		var bad = json.RootElement.GetProperty("providers").EnumerateArray().Single(p => p.GetProperty("id").GetString() == "Bad");

		Assert.Equal("FAIL", bad.GetProperty("verdict").GetString());
		Assert.Equal(1, bad.GetProperty("counts").GetProperty("fail").GetInt32());
		Assert.Equal(1, bad.GetProperty("counts").GetProperty("not-implemented").GetInt32());
		Assert.Equal(1, bad.GetProperty("counts").GetProperty("different").GetInt32());
	}

	[Fact]
	public void Xml_carries_the_same_matrix()
	{
		var doc = XDocument.Parse(MatrixXml.Write(Sample()));
		var root = doc.Root!;

		Assert.Equal("FAIL", (string?) root.Attribute("verdict"));
		Assert.Equal(Enum.GetValues<Outcome>().Length, root.Element("legend")!.Elements("outcome").Count());
		Assert.Equal(2, root.Element("providers")!.Elements("provider").Count());
		var test = root.Element("tests")!.Elements("test").Single(t => (string?) t.Attribute("name") == "Prepare_throws");
		Assert.Equal("Command", (string?) test.Attribute("area"));
		Assert.Equal("fail", (string?) test.Elements("result").Single(r => (string?) r.Attribute("provider") == "Bad").Attribute("outcome"));
		Assert.Equal("no throw", test.Elements("result").Single(r => (string?) r.Attribute("provider") == "Bad").Value);
	}

	[Fact]
	public void Control_characters_in_a_failure_message_do_not_break_the_xml()
	{
		var xml = new XmlBuilder().Test("A", "Fail", "Required", XmlBuilder.Base("Connection", "Required"), failure: "Expected: \"a@CTRL1@b\" but was \"@CTRL0@\"").ToString()
			.Replace("@CTRL1@", "\u0001").Replace("@CTRL0@", "\u0000");
		var matrix = Matrix.Build(new[] { new ProviderInput("P", "P.Tests", XunitXml.Parse(xml), null) });

		var text = MatrixXml.Write(matrix);
		var detail = XDocument.Parse(text).Descendants("result").Single().Value;

		Assert.Contains("a\\u0001b", detail);
		Assert.Contains("\\u0000", detail);
	}

	[Fact]
	public void Control_characters_survive_in_the_json_too()
	{
		var xml = new XmlBuilder().Test("A", "Fail", "Required", XmlBuilder.Base("Connection", "Required"), failure: "a@CTRL1@b").ToString().Replace("@CTRL1@", "\u0001");
		var matrix = Matrix.Build(new[] { new ProviderInput("P", "P.Tests", XunitXml.Parse(xml), null) });

		using var json = JsonDocument.Parse(MatrixJson.Write(matrix));

		var detail = json.RootElement.GetProperty("tests")[0].GetProperty("results").GetProperty("P").GetProperty("detail").GetString();
		Assert.Equal("a\\u0001b", detail);
	}

	[Fact]
	public void The_legend_says_what_each_outcome_counts_as()
	{
		using var json = JsonDocument.Parse(MatrixJson.Write(Sample()));
		var legend = json.RootElement.GetProperty("legend").EnumerateArray().ToDictionary(e => e.GetProperty("outcome").GetString()!, e => e.GetProperty("counts-as").GetString());

		Assert.Equal("pass", legend["not-implemented"]);
		Assert.Equal("fail", legend["implemented-fail"]);
		Assert.Equal("advisory", legend["different"]);
	}

	[Fact]
	public void Console_report_shows_one_line_per_provider_and_an_overall_verdict()
	{
		var lines = ConsoleReport.Render(Sample()).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.TrimEnd()).ToList();

		Assert.Contains(lines, l => l.StartsWith("Bad", StringComparison.Ordinal) && l.Contains("FAIL") && l.Contains("Required 1/2"));
		Assert.Contains(lines, l => l.StartsWith("Good", StringComparison.Ordinal) && l.Contains("PASS") && l.Contains("Required 2/2"));
		Assert.Equal("OVERALL: FAIL", lines[^1]);
	}

	[Fact]
	public void Console_report_says_why_a_provider_did_not_run()
	{
		var matrix = Matrix.Build(new[] { new ProviderInput("Down", "Down.Tests", null, "no result file (exit code 1)") });

		var text = ConsoleReport.Render(matrix);

		Assert.Contains("NOT RUN", text);
		Assert.Contains("no result file (exit code 1)", text);
		Assert.EndsWith("OVERALL: FAIL", text.TrimEnd());
	}

	[Fact]
	public void Console_report_passes_when_every_provider_passes()
	{
		var xml = new XmlBuilder().Test("A", "Pass", "Required", XmlBuilder.Base("Connection", "Required"));
		var matrix = Matrix.Build(new[] { new ProviderInput("P", "P.Tests", XunitXml.Parse(xml.ToString()), null) });

		Assert.EndsWith("OVERALL: PASS", ConsoleReport.Render(matrix).TrimEnd());
		Assert.Equal(0, ConsoleReport.ExitCode(matrix));
		Assert.Equal(1, ConsoleReport.ExitCode(Sample()));
	}
}
