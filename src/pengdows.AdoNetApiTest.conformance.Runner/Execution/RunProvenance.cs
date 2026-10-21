using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>
/// Where a matrix came from: the suite's commit (and whether the working tree had changes), and for each provider the
/// provenance its test process recorded (<see cref="Provenance"/>) plus, for a configured provider, its file's checksum.
/// </summary>
internal static class RunProvenance
{
	/// <summary>The provenance file a provider's test process writes beside its result file.</summary>
	public static string FileFor(string resultFile) => Path.ChangeExtension(resultFile, ".provenance.json");

	public static IReadOnlyDictionary<string, string>? Read(string resultFile, string? configPath)
	{
		var values = new Dictionary<string, string>(StringComparer.Ordinal);
		var path = FileFor(resultFile);
		if (File.Exists(path))
		{
			try
			{
				using var document = JsonDocument.Parse(File.ReadAllText(path));
				if (document.RootElement.ValueKind != JsonValueKind.Object)
				{
					throw new InvalidDataException($"Provenance file {path} must contain a JSON object.");
				}

				foreach (var property in document.RootElement.EnumerateObject())
				{
					if (property.Value.ValueKind == JsonValueKind.String)
					{
						values[property.Name] = property.Value.GetString()!;
					}
				}
			}
			catch (JsonException exception)
			{
				throw new InvalidDataException($"Provenance file {path} is not valid JSON.", exception);
			}
		}

		if (configPath is not null && File.Exists(configPath))
		{
			values["config"] = "providers/" + Path.GetFileName(configPath);
			values["configSha256"] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(configPath)));
		}

		return values.Count == 0 ? null : values;
	}

	/// <summary>The suite's commit and whether the working tree differs from it, or nothing outside a git checkout.</summary>
	public static IReadOnlyDictionary<string, string> Suite(string directory)
	{
		var values = new Dictionary<string, string>(StringComparer.Ordinal);
		if (Git(directory, "rev-parse HEAD") is { Length: > 0 } commit)
		{
			values["commit"] = commit;
			values["dirty"] = (Git(directory, "status --porcelain") is { Length: > 0 }).ToString().ToLowerInvariant();
		}

		return values;
	}

	private static string? Git(string directory, string arguments)
	{
		try
		{
			var start = new ProcessStartInfo("git", arguments)
			{
				WorkingDirectory = directory,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
			};
			using var process = Process.Start(start)!;
			var output = process.StandardOutput.ReadToEnd().Trim();
			process.WaitForExit();
			return process.ExitCode == 0 ? output : null;
		}
		catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or InvalidOperationException)
		{
			return null;
		}
	}
}
