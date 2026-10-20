using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>
/// Turns a matrix (<c>matrix.json</c>) into a static site: an index of every provider with its certification status, and
/// a page per provider with the evidence behind it. It reads only the matrix, so anyone with a published matrix can
/// build the same site. A provider is <b>Certified</b> when its verdict is PASS: every Required test ran and passed and no
/// Optional test failed.
/// </summary>
internal static class SiteGenerator
{
	private const int MaxRows = 400;

	public static string Write(string matrixPath, string outDirectory)
	{
		using var document = JsonDocument.Parse(File.ReadAllText(matrixPath));
		var root = document.RootElement;
		var providers = root.GetProperty("providers").EnumerateArray().ToList();
		var tests = root.GetProperty("tests").EnumerateArray().ToList();
		var fidelity = root.TryGetProperty("fidelity", out var f) ? f.EnumerateArray().ToList() : new List<JsonElement>();

		Directory.CreateDirectory(outDirectory);
		File.WriteAllText(Path.Combine(outDirectory, "index.html"), Index(root, providers));
		foreach (var provider in providers)
		{
			var id = Text(provider, "id")!;
			File.WriteAllText(Path.Combine(outDirectory, FileName(id)), ProviderPage(root, provider, tests, fidelity));
		}

		return Path.Combine(outDirectory, "index.html");
	}

	internal static string FileName(string id) => new string(id.Select(c => char.IsLetterOrDigit(c) || c is '_' or '-' ? c : '_').ToArray()) + ".html";

	private static (string Label, string Css, int Rank) Status(string? verdict) => verdict switch
	{
		"PASS" => ("Certified", "certified", 0),
		"INCOMPLETE" => ("Incomplete", "incomplete", 1),
		"FAIL" => ("Not certified", "failed", 2),
		_ => ("Not tested", "untested", 3),
	};

