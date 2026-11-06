using System.Diagnostics;

namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>Runs a provider's test project and reads the xUnit XML it writes.</summary>
internal static class ProviderRunner
{
	/// <summary>
	/// A provider's environment value: <c>${output}</c> is its output directory and <c>${inherit}</c> the runner's own value
	/// of the variable, so a path list can keep what the host needs. A missing value leaves no empty path entry.
	/// </summary>
	internal static string ExpandEnvironment(string value, string output, string? inherited)
	{
		var expanded = value.Replace("${output}", output, StringComparison.Ordinal);
		if (string.IsNullOrEmpty(inherited))
		{
			var separator = Path.PathSeparator.ToString();
			return expanded.Replace(separator + "${inherit}", string.Empty, StringComparison.Ordinal)
				.Replace("${inherit}" + separator, string.Empty, StringComparison.Ordinal)
				.Replace("${inherit}", string.Empty, StringComparison.Ordinal);
		}

		return expanded.Replace("${inherit}", inherited, StringComparison.Ordinal);
	}

	/// <summary>Most crashes one run survives; each one excludes the test that caused it and starts the provider again.</summary>
	private const int MaxCrashes = 25;

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
		File.Delete(RunProvenance.FileFor(xml));
		File.Delete(log);

		// A driver that faults in native code (an access violation, a stack overflow) takes the whole in-process test
		// run with it, and no result file is written. The crashing test is read from the log, excluded, and the run
		// repeated; the test is then reported as a failure of its own, so one crash costs one test, not the provider.
		var crashed = new List<CrashedTest>();
		while (true)
		{
			var attempt = RunOnce(provider, options, xml, log, crashed.Select(c => c.Method).ToList());
			if (attempt.Error is not null)
			{
				return new ProviderInput(provider.Id, provider.Project, null, attempt.Error);
			}

			if (!File.Exists(xml))
			{
				var test = crashed.Count < MaxCrashes ? CrashDiagnosis.FindCrashedTest(log) : null;
				if (test is not null && crashed.All(c => c.Method != test.Method))
				{
					crashed.Add(test);
					continue;
				}

				return new ProviderInput(provider.Id, provider.Project, null, $"no result file (exit code {attempt.ExitCode}); see {log}");
			}

			try
			{
				var results = XunitXml.Parse(File.ReadAllText(xml)).Concat(crashed.Select(c => c.AsResult())).ToList();
				return new ProviderInput(provider.Id, provider.Project, results, null)
				{
					Provenance = RunProvenance.Read(xml, provider.ConfigPath),
				};
			}
			catch (Exception exception) when (exception is System.Xml.XmlException or IOException)
			{
				return new ProviderInput(provider.Id, provider.Project, null, $"unreadable result file: {exception.Message}");
			}
		}
	}

	private static (int ExitCode, string? Error) RunOnce(ProviderProject provider, Options options, string xml, string log, IReadOnlyList<string> excludedMethods)
	{
		var start = new ProcessStartInfo("dotnet")
		{
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
		};
		var output = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(provider.Project))!, "bin", options.Configuration, options.Framework);
		start.Environment[Provenance.Variable] = RunProvenance.FileFor(xml);
		foreach (var pair in provider.Environment ?? new Dictionary<string, string>())
		{
			start.Environment[pair.Key] = ExpandEnvironment(pair.Value, output, Environment.GetEnvironmentVariable(pair.Key));
		}

		foreach (var argument in new[] { "run", "--project", provider.Project, "-f", options.Framework, "-c", options.Configuration })
		{
			start.ArgumentList.Add(argument);
		}

		// Only the first attempt builds; a repeat after a crash runs what that one built.
		if (options.NoBuild || excludedMethods.Count > 0)
		{
			start.ArgumentList.Add("--no-build");
		}

		foreach (var argument in new[] { "--", "-xml", xml })
		{
			start.ArgumentList.Add(argument);
		}

		foreach (var method in excludedMethods)
		{
			start.ArgumentList.Add("-method-");
			start.ArgumentList.Add("*." + method);
		}

		using var writer = new StreamWriter(log, append: true);
		if (excludedMethods.Count > 0)
		{
			writer.WriteLine($"=== run repeated without the crashing test(s): {string.Join(", ", excludedMethods)} ===");
		}

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
			return (-1, $"could not start dotnet: {exception.Message}");
		}

		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		if (!process.WaitForExit(options.Timeout))
		{
			process.Kill(entireProcessTree: true);
			process.WaitForExit();
			return (-1, $"timed out after {options.Timeout.TotalMinutes:0.#} minutes; see {log}");
		}

		process.WaitForExit();
		return (process.ExitCode, null);
	}
}
