using System.Diagnostics;

namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>Runs a provider's test project and reads the xUnit XML it writes.</summary>
internal static class ProviderRunner
{
	public static ProviderInput Run(ProviderProject provider, Options options, string outputDirectory)
	{
		if (provider.NotRunReason is not null)
		{
			return new ProviderInput(provider.Id, provider.Project, null, provider.NotRunReason);
		}

		Directory.CreateDirectory(outputDirectory);
		var xml = Path.GetFullPath(Path.Combine(outputDirectory, provider.Id + ".xml"));
		var log = Path.GetFullPath(Path.Combine(outputDirectory, provider.Id + ".log"));
		File.Delete(xml);

		var start = new ProcessStartInfo("dotnet")
		{
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
		};
		var output = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(provider.Project))!, "bin", options.Configuration, options.Framework);
		foreach (var pair in provider.Environment ?? new Dictionary<string, string>())
		{
			start.Environment[pair.Key] = pair.Value.Replace("${output}", output, StringComparison.Ordinal);
		}

		foreach (var argument in new[] { "run", "--project", provider.Project, "-f", options.Framework, "-c", options.Configuration })
		{
			start.ArgumentList.Add(argument);
		}

		if (options.NoBuild)
		{
			start.ArgumentList.Add("--no-build");
		}

		foreach (var argument in new[] { "--", "-xml", xml })
		{
			start.ArgumentList.Add(argument);
		}

		using var writer = new StreamWriter(log);
		using var process = new Process { StartInfo = start };
		var gate = new object();
		void Write(string? line)
		{
			if (line is not null)
			{
				lock (gate)
				{
					writer.WriteLine(line);
				}
			}
		}

		process.OutputDataReceived += (_, e) => Write(e.Data);
		process.ErrorDataReceived += (_, e) => Write(e.Data);
		try
		{
			process.Start();
		}
		catch (Exception exception)
		{
			return new ProviderInput(provider.Id, provider.Project, null, $"could not start dotnet: {exception.Message}");
		}

		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		if (!process.WaitForExit(options.Timeout))
		{
			process.Kill(entireProcessTree: true);
			process.WaitForExit();
			return new ProviderInput(provider.Id, provider.Project, null, $"timed out after {options.Timeout.TotalMinutes:0.#} minutes; see {log}");
		}

		process.WaitForExit();
		if (!File.Exists(xml))
		{
			return new ProviderInput(provider.Id, provider.Project, null, $"no result file (exit code {process.ExitCode}); see {log}");
		}

		try
		{
			return new ProviderInput(provider.Id, provider.Project, XunitXml.Parse(File.ReadAllText(xml)), null);
		}
		catch (Exception exception) when (exception is System.Xml.XmlException or IOException)
		{
			return new ProviderInput(provider.Id, provider.Project, null, $"unreadable result file: {exception.Message}");
		}
	}
}
