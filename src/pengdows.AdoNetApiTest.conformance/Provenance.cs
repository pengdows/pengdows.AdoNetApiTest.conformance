using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// What a run actually tested, written once per test process to the file <see cref="Variable"/> names (the runner sets
/// it): the driver assembly and version that loaded, the server's own version, the container image when a fixture
/// started one, and the runtime. A certification is only worth something if it can be reproduced, and these say what to
/// reproduce; they come from the running process, not from what a project file asked for.
/// </summary>
public static class Provenance
{
	public const string Variable = "CONFORMANCE_PROVENANCE";

	private static readonly object s_lock = new();
	private static List<KeyValuePair<string, string>> s_values;

	/// <summary>The container image the server runs from; set by the server fixture.</summary>
	public static string Image { get; set; }

	/// <summary>Records the provenance on the first open connection of the process; later calls do nothing.</summary>
	public static void Record(DbProviderFactory factory, DbConnection connection)
	{
		if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Variable)))
		{
			return;
		}

		lock (s_lock)
		{
			if (s_values is not null)
			{
				return;
			}

			var assembly = factory.GetType().Assembly;
			string serverVersion = null;
			try
			{
				serverVersion = connection.ServerVersion;
			}
			catch (Exception)
			{
			}

			s_values = new List<KeyValuePair<string, string>>
			{
				new("driverAssembly", assembly.GetName().Name),
				new("driverVersion", assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? assembly.GetName().Version?.ToString()),
				new("factory", factory.GetType().FullName),
				new("serverVersion", serverVersion),
				new("image", Image),
				new("runtime", RuntimeInformation.FrameworkDescription),
				new("os", RuntimeInformation.OSDescription),
			};
			Write();
		}
	}

	/// <summary>
	/// Records how the tests wrote a parameter in SQL (<c>@p</c>, <c>:p</c>, <c>?</c>) and where that came from:
	/// <c>fixture</c>, <c>metadata</c> (DataSourceInformation), or <c>probe</c> (a query tried <c>:p</c>, then <c>@p</c>).
	/// </summary>
	public static void RecordParameterMarker(string marker, string source)
	{
		lock (s_lock)
		{
			if (s_values is null || s_values.Any(v => v.Key == "parameterMarker"))
			{
				return;
			}

			s_values.Add(new("parameterMarker", marker));
			s_values.Add(new("parameterMarkerSource", source));
			Write();
		}
	}

	private static void Write()
	{
		try
		{
			File.WriteAllText(Environment.GetEnvironmentVariable(Variable), "{" + string.Join(",", s_values.Where(v => v.Value is not null).Select(v => Quote(v.Key) + ":" + Quote(v.Value))) + "}");
		}
		catch (IOException)
		{
		}
	}

	private static string Quote(string text)
	{
		var quoted = new StringBuilder("\"");
		foreach (var c in text)
		{
			switch (c)
			{
				case '"': quoted.Append("\\\""); break;
				case '\\': quoted.Append("\\\\"); break;
				default:
					if (c < ' ')
					{
						quoted.Append("\\u").Append(((int) c).ToString("x4"));
					}
					else
					{
						quoted.Append(c);
					}

					break;
			}
		}

		return quoted.Append('"').ToString();
	}
}
