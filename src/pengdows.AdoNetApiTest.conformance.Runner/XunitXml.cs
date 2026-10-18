using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>How one value of one type fared on one path, as a type-fidelity test attached it.</summary>
internal sealed record Finding(string Type, string Value, string Path, FidelityStatus Status, string? Detail);

/// <summary>One test as an xUnit v3 result file reports it.</summary>
internal sealed record RawResult(
	string Type,
	string Method,
	string? SourceFile,
	string Result,
	IReadOnlyDictionary<string, string> Traits,
	string Output,
	string? Failure,
	string? SkipReason,
	string? MarkerOutcome,
	string? MarkerReason,
	IReadOnlyList<Finding> Findings);

/// <summary>Reads the XML a provider test project writes with <c>-xml</c>.</summary>
internal static partial class XunitXml
{
	private const string AttachmentName = "conformance-outcome";
	private const string FindingPrefix = "conformance-finding-";

	public static IReadOnlyList<RawResult> Parse(string xml)
	{
		var document = XDocument.Parse(XmlText.Escape(xml));
		return document.Descendants("test").Select(ToResult).ToList();
	}

	private static RawResult ToResult(XElement test)
	{
		var traits = new Dictionary<string, string>(StringComparer.Ordinal);
		foreach (var trait in test.Element("traits")?.Elements("trait") ?? Enumerable.Empty<XElement>())
		{
			var name = (string?) trait.Attribute("name");
			if (name is not null)
			{
				traits[name] = (string?) trait.Attribute("value") ?? string.Empty;
			}
		}

		var output = test.Element("output")?.Value ?? string.Empty;
		var (markerOutcome, markerReason) = ReadMarker(test, output);

		return new RawResult(
			(string?) test.Attribute("type") ?? string.Empty,
			(string?) test.Attribute("method") ?? ((string?) test.Attribute("name") ?? string.Empty),
			(string?) test.Attribute("source-file"),
			(string?) test.Attribute("result") ?? "NotRun",
			traits,
			output,
			test.Element("failure")?.Element("message")?.Value,
			test.Element("reason")?.Value,
			markerOutcome,
			markerReason,
			ReadFindings(test));
	}

	private static List<Finding> ReadFindings(XElement test)
	{
		var findings = new List<Finding>();
		foreach (var attachment in test.Element("attachments")?.Elements("attachment") ?? Enumerable.Empty<XElement>())
		{
			var name = (string?) attachment.Attribute("name");
			if (name is null || !name.StartsWith(FindingPrefix, StringComparison.Ordinal))
			{
				continue;
			}

			var fields = attachment.Value.Split('|').Select(Uri.UnescapeDataString).ToArray();
			if (fields.Length == 5 && FidelityInfo.TryParse(fields[3], out var status))
			{
				findings.Add(new Finding(fields[0], fields[1], fields[2], status, fields[4].Length == 0 ? null : fields[4]));
			}
		}

		return findings;
	}

	private static (string? Outcome, string? Reason) ReadMarker(XElement test, string output)
	{
		var attachment = test.Element("attachments")?.Elements("attachment")
			.FirstOrDefault(a => (string?) a.Attribute("name") == AttachmentName)?.Value;
		if (attachment is not null)
		{
			var colon = attachment.IndexOf(':');
			return colon < 0
				? (attachment.Trim(), null)
				: (attachment[..colon].Trim(), attachment[(colon + 1)..].Trim());
		}

		var match = MarkerLine().Match(output);
		return match.Success ? (match.Groups[1].Value, match.Groups[2].Value.Trim()) : (null, null);
	}

	[GeneratedRegex(@"\[CONFORMANCE ([\w-]+)\] ?(.*)")]
	private static partial Regex MarkerLine();
}
