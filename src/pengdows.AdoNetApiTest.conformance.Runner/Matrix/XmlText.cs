using System.Globalization;
using System.Text;
using System.Xml;

namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>
/// A provider's messages and a test's raw output can hold characters XML 1.0 cannot represent (a failure that quotes a
/// value of 0x01, U+FFFF, a lone surrogate). xUnit writes them into its result file anyway, and no XML reader or writer
/// accepts them. They are written as a visible <c>\uXXXX</c>, so a file stays well-formed and the message stays readable.
/// </summary>
internal static class XmlText
{
	public static string Escape(string? text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}

		StringBuilder? escaped = null;
		for (var i = 0; i < text.Length; i++)
		{
			var c = text[i];
			var valid = XmlConvert.IsXmlChar(c) && !char.IsSurrogate(c);
			var pair = char.IsHighSurrogate(c) && i + 1 < text.Length && XmlConvert.IsXmlSurrogatePair(text[i + 1], c);
			if (valid || pair)
			{
				escaped?.Append(c);
				if (pair)
				{
					escaped?.Append(text[i + 1]);
					i++;
				}

				continue;
			}

			escaped ??= new StringBuilder(text.Length + 8).Append(text, 0, i);
			escaped.Append("\\u").Append(((int) c).ToString("X4", CultureInfo.InvariantCulture));
		}

		return escaped?.ToString() ?? text;
	}
}