	private static string Index(JsonElement root, List<JsonElement> providers)
	{
		var html = Page("ADO.NET provider conformance", out var body);
		body.Append("<header><h1>ADO.NET provider conformance</h1>");
		body.Append("<p class=\"lede\">Each provider is tested against the documented <code>System.Data.Common</code> contract, " +
			"and every native type of its database is pushed through it to find values that come back changed. " +
			"Every provider is measured against the same Required tests.</p></header>");

		var counts = providers.GroupBy(p => Status(Text(p, "verdict")).Label).ToDictionary(g => g.Key, g => g.Count());
		body.Append("<ul class=\"chips\">");
		foreach (var status in new[] { "PASS", "INCOMPLETE", "FAIL", "NOT RUN" }.Select(Status))
		{
			body.Append(CultureInfo.InvariantCulture, $"<li class=\"chip {status.Css}\"><strong>{counts.GetValueOrDefault(status.Label)}</strong> {E(status.Label)}</li>");
		}

		body.Append("</ul>");

		body.Append("<section class=\"key\"><h2>What the statuses mean</h2><dl>");
		body.Append("<dt><span class=\"badge certified\">Certified</span></dt><dd>Every Required test ran and passed, and no Optional capability the provider implements failed.</dd>");
		body.Append("<dt><span class=\"badge incomplete\">Incomplete</span></dt><dd>Nothing failed, but some Required tests have not run for this provider yet, so it cannot be certified.</dd>");
		body.Append("<dt><span class=\"badge failed\">Not certified</span></dt><dd>At least one Required test, or an Optional capability the provider implements, failed. Each failure names the documentation it breaks.</dd>");
		body.Append("<dt><span class=\"badge untested\">Not tested</span></dt><dd>The provider could not be run, for the reason given.</dd>");
		body.Append("</dl><p>Type fidelity and the comparison of a provider's <code>DataTypes</code> with its server are reported for every provider but do not decide the status.</p></section>");

		body.Append("<section><h2>Providers</h2><div class=\"scroll\"><table class=\"providers\"><thead><tr>" +
			"<th>Provider</th><th>Status</th><th>Driver</th><th>Server</th><th>Required</th><th class=\"num\">Failed</th>" +
			"<th class=\"num\">Not covered</th><th class=\"num\">Optional failed</th><th class=\"num\">Types exact</th></tr></thead><tbody>");
		foreach (var provider in providers.OrderBy(p => Status(Text(p, "verdict")).Rank).ThenBy(p => Text(p, "id"), StringComparer.OrdinalIgnoreCase))
		{
			var id = Text(provider, "id")!;
			var status = Status(Text(provider, "verdict"));
			var provenance = Map(provider, "provenance");
			body.Append("<tr>");
			body.Append(CultureInfo.InvariantCulture, $"<td><a href=\"{FileName(id)}\">{E(id)}</a></td>");
			body.Append(CultureInfo.InvariantCulture, $"<td><span class=\"badge {status.Css}\">{E(status.Label)}</span></td>");
			if (Text(provider, "reason") is { } reason)
			{
				body.Append(CultureInfo.InvariantCulture, $"<td colspan=\"8\" class=\"reason\">{E(reason)}</td>");
			}
			else
			{
				var required = provider.GetProperty("required");
				var passed = Number(required, "passed");
				var total = Number(required, "total");
				var counts2 = provider.GetProperty("counts");
				var types = provider.GetProperty("fidelity");
				var checkedTypes = types.EnumerateObject().Where(p => p.Name != "not-applicable").Sum(p => p.Value.GetInt32());
				body.Append(CultureInfo.InvariantCulture, $"<td>{E(Driver(provenance))}</td>");
				body.Append(CultureInfo.InvariantCulture, $"<td>{E(Server(provenance))}</td>");
				body.Append(CultureInfo.InvariantCulture, $"<td class=\"required\">{Bar(required)}<span class=\"fraction\">{passed}/{total}</span></td>");
				body.Append(CultureInfo.InvariantCulture, $"<td class=\"num\">{Number(required, "failed")}</td>");
				body.Append(CultureInfo.InvariantCulture, $"<td class=\"num\">{Number(required, "not-covered")}</td>");
				body.Append(CultureInfo.InvariantCulture, $"<td class=\"num\">{Number(counts2, "implemented-fail")}</td>");
				body.Append(CultureInfo.InvariantCulture, $"<td class=\"num\">{(checkedTypes == 0 ? "—" : $"{Number(types, "exact")}/{checkedTypes}")}</td>");
			}

			body.Append("</tr>");
		}

		body.Append("</tbody></table></div></section>");
		body.Append(Footer(root));
		return Close(html, body);
	}

