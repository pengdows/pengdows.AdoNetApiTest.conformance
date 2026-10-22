using pengdows.AdoNetApiTest.conformance.Runner;
using System;
using System.IO;
using Xunit;

namespace Runner.Tests;

public sealed class ProviderEnvironmentTests
{
	[Fact]
	public void Output_is_replaced_by_the_provider_output_directory()
	{
		Assert.Equal("/out/clidriver/lib", ProviderRunner.ExpandEnvironment("${output}/clidriver/lib", "/out", null));
	}

	// A host that needs extra libraries (Db2's libxml2.so.2 compatibility copies) keeps them on the path.
	[Fact]
	public void Inherit_is_replaced_by_the_runner_own_value()
	{
		var separator = Path.PathSeparator;
		Assert.Equal($"/out/lib{separator}/host/lib", ProviderRunner.ExpandEnvironment($"${{output}}/lib{separator}${{inherit}}", "/out", "/host/lib"));
	}

	// An empty entry in a path list means the current directory, so a missing value leaves no separator behind.
	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public void Inherit_without_a_value_leaves_no_empty_entry(string? inherited)
	{
		var separator = Path.PathSeparator;
		Assert.Equal("/out/lib", ProviderRunner.ExpandEnvironment($"${{output}}/lib{separator}${{inherit}}", "/out", inherited));
		Assert.Equal("/out/lib", ProviderRunner.ExpandEnvironment($"${{inherit}}{separator}${{output}}/lib", "/out", inherited));
	}
}
