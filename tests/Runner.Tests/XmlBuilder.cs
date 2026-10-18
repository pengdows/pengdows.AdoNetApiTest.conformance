using System.Text;
using System.Xml.Linq;

namespace Runner.Tests;

/// <summary>Builds the xUnit v3 XML a provider test project writes, with only the parts the runner reads.</summary>
internal sealed class XmlBuilder
{
	private readonly List<XElement> _tests = new();

	public XmlBuilder Test(string method, string result, string? group = "Required", string? sourceFile = null, string? contract = null,
		string? because = null, string? outcome = null, string? output = null, string? failure = null, string? skipReason = null,
		string type = "Provider.Tests.ProviderCommandTests")
	{
		var test = new XElement("test",
			new XAttribute("name", type + "." + method),
			new XAttribute("type", type),
			new XAttribute("method", method),
			new XAttribute("result", result));
		if (sourceFile is not null)
		{
			test.Add(new XAttribute("source-file", sourceFile));
		}

		var traits = new XElement("traits");
		if (group is not null)
		{
			traits.Add(new XElement("trait", new XAttribute("name", "Category"), new XAttribute("value", group)));
		}

		if (contract is not null)
		{
			traits.Add(new XElement("trait", new XAttribute("name", "Contract"), new XAttribute("value", contract)));
		}

		if (because is not null)
		{
			traits.Add(new XElement("trait", new XAttribute("name", "Because"), new XAttribute("value", because)));
		}

		if (traits.HasElements)
		{
			test.Add(traits);
		}

		if (output is not null)
		{
			test.Add(new XElement("output", new XCData(output)));
		}

		if (outcome is not null)
		{
			test.Add(new XElement("attachments", new XElement("attachment", new XAttribute("name", "conformance-outcome"), new XCData(outcome))));
		}

		if (failure is not null)
		{
			test.Add(new XElement("failure", new XElement("message", new XCData(failure))));
		}

		if (skipReason is not null)
		{
			test.Add(new XElement("reason", new XCData(skipReason)));
		}

		_tests.Add(test);
		return this;
	}

	public override string ToString() =>
		new XDocument(new XElement("assemblies", new XElement("assembly", new XElement("collection", _tests)))).ToString();

	public static string Base(string area, string group) =>
		$"/repo/src/pengdows.AdoNetApiTest.conformance/{group}/{area}TestBase.{group}.cs";
}