	private static string ProviderPage(JsonElement root, JsonElement provider, List<JsonElement> tests, List<JsonElement> fidelity)
	{
		var id = Text(provider, "id")!;
		var status = Status(Text(provider, "verdict"));
		var html = Page(id + " — ADO.NET provider conformance", out var body);
		body.Append("<p class=\"back\"><a href=\"index.html\">← All providers</a></p>");
		body.Append(CultureInfo.InvariantCulture, $"<header><h1>{E(id)}</h1><p><span class=\"badge big {status.Css}\">{E(status.Label)}</span></p></header>");

		if (Text(provider, "reason") is { } reason)
		{
			body.Append(CultureInfo.InvariantCulture, $"<p class=\"reason\">{E(reason)}</p>");
		}
		else
		{
			var required = provider.GetProperty("required");
			body.Append(CultureInfo.InvariantCulture,
				$"<p class=\"summary\">{Bar(required)} <strong>{Number(required, "passed")}</strong> of {Number(required, "total")} Required tests passed: " +
				$"{Number(required, "failed")} failed, {Number(required, "not-run")} not run, {Number(required, "not-covered")} not covered.</p>");
		}

		var provenance = Map(provider, "provenance");
		var suite = Map(root, "suite");
		body.Append("<section><h2>What was tested</h2><table class=\"facts\"><tbody>");
		Fact(body, "Driver", provenance.GetValueOrDefault("driverAssembly"));
		Fact(body, "Driver version", provenance.GetValueOrDefault("driverVersion"));
		Fact(body, "Factory", provenance.GetValueOrDefault("factory"));
		Fact(body, "Server version", provenance.GetValueOrDefault("serverVersion"));
		Fact(body, "Server image", provenance.GetValueOrDefault("image"));
		Fact(body, "Runtime", provenance.GetValueOrDefault("runtime"));
		Fact(body, "Operating system", provenance.GetValueOrDefault("os"));
		Fact(body, "Configuration", provenance.TryGetValue("config", out var config) ? $"{config} (sha256 {provenance.GetValueOrDefault("configSha256")})" : null);
		Fact(body, "Suite commit", suite.TryGetValue("commit", out var commit) ? commit + (suite.GetValueOrDefault("dirty") == "true" ? " (with local changes)" : string.Empty) : null);
		Fact(body, "Generated", Text(root, "generated"));
		body.Append("</tbody></table></section>");

		var mine = tests
			.Select(t => (Test: t, Result: t.GetProperty("results").TryGetProperty(id, out var r) ? r : (JsonElement?) null))
			.Where(x => x.Result is not null)
			.Select(x => (x.Test, Result: x.Result!.Value, Outcome: Text(x.Result!.Value, "outcome")))
			.ToList();

		Section(body, "Required failures", "Each breaks a behavior the linked documentation states.",
			mine.Where(x => Text(x.Test, "group") == "Required" && x.Outcome == "fail"), withContract: true);
		Section(body, "Optional failures", "The provider implements this optional capability, and it failed.",
			mine.Where(x => x.Outcome == "implemented-fail"), withContract: true);
		Section(body, "Required tests that did not run", "The provider covers this area but reported no result (a crashed or timed-out test host).",
			mine.Where(x => Text(x.Test, "group") == "Required" && x.Outcome == "not-run"), withContract: true);

		var notCovered = mine.Where(x => x.Outcome == "not-covered" && Text(x.Test, "group") == "Required")
			.GroupBy(x => Text(x.Test, "area")).OrderBy(g => g.Key, StringComparer.Ordinal).ToList();
		if (notCovered.Count > 0)
		{
			body.Append("<section><h2>Not covered</h2><p class=\"note\">Required tests in areas this provider has no tests for yet. They are counted in the total but have not run, which is why it cannot be certified.</p><ul class=\"areas\">");
			foreach (var area in notCovered)
			{
				body.Append(CultureInfo.InvariantCulture, $"<li><strong>{E(area.Key)}</strong> — {area.Count()} Required</li>");
			}

			body.Append("</ul></section>");
		}

		var findings = fidelity
			.Select(row => (Row: row, Cell: row.GetProperty("results").TryGetProperty(id, out var c) ? c : (JsonElement?) null))
			.Where(x => x.Cell is not null)
			.Select(x => (Type: Text(x.Row, "type")!, Value: Text(x.Row, "value")!, Path: Text(x.Row, "path")!, Status: Text(x.Cell!.Value, "status")!, Detail: Text(x.Cell!.Value, "detail")))
			.ToList();
		if (findings.Count > 0)
		{
			var types = provider.GetProperty("fidelity");
			body.Append("<section><h2>Type fidelity</h2><p class=\"note\">Values of each native type written and read back through the provider. Advisory: it does not decide the status.</p><ul class=\"chips small\">");
			foreach (var count in types.EnumerateObject())
			{
				body.Append(CultureInfo.InvariantCulture, $"<li class=\"chip fidelity-{E(count.Name)}\"><strong>{count.Value.GetInt32()}</strong> {E(count.Name)}</li>");
			}

			body.Append("</ul>");
			bool NotTested((string Type, string Value, string Path, string Status, string? Detail) x) =>
				x.Value == "(not covered)" || (x.Detail?.StartsWith("not covered:", StringComparison.Ordinal) ?? false);
			var serverRows = findings.Where(x => x.Value is "(server)" or "(advertised)" && !NotTested(x)).ToList();
			var untested = findings.Where(NotTested).ToList();
			if (serverRows.Count > 0)
			{
				body.Append("<h3>DataTypes compared with the server</h3><p class=\"note\">A provider's <code>DataTypes</code> collection should describe the server it is connected to.</p>");
				Table(body, new[] { "Type", "Finding" }, serverRows.Select(x => new[] { x.Type.Replace(" (DataTypes)", string.Empty), x.Detail ?? string.Empty }));
			}

			if (untested.Count > 0)
			{
				body.Append("<h3>Types not tested</h3><p class=\"note\">Listed by the provider or the server, but no values were generated for them.</p>");
				Table(body, new[] { "Type", "Why" }, untested.Select(x => new[] { x.Type.Replace(" (DataTypes)", string.Empty), (x.Detail ?? string.Empty).Replace("not covered: ", string.Empty) }));
			}

			var problems = findings.Where(x => x.Status is "lossy" or "wrong-type" or "throws" && x.Value is not "(server)" and not "(advertised)" and not "(not covered)").ToList();
			if (problems.Count > 0)
			{
				body.Append("<h3>Values that did not come back exactly</h3>");
				Table(body, new[] { "Type", "Value", "Path", "Status", "Detail" }, problems.Take(MaxRows).Select(x => new[] { x.Type, x.Value, x.Path, x.Status, x.Detail ?? string.Empty }));
				if (problems.Count > MaxRows)
				{
					body.Append(CultureInfo.InvariantCulture, $"<p class=\"note\">{problems.Count - MaxRows} more are in the matrix.</p>");
				}
			}
		}

		var differences = mine.Where(x => x.Outcome == "different").ToList();
		if (differences.Count > 0)
		{
			body.Append(CultureInfo.InvariantCulture, $"<section><h2>Common differences</h2><details><summary>{differences.Count} departures from common expectations the contract does not require (advisory)</summary>");
			Table(body, new[] { "Area", "Test", "Detail" }, differences.Take(MaxRows).Select(x => new[] { Text(x.Test, "area") ?? string.Empty, Text(x.Test, "name") ?? string.Empty, Text(x.Result, "detail") ?? string.Empty }));
			body.Append("</details></section>");
		}

		body.Append(Footer(root));
		return Close(html, body);
	}

