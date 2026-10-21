using pengdows.AdoNetApiTest.conformance.Runner;
using Xunit;

namespace Runner.Tests;

public class CertificationTests
{
	private static ProviderInput Provider(string id, XmlBuilder xml) => new(id, id + ".Tests", XunitXml.Parse(xml.ToString()), null);

	private static CertificationProfile Profile(params ProfileTest[] tests) =>
		new("consumer", "Consumer", "certifies as Consumer compliant", "Consumer 1.0", "https://example.org/consumer", "by hand", tests, []);

	private static ProfileTest Entry(string test, string? ifNotImplemented = null) =>
		new(test, "DbCommand.CommandText", ["https://github.com/example/consumer/blob/main/A.cs#L1"], null, ifNotImplemented);

	private static Certification Certify(XmlBuilder xml, CertificationProfile profile) =>
		Matrix.Build(new[] { Provider("P", xml) }, profiles: [profile]).Providers.Single().Certifications.Single(c => c.Id == "consumer");

	[Fact]
	public void A_provider_certifies_when_every_profile_test_passes_or_does_not_apply()
	{
		var xml = new XmlBuilder()
			.Test("A", "Pass", "Required", XmlBuilder.Base("Command", "Required"))
			.Test("B", "Pass", "Common", XmlBuilder.Base("Command", "Common"), outcome: "not-applicable");

		var certification = Certify(xml, Profile(Entry("Command.A"), Entry("Command.B")));

		Assert.True(certification.Certified);
		Assert.Equal(2, certification.Checked);
		Assert.Equal("P certifies as Consumer compliant", certification.Sentence("P"));
	}

	[Fact]
	public void A_failure_outside_the_profile_does_not_stop_it()
	{
		var xml = new XmlBuilder()
			.Test("A", "Pass", "Required", XmlBuilder.Base("Command", "Required"))
			.Test("Unused", "Fail", "Required", XmlBuilder.Base("Command", "Required"), failure: "boom");

		var provider = Matrix.Build(new[] { Provider("P", xml) }, profiles: [Profile(Entry("Command.A"))]).Providers.Single();

		Assert.False(provider.Certifications.Single(c => c.Id == Certifier.FullContractId).Certified);
		Assert.True(provider.Certifications.Single(c => c.Id == "consumer").Certified);
	}

	[Fact]
	public void A_Common_difference_in_the_profile_stops_it()
	{
		var xml = new XmlBuilder().Test("B", "Fail", "Common", XmlBuilder.Base("Command", "Common"), failure: "differs");

		var certification = Certify(xml, Profile(Entry("Command.B")));

		Assert.False(certification.Certified);
		Assert.Equal(Outcome.Different, certification.Failures.Single().Outcome);
		Assert.Equal("P does not certify as Consumer compliant", certification.Sentence("P"));
	}

	[Fact]
	public void Not_implemented_is_accepted_only_where_the_consumer_copes()
	{
		var xml = new XmlBuilder()
			.Test("Copes", "Pass", "Optional", XmlBuilder.Base("Command", "Optional"), outcome: "not-implemented")
			.Test("Needs", "Pass", "Optional", XmlBuilder.Base("Command", "Optional"), outcome: "not-implemented");

		var certification = Certify(xml, Profile(Entry("Command.Copes", ifNotImplemented: "falls back"), Entry("Command.Needs")));

		Assert.False(certification.Certified);
		Assert.Equal("Needs", certification.Failures.Single().Test);
	}

	[Fact]
	public void A_profile_test_with_no_result_stops_it()
	{
		var xml = new XmlBuilder().Test("A", "Pass", "Required", XmlBuilder.Base("Command", "Required"));

		var certification = Certify(xml, Profile(Entry("Command.A"), Entry("DataReader.Missing")));

		Assert.False(certification.Certified);
		Assert.Equal(Outcome.NotRun, certification.Failures.Single().Outcome);
	}

	[Fact]
	public void The_full_contract_follows_the_verdict()
	{
		var xml = new XmlBuilder().Test("A", "Pass", "Required", XmlBuilder.Base("Command", "Required"));

		var provider = Matrix.Build(new[] { Provider("P", xml) }).Providers.Single();
		var full = provider.Certifications.Single();

		Assert.Equal(Verdict.Pass, provider.Verdict);
		Assert.True(full.Certified);
		Assert.Equal("P certifies as fully compliant against the entire ADO.NET DbProvider contract", full.Sentence("P"));
	}

	[Fact]
	public void Mode_safety_diagnostics_never_affect_full_contract_certification()
	{
		var xml = new XmlBuilder()
			.Test("A", "Pass", "Required", XmlBuilder.Base("Command", "Required"))
			.Test("FileBackedMode", "Fail", "ModeSafety", XmlBuilder.Base("ModeSafety", "ModeSafety"), failure: "Standard mode is unsafe");

		var provider = Matrix.Build(new[] { Provider("P", xml) }).Providers.Single();
		var full = provider.Certifications.Single(c => c.Id == Certifier.FullContractId);

		Assert.True(full.Certified);
		Assert.Equal(1, full.Checked);
		Assert.Empty(full.Failures);
	}

