using pengdows.AdoNetApiTest.conformance.Runner;
using Xunit;

namespace Runner.Tests;

/// <summary>The certification site is built from a matrix alone, with a page per provider.</summary>
public sealed class SiteTests : IDisposable
{
	private readonly string m_directory = Path.Combine(Path.GetTempPath(), "conformance-site-" + Guid.NewGuid().ToString("N"));

	public void Dispose()
	{
		if (Directory.Exists(m_directory))
		{
			Directory.Delete(m_directory, recursive: true);
		}
	}

	[Fact]
	public void The_site_has_an_index_and_a_page_per_provider_with_its_status()
	{
		var certified = new XmlBuilder().Test("Connection_opens", "Pass", "Required", XmlBuilder.Base("Connection", "Required"), contract: "https://learn.microsoft.com/c");
		var failing = new XmlBuilder().Test("Connection_opens", "Fail", "Required", XmlBuilder.Base("Connection", "Required"), contract: "https://learn.microsoft.com/c", failure: "boom\nSystem.InvalidOperationException: stack evidence");
		var matrix = Matrix.Build(new[]
		{
			new ProviderInput("Good", "Good.Tests", XunitXml.Parse(certified.ToString()), null)
			{
				Provenance = new Dictionary<string, string> { ["driverAssembly"] = "Good.Data", ["driverVersion"] = "1.2.3+abc", ["image"] = "postgres:17" },
			},
			new ProviderInput("Bad", "Bad.Tests", XunitXml.Parse(failing.ToString()), null),
			new ProviderInput("Down", "Down.Tests", null, "needs windows"),
		});
		Directory.CreateDirectory(m_directory);
		var json = Path.Combine(m_directory, "matrix.json");
		File.WriteAllText(json, MatrixJson.Write(matrix));

		SiteGenerator.Write(json, Path.Combine(m_directory, "site"));

		var index = File.ReadAllText(Path.Combine(m_directory, "site", "index.html"));
		Assert.Contains("Certified", index);
		Assert.Contains("Not certified", index);
		Assert.Contains("needs windows", index);
		Assert.Contains("Good.Data 1.2.3", index);
		Assert.Contains("postgres:17", index);
		Assert.Contains("Bad.html#required-failures", index);
		var bad = File.ReadAllText(Path.Combine(m_directory, "site", "Bad.html"));
		Assert.Contains("Required failures", bad);
		Assert.Contains("https://learn.microsoft.com/c", bad);
		Assert.Contains("boom", bad);
		Assert.Contains("stack evidence", bad);
		Assert.Contains("<details>", bad);
		Assert.True(File.Exists(Path.Combine(m_directory, "site", "Down.html")));
	}

	[Fact]
	public void Provider_names_become_safe_file_names() => Assert.Equal("a_b.html", SiteGenerator.FileName("a/b"));
}
