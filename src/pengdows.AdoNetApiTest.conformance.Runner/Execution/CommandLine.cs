namespace pengdows.AdoNetApiTest.conformance.Runner;

internal enum Command
{
	Help,
	Run,
	Report,
	Site,
}

internal sealed record Options(
	Command Command,
	IReadOnlyList<string> Providers,
	IReadOnlyList<string> Files,
	string JsonPath,
	string XmlPath,
	string Configuration,
	string Framework,
	bool NoBuild,
	TimeSpan Timeout,
	string? TestsDirectory,
	string? Error,
	string SiteDirectory = "conformance-results/site",
	IReadOnlyList<string>? Excluded = null);

internal static class CommandLine
{
	public const string Usage = """
		conformance: tests ADO.NET providers against the documented System.Data.Common contract.

		  conformance run [provider ...] [options]    run provider test projects, write the matrix, print PASS or FAIL
		  conformance report <result.xml ...>         build the matrix from xUnit -xml files that already exist
		  conformance site [matrix.json] [--out <dir>] build the certification site from a matrix

		A provider passes when every Required test ran and none failed, and no Optional test failed. "Not implemented"
		is a pass; a Required test it has no tests for leaves it INCOMPLETE. Common differences are advisory. The exit
		code is 0 for PASS and 1 for anything else (2 for a usage error).

		Options:
		  --json <file>       matrix as JSON   (default conformance-results/matrix.json)
		  --xml <file>        matrix as XML    (default conformance-results/matrix.xml)
		  --config <name>     build configuration (default Release)
		  --framework <tfm>   target framework (default net10.0)
		  --no-build          run without building first
		  --tests <dir>       the folder holding the provider test projects (default: found above the current folder)
		  --timeout <min>     minutes allowed per provider (default 30)
		  --exclude <names>   providers not to run, comma-separated (those that cannot run on this machine)
		  --out <dir>         where site writes the pages (default conformance-results/site)
		""";

	private static readonly string s_resultsFolder = "conformance-results";

	public static Options Parse(string[] args)
	{
		var options = new Options(Command.Help, Array.Empty<string>(), Array.Empty<string>(),
			Path.Combine(s_resultsFolder, "matrix.json"), Path.Combine(s_resultsFolder, "matrix.xml"),
			"Release", "net10.0", false, TimeSpan.FromMinutes(30), null, null);
		if (args.Length == 0 || args[0] is "--help" or "-h" or "help")
		{
			return options;
		}

		var command = args[0] switch
		{
			"run" => Command.Run,
			"report" => Command.Report,
			"site" => Command.Site,
			_ => (Command?) null,
		};
		if (command is null)
		{
			return options with { Error = $"Unknown command '{args[0]}'." };
		}

		var names = new List<string>();
		options = options with { Command = command.Value };
		for (var i = 1; i < args.Length; i++)
		{
			var arg = args[i];
			if (!arg.StartsWith("--", StringComparison.Ordinal))
			{
				names.Add(arg);
				continue;
			}

			if (arg == "--no-build")
			{
				options = options with { NoBuild = true };
				continue;
			}

			if (i + 1 >= args.Length)
			{
				return options with { Error = $"{arg} needs a value." };
			}

			var value = args[++i];
			switch (arg)
			{
				case "--json": options = options with { JsonPath = value }; break;
				case "--xml": options = options with { XmlPath = value }; break;
				case "--config": options = options with { Configuration = value }; break;
				case "--framework": options = options with { Framework = value }; break;
				case "--tests": options = options with { TestsDirectory = value }; break;
				case "--out": options = options with { SiteDirectory = value }; break;
				case "--exclude": options = options with { Excluded = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) }; break;
				case "--timeout":
					if (!double.TryParse(value, out var minutes) || minutes <= 0)
					{
						return options with { Error = $"--timeout needs a number of minutes, not '{value}'." };
					}

					options = options with { Timeout = TimeSpan.FromMinutes(minutes) };
					break;
				default:
					return options with { Error = $"Unknown option '{arg}'." };
			}
		}

		if (command == Command.Site)
		{
			return names.Count > 1
				? options with { Error = "site takes one matrix file." }
				: options with { Files = names.Count == 1 ? names : new[] { options.JsonPath } };
		}

		if (command == Command.Report)
		{
			return names.Count == 0
				? options with { Error = "report needs at least one xUnit result file." }
				: options with { Files = names };
		}

		return options with { Providers = names };
	}

	/// <summary>The provider a result file belongs to: its file name without the extension.</summary>
	public static string ProviderIdFromFile(string path) => Path.GetFileNameWithoutExtension(path);
}