	[Fact]
	public void The_console_says_whether_each_provider_certifies()
	{
		var xml = new XmlBuilder()
			.Test("A", "Pass", "Required", XmlBuilder.Base("Command", "Required"))
			.Test("B", "Fail", "Common", XmlBuilder.Base("Command", "Common"), failure: "differs");
		var matrix = Matrix.Build(new[] { Provider("P", xml) }, profiles: [Profile(Entry("Command.A"), Entry("Command.B"))]);

		var text = ConsoleReport.Render(matrix);

		Assert.Contains("P certifies as fully compliant against the entire ADO.NET DbProvider contract", text);
		Assert.Contains("P does not certify as Consumer compliant: 1 of 2 tests (Command.B different)", text);
	}

	private static CertificationProfile WithWorkarounds(params ProfileWorkaround[] workarounds) =>
		Profile(Entry("Command.A")) with { Workarounds = workarounds };

	[Fact]
	public void A_workaround_is_needed_by_the_providers_that_fail_its_test()
	{
		var fails = new XmlBuilder().Test("A", "Fail", "Common", XmlBuilder.Base("Command", "Common"), failure: "throws");
		var passes = new XmlBuilder().Test("A", "Pass", "Common", XmlBuilder.Base("Command", "Common"));
		var profile = WithWorkarounds(
			new ProfileWorkaround("dispose", "catches a throwing Dispose", ["https://github.com/x/y/blob/z/A.cs#L1"], ["Command.A"], []),
			new ProfileWorkaround("untested", "no test yet", ["https://github.com/x/y/blob/z/B.cs#L2"], [], []));

		var matrix = Matrix.Build(new[] { Provider("Broken", fails), Provider("Fixed", passes) }, profiles: [profile]);

		var statuses = matrix.Workarounds["consumer"];
		Assert.Equal(["Broken"], statuses.Single(s => s.Workaround.Id == "dispose").NeededBy);
		Assert.False(statuses.Single(s => s.Workaround.Id == "untested").Workaround.Tested);
		Assert.False(matrix.Providers.Single(p => p.Id == "Broken").Certifications.Single(c => c.Id == "consumer").Certified);
		var text = ConsoleReport.Render(matrix);
		Assert.Contains("dispose: still needed by Broken", text);
		Assert.Contains("untested: no test covers it yet", text);
	}

	[Fact]
	public void A_fidelity_finding_a_workaround_covers_stops_certification()
	{
		var xml = new XmlBuilder()
			.Test("A", "Pass", "Required", XmlBuilder.Base("Command", "Required"))
			.Test("Types", "Pass", "Common", XmlBuilder.Base("TypeFidelity", "Common"),
				findings: [("timestamp", "now", "read", "Throws", "GetValue threw"), ("integer", "max", "read", "Throws", "overflow")]);
		var profile = WithWorkarounds(new ProfileWorkaround("timestamp-reads", "falls back for timestamps", ["https://github.com/x/y/blob/z/A.cs#L1"], [],
			[new FidelityCriterion("timestamp", ["throws", "wrong-type"], ["read"])]));

		var matrix = Matrix.Build(new[] { Provider("P", xml) }, profiles: [profile]);

		var certification = matrix.Providers.Single().Certifications.Single(c => c.Id == "consumer");
		Assert.False(certification.Certified);
		Assert.Equal("timestamp now (read)", certification.Failures.Single().Test);
		Assert.Equal(["P"], matrix.Workarounds["consumer"].Single().NeededBy);
	}

	[Fact]
	public void The_shipped_profiles_load_with_their_evidence()
	{
		var profiles = CertificationProfile.Load();

		Assert.Contains(profiles, p => p.Id == "pengdows.crud" && p.Statement == "certifies as pengdows.crud compliant");
		Assert.Contains(profiles, p => p.Id == "efcore" && p.Statement == "certifies as EF Core compliant");
		Assert.All(profiles.SelectMany(p => p.Tests), t => Assert.NotEmpty(t.Sites));
	}

	[Fact]
	public void The_matrix_carries_the_certifications_and_the_profiles()
	{
		var xml = new XmlBuilder().Test("A", "Pass", "Required", XmlBuilder.Base("Command", "Required"));
		var matrix = Matrix.Build(new[] { Provider("P", xml) }, profiles: [Profile(Entry("Command.A"))]);

		using var json = System.Text.Json.JsonDocument.Parse(MatrixJson.Write(matrix));
		var certifications = json.RootElement.GetProperty("providers")[0].GetProperty("certifications");
		Assert.Equal(2, certifications.GetArrayLength());
		Assert.True(certifications[1].GetProperty("certified").GetBoolean());
		Assert.Equal("consumer", json.RootElement.GetProperty("certification-profiles")[1].GetProperty("id").GetString());
		Assert.Contains("certified=\"true\"", MatrixXml.Write(matrix));
	}
}
