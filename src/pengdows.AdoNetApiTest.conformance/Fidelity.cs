using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>How faithfully a value survived a trip through a provider.</summary>
public enum FidelityStatus
{
	/// <summary>The value, and its CLR type, came back unchanged.</summary>
	Exact,

	/// <summary>The type is right but the value changed: truncated, rounded, padded or shifted.</summary>
	Lossy,

	/// <summary>The value came back as a different CLR type than the one expected.</summary>
	WrongType,

	/// <summary>The provider threw instead of returning the value.</summary>
	Throws,

	/// <summary>The check does not apply to this type (a type the database cannot compare, for example).</summary>
	NotApplicable,
}

/// <summary>The outcome of comparing one value, with the detail that shows the difference.</summary>
public sealed class FidelityResult
{
	public FidelityResult(FidelityStatus status, string detail)
	{
		Status = status;
		Detail = detail;
	}

	public FidelityStatus Status { get; }

	public string Detail { get; }
}

/// <summary>Decides whether the value a provider returned is the value that was stored.</summary>
public static class FidelityComparer
{
	public static FidelityResult Compare(object expected, object actual, bool ignoreCase = false)
	{
		var expectedNull = expected is null || expected is DBNull;
		var actualNull = actual is null || actual is DBNull;
		if (expectedNull && actualNull)
		{
			return Exact();
		}

		if (expectedNull || actualNull)
		{
			return Lossy(expected, actual);
		}

		if (expected.GetType() != actual.GetType())
		{
			return new FidelityResult(FidelityStatus.WrongType,
				$"expected {expected.GetType().Name} {Show(expected)} but was {actual.GetType().Name} {Show(actual)}");
		}

		switch (expected)
		{
			case string text when ignoreCase:
				return string.Equals(text, (string) actual, StringComparison.OrdinalIgnoreCase) ? Exact() : Lossy(expected, actual);
			case byte[] bytes:
				return bytes.SequenceEqual((byte[]) actual) ? Exact() : Lossy(expected, actual);
			case float single:
				return SameBits(BitConverter.GetBytes(single), BitConverter.GetBytes((float) actual)) || (float.IsNaN(single) && float.IsNaN((float) actual))
					? Exact() : Lossy(expected, actual);
			case double number:
				return BitConverter.DoubleToInt64Bits(number) == BitConverter.DoubleToInt64Bits((double) actual) || (double.IsNaN(number) && double.IsNaN((double) actual))
					? Exact() : Lossy(expected, actual);
			case DateTime date:
				return date.Ticks == ((DateTime) actual).Ticks ? Exact() : Lossy(expected, actual);
			case DateTimeOffset offset:
				var other = (DateTimeOffset) actual;
				if (offset.UtcTicks != other.UtcTicks)
				{
					return Lossy(expected, actual);
				}

				return offset.Offset == other.Offset
					? Exact()
					: new FidelityResult(FidelityStatus.Lossy, $"offset changed: expected {Show(expected)} but was {Show(actual)}");
			default:
				return expected.Equals(actual) ? Exact() : Lossy(expected, actual);
		}
	}

	private static FidelityResult Exact() => new FidelityResult(FidelityStatus.Exact, null);

	private static FidelityResult Lossy(object expected, object actual) =>
		new FidelityResult(FidelityStatus.Lossy, $"expected {Show(expected)} but was {Show(actual)}");

	private static bool SameBits(byte[] a, byte[] b) => a.SequenceEqual(b);

	/// <summary>A value as text for a finding, short enough to read and exact enough to see what changed.</summary>
	public static string Show(object value)
	{
		switch (value)
		{
			case null:
			case DBNull _:
				return "NULL";
			case string text:
				return "\"" + Truncate(Escape(text), 80) + "\" (" + text.Length + " chars)";
			case byte[] bytes:
				return "0x" + Truncate(BitConverter.ToString(bytes).Replace("-", string.Empty), 64) + " (" + bytes.Length + " bytes)";
			case DateTime date:
				return date.ToString("O", CultureInfo.InvariantCulture);
			case DateTimeOffset offset:
				return offset.ToString("O", CultureInfo.InvariantCulture);
			case TimeSpan span:
				return span.ToString("c", CultureInfo.InvariantCulture);
			case decimal number:
				return number.ToString(CultureInfo.InvariantCulture);
			case double number:
				return number.ToString("R", CultureInfo.InvariantCulture);
			case float number:
				return number.ToString("R", CultureInfo.InvariantCulture);
			default:
				return Convert.ToString(value, CultureInfo.InvariantCulture);
		}
	}

	private static string Escape(string text)
	{
		var shown = new StringBuilder(text.Length);
		foreach (var c in text)
		{
			if (c < ' ' || c == '\u007f')
			{
				shown.Append("\\u").Append(((int) c).ToString("X4", CultureInfo.InvariantCulture));
			}
			else
			{
				shown.Append(c);
			}
		}

		return shown.ToString();
	}

	private static string Truncate(string text, int length) => text.Length <= length ? text : text.Substring(0, length) + "…";
}