	private static void Section(StringBuilder body, string title, string note, IEnumerable<(JsonElement Test, JsonElement Result, string? Outcome)> rows, bool withContract)
	{
		var list = rows.OrderBy(x => Text(x.Test, "area"), StringComparer.Ordinal).ThenBy(x => Text(x.Test, "name"), StringComparer.Ordinal).ToList();
		if (list.Count == 0)
		{
			return;
		}

		body.Append(CultureInfo.InvariantCulture, $"<section><h2>{E(title)} <span class=\"count\">{list.Count}</span></h2><p class=\"note\">{E(note)}</p><div class=\"scroll\"><table><thead><tr><th>Area</th><th>Test</th><th>Result</th></tr></thead><tbody>");
		foreach (var (test, result, _) in list)
		{
			var name = Text(test, "name") ?? string.Empty;
			var contract = Text(test, "contract");
			var nameCell = withContract && contract is not null ? $"<a href=\"{E(contract)}\">{E(name)}</a>" : E(name);
			body.Append(CultureInfo.InvariantCulture, $"<tr><td>{E(Text(test, "area"))}</td><td>{nameCell}</td>");
			body.Append(CultureInfo.InvariantCulture, $"<td class=\"detail\">{E(FirstLine(Text(result, "detail")))}</td></tr>");
		}

		body.Append("</tbody></table></div></section>");
	}

	private static void Table(StringBuilder body, string[] headers, IEnumerable<string[]> rows)
	{
		body.Append("<div class=\"scroll\"><table><thead><tr>");
		foreach (var header in headers)
		{
			body.Append("<th>").Append(E(header)).Append("</th>");
		}

		body.Append("</tr></thead><tbody>");
		foreach (var row in rows)
		{
			body.Append("<tr>");
			foreach (var cell in row)
			{
				body.Append("<td>").Append(E(FirstLine(cell))).Append("</td>");
			}

			body.Append("</tr>");
		}

		body.Append("</tbody></table></div>");
	}

