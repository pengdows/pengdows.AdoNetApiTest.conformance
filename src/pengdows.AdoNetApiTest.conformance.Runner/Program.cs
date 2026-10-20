namespace pengdows.AdoNetApiTest.conformance.Runner;

internal static class Program
{
	private static int Main(string[] args)
	{
		var options = CommandLine.Parse(args);
		if (options.Error is not null)
		{
			Console.Error.WriteLine(options.Error);
			Console.Error.WriteLine();
			Console.Error.WriteLine(CommandLine.Usage);
			return 2;
		}

		if (options.Command == Command.Help)
		{
			Console.WriteLine(CommandLine.Usage);
			return 0;
		}

		if (options.Command == Command.Site)
		{
			if (!File.Exists(options.Files[0]))
			{
				Console.Error.WriteLine($"No matrix at {options.Files[0]}; run the providers first or name a matrix.json.");
				return 2;
			}

			Console.WriteLine($"Site: {SiteGenerator.Write(options.Files[0], options.SiteDirectory)}");
			return 0;
		}

		try
		{
			var inputs = options.Command == Command.Report ? ReadFiles(options) : RunProviders(options);
			var matrix = Matrix.Build(inputs, RequiredCatalog.Load());
			matrix.Suite = RunProvenance.Suite(Directory.GetCurrentDirectory());
			Write(options.JsonPath, MatrixJson.Write(matrix));
			Write(options.XmlPath, MatrixXml.Write(matrix));
			Console.WriteLine(ConsoleReport.Render(matrix));
			Console.WriteLine($"Matrix: {options.JsonPath}, {options.XmlPath}");
			return ConsoleReport.ExitCode(matrix);
		}
		catch (ArgumentException exception)
		{
			Console.Error.WriteLine(exception.Message);
			return 2;
		}
	}

	private static List<ProviderInput> ReadFiles(Options options) =>
		options.Files.Select(file =>
		{
			var id = CommandLine.ProviderIdFromFile(file);
			try
			{
				return new ProviderInput(id, file, XunitXml.Parse(File.ReadAllText(file)), null)
				{
					Provenance = RunProvenance.Read(file, null),
				};
			}
			catch (Exception exception) when (exception is System.Xml.XmlException or IOException)
			{
				return new ProviderInput(id, file, null, $"unreadable result file: {exception.Message}");
			}
		}).ToList();

	private static List<ProviderInput> RunProviders(Options options)
	{
		var testsDirectory = options.TestsDirectory ?? FindTestsDirectory()
			?? throw new ArgumentException("Could not find the tests folder above the current folder; pass --tests <dir>.");
		var all = ProviderDiscovery.Discover(testsDirectory).Concat(ConfiguredProjects.Discover(testsDirectory)).ToList();
		var duplicate = all.GroupBy(p => p.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
		if (duplicate is not null)
		{
			throw new ArgumentException($"Two providers are named {duplicate.Key}: {string.Join(" and ", duplicate.Select(p => p.Project))}.");
		}

		var providers = ProviderDiscovery.Select(all, options.Providers, options.Excluded);
		var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(options.JsonPath))!;

		var inputs = new List<ProviderInput>();
		foreach (var provider in providers)
		{
			Console.Error.WriteLine($"Running {provider.Id} ...");
			inputs.Add(ProviderRunner.Run(provider, options, outputDirectory));
		}

		return inputs;
	}

	private static string? FindTestsDirectory()
	{
		for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
		{
			var tests = Path.Combine(directory.FullName, "tests");
			if (Directory.Exists(Path.Combine(tests, "Repository.Tests")))
			{
				return tests;
			}
		}

		return null;
	}

	private static void Write(string path, string content)
	{
		var directory = Path.GetDirectoryName(Path.GetFullPath(path));
		if (directory is not null)
		{
			Directory.CreateDirectory(directory);
		}

		File.WriteAllText(path, content);
	}
}
