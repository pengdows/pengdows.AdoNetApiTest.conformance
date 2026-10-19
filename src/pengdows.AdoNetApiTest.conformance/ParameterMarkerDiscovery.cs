using System;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// A provider that cannot advertise its parameter marker through <c>GetSchema("DataSourceInformation")</c> states it by
/// implementing this on its fixture. The returned text is the parameter as it appears in SQL, for example <c>@p</c>,
/// <c>$p</c> or <c>?</c> (a positional provider ignores the name).
/// </summary>
public interface IParameterMarkerFixture
{
	string ParameterMarker(string name);
}

/// <summary>How a provider writes a parameter in SQL.</summary>
internal sealed class ParameterMarker
{
	public ParameterMarker(bool isPositional, string prefix)
	{
		IsPositional = isPositional;
		Prefix = prefix;
	}

	public bool IsPositional { get; }

	public string Prefix { get; }
}

/// <summary>
/// Reads the marker from <c>DataSourceInformation</c>. <c>ParameterMarkerPattern</c> is a regular expression, so a "?"
/// inside it (such as the lookahead "(?=") says nothing about positional parameters; only the pattern's leading
/// literal does. Nothing usable gives <see langword="null"/>: the marker is never guessed.
/// </summary>
internal static class ParameterMarkerDiscovery
{
	private const string Placeholder = "{0}";

	public static ParameterMarker Discover(string format, string pattern)
	{
		if (string.IsNullOrEmpty(format))
		{
			return null;
		}

		if (format == "?")
		{
			return new ParameterMarker(true, null);
		}

		var index = format.IndexOf(Placeholder, StringComparison.Ordinal);
		if (index < 0)
		{
			return null;
		}

		if (index > 0)
		{
			return new ParameterMarker(false, format.Substring(0, index));
		}

		var literal = LeadingLiteral(pattern);
		return literal switch
		{
			'?' => new ParameterMarker(true, null),
			'@' or ':' or '$' => new ParameterMarker(false, literal.Value.ToString()),
			_ => null,
		};
	}

	private static char? LeadingLiteral(string pattern)
	{
		if (string.IsNullOrEmpty(pattern))
		{
			return null;
		}

		var i = 0;
		while (i < pattern.Length && (pattern[i] == '^' || pattern[i] == '('))
		{
			if (pattern[i] == '(' && i + 2 < pattern.Length && pattern[i + 1] == '?' && pattern[i + 2] == ':')
			{
				i += 2;
			}

			i++;
		}

		if (i < pattern.Length && pattern[i] == '\\')
		{
			i++;
		}

		return i < pattern.Length ? pattern[i] : null;
	}
}