	private static void Fact(StringBuilder body, string label, string? value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			body.Append(CultureInfo.InvariantCulture, $"<tr><th scope=\"row\">{E(label)}</th><td>{E(value)}</td></tr>");
		}
	}

	private static string Bar(JsonElement required)
	{
		var total = Math.Max(1, Number(required, "total"));
		string Segment(string css, int count) => count == 0 ? string.Empty
			: string.Create(CultureInfo.InvariantCulture, $"<span class=\"seg {css}\" style=\"width:{count * 100.0 / total:0.##}%\"></span>");
		return "<span class=\"bar\" aria-hidden=\"true\">" + Segment("passed", Number(required, "passed")) +
			Segment("failed", Number(required, "failed") + Number(required, "not-run")) + Segment("uncovered", Number(required, "not-covered")) + "</span>";
	}

	private static string Driver(IReadOnlyDictionary<string, string> provenance) =>
		provenance.TryGetValue("driverAssembly", out var name) ? name + (provenance.TryGetValue("driverVersion", out var version) ? " " + ShortVersion(version) : string.Empty) : "—";

	private static string Server(IReadOnlyDictionary<string, string> provenance)
	{
		var image = provenance.GetValueOrDefault("image");
		var version = provenance.GetValueOrDefault("serverVersion");
		return image is not null ? image.Split('/').Last() : version is not null ? "embedded, " + version : "—";
	}

	// An informational version may carry a build hash after '+'; the page shows the release.
	private static string ShortVersion(string version) => version.Split('+')[0];

	private static string Footer(JsonElement root)
	{
		var suite = Map(root, "suite");
		return $"<footer><p>Generated {E(Text(root, "generated"))}{(suite.TryGetValue("commit", out var commit) ? $" from suite commit <code>{E(commit[..Math.Min(12, commit.Length)])}</code>" : string.Empty)}. " +
			"Built from the run's <code>matrix.json</code> by <code>conformance site</code>; every configuration in <code>providers/</code> can be run again to reproduce it.</p></footer>";
	}

	private static StringBuilder Page(string title, out StringBuilder body)
	{
		var head = new StringBuilder();
		head.Append("<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
		head.Append("<title>").Append(E(title)).Append("</title><style>").Append(Css).Append("</style></head><body><main>");
		body = new StringBuilder();
		return head;
	}

	private static string Close(StringBuilder head, StringBuilder body) => head.Append(body).Append("</main></body></html>").ToString();

	private static string? Text(JsonElement element, string name) =>
		element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

	private static int Number(JsonElement element, string name) =>
		element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetInt32() : 0;

	private static IReadOnlyDictionary<string, string> Map(JsonElement element, string name) =>
		element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object
			? value.EnumerateObject().Where(p => p.Value.ValueKind == JsonValueKind.String).ToDictionary(p => p.Name, p => p.Value.GetString()!)
			: new Dictionary<string, string>();

	private static string FirstLine(string? text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}

		var line = text.Split('\n')[0].Trim();
		return line.Length > 300 ? line[..300] + "…" : line;
	}

	private static string E(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);

	private const string Css = """
		:root{--bg:#fbfbfa;--fg:#1d1d1f;--muted:#5f6368;--line:#e3e3e0;--card:#ffffff;--link:#1a5fb4;
		--ok:#1e7d3a;--ok-bg:#e3f4e8;--warn:#8a5a00;--warn-bg:#fdf1d6;--bad:#b3261e;--bad-bg:#fbe4e2;--na:#5f6368;--na-bg:#eceeef;
		--seg-pass:#2e9e4f;--seg-fail:#d9473d;--seg-uncovered:#d7d9db;color-scheme:light}
		@media (prefers-color-scheme:dark){:root:not([data-theme="light"]){--bg:#161617;--fg:#ececee;--muted:#a3a7ad;--line:#2e2f31;--card:#1e1f21;--link:#8ab4f8;
		--ok:#7ed69a;--ok-bg:#173623;--warn:#f2c46b;--warn-bg:#3a2d10;--bad:#f2938b;--bad-bg:#3d1916;--na:#a3a7ad;--na-bg:#2a2b2d;
		--seg-pass:#3fb365;--seg-fail:#e0625a;--seg-uncovered:#3a3b3e;color-scheme:dark}}
		:root[data-theme="dark"]{--bg:#161617;--fg:#ececee;--muted:#a3a7ad;--line:#2e2f31;--card:#1e1f21;--link:#8ab4f8;
		--ok:#7ed69a;--ok-bg:#173623;--warn:#f2c46b;--warn-bg:#3a2d10;--bad:#f2938b;--bad-bg:#3d1916;--na:#a3a7ad;--na-bg:#2a2b2d;
		--seg-pass:#3fb365;--seg-fail:#e0625a;--seg-uncovered:#3a3b3e;color-scheme:dark}
		*{box-sizing:border-box}body{margin:0;background:var(--bg);color:var(--fg);font:15px/1.5 system-ui,-apple-system,"Segoe UI",sans-serif}
		main{max-width:1180px;margin:0 auto;padding:32px 16px 48px}a{color:var(--link)}code{font:13px ui-monospace,SFMono-Regular,Menlo,monospace}
		h1{font-size:28px;margin:0 0 8px}h2{font-size:19px;margin:36px 0 8px}h3{font-size:16px;margin:24px 0 8px}
		.lede{color:var(--muted);max-width:760px;margin:0}.note{color:var(--muted);margin:0 0 12px}.back{margin:0 0 16px}
		.chips{display:flex;flex-wrap:wrap;gap:8px;list-style:none;padding:0;margin:20px 0}.chip{padding:6px 12px;border-radius:999px;background:var(--na-bg);color:var(--na)}
		.chips.small .chip{padding:3px 10px;font-size:13px}
		.badge{display:inline-block;padding:2px 10px;border-radius:999px;font-size:13px;font-weight:600;white-space:nowrap}.badge.big{font-size:15px;padding:4px 14px}
		.certified{background:var(--ok-bg);color:var(--ok)}.incomplete{background:var(--warn-bg);color:var(--warn)}.failed{background:var(--bad-bg);color:var(--bad)}.untested{background:var(--na-bg);color:var(--na)}
		.fidelity-exact{background:var(--ok-bg);color:var(--ok)}.fidelity-lossy,.fidelity-wrong-type,.fidelity-throws{background:var(--bad-bg);color:var(--bad)}
		.key dl{display:grid;grid-template-columns:max-content 1fr;gap:8px 16px;margin:0}.key dd{margin:0;color:var(--muted)}
		.scroll{overflow-x:auto;border:1px solid var(--line);border-radius:10px;background:var(--card)}
		table{border-collapse:collapse;width:100%;font-size:14px}th,td{padding:8px 12px;border-bottom:1px solid var(--line);text-align:left;vertical-align:top}
		thead th{font-size:12px;text-transform:uppercase;letter-spacing:.04em;color:var(--muted);white-space:nowrap}tbody tr:last-child td{border-bottom:0}
		.num{text-align:right;font-variant-numeric:tabular-nums}.reason{color:var(--muted)}.detail{color:var(--muted);font-size:13px;word-break:break-word}
		.facts{border:1px solid var(--line);border-radius:10px;background:var(--card)}.facts th{width:180px;color:var(--muted);font-weight:500}
		.bar{display:inline-flex;width:120px;height:8px;border-radius:4px;overflow:hidden;background:var(--seg-uncovered);vertical-align:middle;margin-right:8px}
		.seg{display:block;height:100%}.seg.passed{background:var(--seg-pass)}.seg.failed{background:var(--seg-fail)}.seg.uncovered{background:var(--seg-uncovered)}
		.fraction{font-variant-numeric:tabular-nums;white-space:nowrap}td.required{white-space:nowrap}.summary{font-size:16px}.count{color:var(--muted);font-weight:400}
		.areas{margin:0;padding-left:20px}details summary{cursor:pointer;margin:0 0 12px}
		footer{margin-top:48px;padding-top:16px;border-top:1px solid var(--line);color:var(--muted);font-size:13px}
		@media (max-width:640px){h1{font-size:23px}.key dl{grid-template-columns:1fr}.bar{width:72px}}
		""";
}
