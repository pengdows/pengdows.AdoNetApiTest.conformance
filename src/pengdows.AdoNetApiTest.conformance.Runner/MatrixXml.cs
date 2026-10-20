using System.Xml.Linq;

namespace pengdows.AdoNetApiTest.conformance.Runner;

internal static class MatrixXml
{
	private static XElement Map(string name, IReadOnlyDictionary<string, string> values) =>
		new(name, values.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair =>
			new XElement("value", new XAttribute("name", XmlText.Escape(pair.Key)), XmlText.Escape(pair.Value))));

	public static string Write(Matrix matrix)
	{
		var root = new XElement("conformance-matrix",
			new XAttribute("schema", MatrixJson.Schema),
			new XAttribute("generated", DateTimeOffset.UtcNow.ToString("O")),
			new XAttribute("verdict", OutcomeInfo.Text(matrix.Verdict)),
			Map("suite", matrix.Suite),
			new XElement("legend", Enum.GetValues<Outcome>().Select(o =>
				new XElement("outcome",
					new XAttribute("code", OutcomeInfo.CodeOf(o)),
					new XAttribute("counts-as", OutcomeInfo.CountsAs(o)),
					XmlText.Escape(OutcomeInfo.MeaningOf(o))))),
			new XElement("fidelity-legend", Enum.GetValues<FidelityStatus>().Select(f =>
				new XElement("status", new XAttribute("code", FidelityInfo.CodeOf(f)), XmlText.Escape(FidelityInfo.MeaningOf(f))))),
			new XElement("providers", matrix.Providers.Select(p =>
				new XElement("provider",
					new XAttribute("id", XmlText.Escape(p.Id)),
					new XAttribute("project", XmlText.Escape(p.Project)),
					new XAttribute("verdict", OutcomeInfo.Text(p.Verdict)),
					p.Reason is null ? null : new XAttribute("reason", XmlText.Escape(p.Reason)),
					new XAttribute("areas-covered", p.AreasCovered),
					new XAttribute("areas-total", p.AreasTotal),
					new XAttribute("required-passed", p.RequiredPassed),
					new XAttribute("required-total", p.RequiredTotal),
					new XAttribute("required-failed", p.RequiredFailed),
					new XAttribute("required-not-run", p.RequiredNotRun),
					new XAttribute("required-not-covered", p.RequiredNotCovered),
					new XElement("counts", p.Counts.Select(c =>
						new XElement("count", new XAttribute("outcome", OutcomeInfo.CodeOf(c.Key)), c.Value))),
					new XElement("fidelity-counts", p.Fidelity.Select(c =>
						new XElement("count", new XAttribute("status", FidelityInfo.CodeOf(c.Key)), c.Value))),
					Map("provenance", p.Provenance)))),
			new XElement("tests", matrix.Tests.Select(t =>
				new XElement("test",
					new XAttribute("area", XmlText.Escape(t.Area)),
					new XAttribute("name", XmlText.Escape(t.Name)),
					new XAttribute("group", t.Group.ToString()),
					t.Contract is null ? null : new XAttribute("contract", XmlText.Escape(t.Contract)),
					t.Because is null ? null : new XAttribute("because", XmlText.Escape(t.Because)),
					t.Results.Select(r =>
						new XElement("result",
							new XAttribute("provider", XmlText.Escape(r.Key)),
							new XAttribute("outcome", OutcomeInfo.CodeOf(r.Value.Outcome)),
							XmlText.Escape(r.Value.Detail)))))),
			new XElement("fidelity", matrix.Fidelity.Select(f =>
				new XElement("finding",
					new XAttribute("type", XmlText.Escape(f.Type)),
					new XAttribute("value", XmlText.Escape(f.Value)),
					new XAttribute("path", XmlText.Escape(f.Path)),
					f.Results.Select(r =>
						new XElement("result",
							new XAttribute("provider", XmlText.Escape(r.Key)),
							new XAttribute("status", FidelityInfo.CodeOf(r.Value.Status)),
							XmlText.Escape(r.Value.Detail)))))));

		return new XDocument(new XDeclaration("1.0", "utf-8", null), root).ToString();
	}
}
