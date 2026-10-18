using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Repository.Tests;

/// <summary>
/// The project is a derivative of mysql-net/AdoNetApiTest (MIT). The upstream notice must stay, the package must not
/// present the upstream project's identity as its own, and the docs must not describe skips (nothing is skipped).
/// </summary>
public class LicenseAndDocsTests
{
	[Fact]
	public void License_keeps_the_upstream_notice_and_names_pengdows()
	{
		var license = File.ReadAllText(Path.Combine(Root(), "LICENSE"));
		Assert.Contains("MIT License", license);
		Assert.Contains("Copyright 2017 Bradley Grainger", license);
		Assert.Contains("Copyright 2025-2026 Pengdows", license);
		Assert.Contains("The above copyright notice and this permission notice shall be included in all", license);
	}

	[Fact]
	public void Package_metadata_names_this_project_and_is_not_signed_with_the_upstream_key()
	{
		var csproj = File.ReadAllText(Path.Combine(Root(), "src", "pengdows.AdoNetApiTest.conformance", "pengdows.AdoNetApiTest.conformance.csproj"));
		Assert.Contains("<Authors>Pengdows;Bradley Grainger</Authors>", csproj);
		Assert.DoesNotContain("<RepositoryUrl>https://github.com/mysql-net", csproj);
		Assert.DoesNotContain("<PackageProjectUrl>https://mysql-net", csproj);
		Assert.DoesNotContain("SignAssembly", csproj);
		Assert.DoesNotContain(".snk", csproj);
		Assert.False(Directory.EnumerateFiles(Root(), "*.snk", SearchOption.AllDirectories).Any(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")), "a strong-name key is committed");
	}

	[Theory]
	[InlineData("README.md")]
	[InlineData("AGENTS.md")]
	[InlineData("src/pengdows.AdoNetApiTest.conformance/README.md")]
	[InlineData("src/pengdows.AdoNetApiTest.conformance/CONTRACT-COVERAGE.md")]
	public void Docs_do_not_describe_skips_or_soft_warnings(string path)
	{
		var text = File.ReadAllText(Path.Combine(Root(), path.Replace('/', Path.DirectorySeparatorChar)));
		Assert.DoesNotContain("Skip =", text);
		Assert.DoesNotContain("[SOFT WARNING]", text);
		Assert.DoesNotContain("SkippableFact", text);
		Assert.DoesNotContain("guinea", text, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void Dependabot_does_not_assign_the_upstream_author()
	{
		Assert.DoesNotContain("bgrainger", File.ReadAllText(Path.Combine(Root(), ".github", "dependabot.yml")));
	}

	private static string Root()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);
		while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
		{
			directory = directory.Parent;
		}

		return directory!.FullName;
	}
}
