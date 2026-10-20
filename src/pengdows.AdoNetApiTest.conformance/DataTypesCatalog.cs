using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>A catalog generated from a provider's <c>DataTypes</c> collection, and the rows it could not use.</summary>
public sealed class GeneratedCatalog
{
	public GeneratedCatalog(IReadOnlyList<TypeSpec> types, IReadOnlyList<UncoveredType> uncovered)
	{
		Types = types;
		Uncovered = uncovered;
	}

	public IReadOnlyList<TypeSpec> Types { get; }

	public IReadOnlyList<UncoveredType> Uncovered { get; }
}

/// <summary>
/// Builds a type catalog from <c>GetSchema("DataTypes")</c>: one <see cref="TypeSpec"/> per row, its column built from
/// <c>CreateFormat</c> and <c>CreateParameters</c> (or <c>TypeName</c> when the provider gives no format), its expected
/// CLR type from <c>DataType</c>, and values chosen for that CLR type. The metadata is taken at its word, so where it is
/// wrong the findings show it: a <c>CreateFormat</c> the database rejects, a <c>DataType</c> that is not the type
/// <c>GetValue</c> returns, an <c>IsFixedLength</c> that does not match the padding.
/// Literals come only where they are portable: numbers in invariant form, and text and binary with the row's
/// <c>LiteralPrefix</c>/<c>LiteralSuffix</c> (SQL-92 single quotes when it has none). Other values take only the
/// parameter round trip. A value the database rejects even as a literal is outside the type's range there, and is not
/// counted against the provider.
/// See https://learn.microsoft.com/dotnet/framework/data/adonet/common-schema-collections#datatypes.
/// </summary>
public static class DataTypesCatalog
{
	/// <summary>The label every generated spec's findings carry, so they are not mistaken for a hand-written catalog's.</summary>
	public const string Label = " (DataTypes)";

	private const int TextLength = 100;
	private const int MaxDecimalPrecision = 28;

	public static GeneratedCatalog Build(DataTable dataTypes, Func<string, Type> resolveType)
	{
		var types = new List<TypeSpec>();
		var uncovered = new List<UncoveredType>();
		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (DataRow row in dataTypes.Rows)
		{
			var typeName = Text(row, "TypeName");
			if (string.IsNullOrEmpty(typeName))
			{
				continue;
			}

			if (Bool(row, "IsConcurrencyType") == true)
			{
				uncovered.Add(new UncoveredType(typeName, "a concurrency type the database generates (IsConcurrencyType)"));
				continue;
			}

			var dataType = Text(row, "DataType");
			if (string.IsNullOrEmpty(dataType))
			{
				uncovered.Add(new UncoveredType(typeName, "DataTypes gives no DataType"));
				continue;
			}

			var clrType = resolveType(dataType);
			if (clrType is null)
			{
				uncovered.Add(new UncoveredType(typeName, $"DataType '{dataType}' is not a loadable CLR type"));
				continue;
			}

			if (!TryColumn(row, typeName, clrType, out var column, out var fallback, out var length, out var precision, out var scale, out var reason))
			{
				uncovered.Add(new UncoveredType(typeName, reason));
				continue;
			}

			if (!seen.Add(column))
			{
				continue;
			}

			var values = Values(row, typeName, clrType, length, precision, scale, HasFractionalSeconds(row));
			if (values is null)
			{
				uncovered.Add(new UncoveredType(typeName, $"no generated values for {clrType}"));
				continue;
			}

			types.Add(new TypeSpec(typeName, column, clrType, DbTypeFor(clrType), values.ToArray())
			{
				Display = column + Label,
				FallbackColumnSql = fallback,
				InferDbType = true,
				Generated = true,
				Comparable = Bool(row, "IsSearchable") != false && Bool(row, "IsLong") != true,
			});
		}

		return new GeneratedCatalog(types, uncovered);
	}

	private static bool TryColumn(DataRow row, string typeName, Type clrType, out string column, out string fallback, out int length,
		out int precision, out int scale, out string reason)
	{
		column = fallback = reason = null;
		length = precision = scale = 0;
		var columnSize = Long(row, "ColumnSize");
		var maximumScale = Long(row, "MaximumScale");
		var format = Text(row, "CreateFormat");
		var parameters = (Text(row, "CreateParameters") ?? string.Empty)
			.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim().ToLowerInvariant()).ToList();

		var args = new List<string>();
		foreach (var parameter in parameters)
		{
			switch (parameter)
			{
				case "precision" when parameters.Contains("scale"):
					precision = (int) Math.Min(columnSize > 0 ? columnSize : maximumScale > 0 ? maximumScale : 18, MaxDecimalPrecision);
					args.Add(precision.ToString(CultureInfo.InvariantCulture));
					break;
				case "scale" when parameters.Contains("precision"):
					scale = Math.Min(10, precision / 2);
					args.Add(scale.ToString(CultureInfo.InvariantCulture));
					break;
				case "scale":
				case "precision of fractional seconds":
				case "secondsprecision":
					// Fractional seconds: as many digits as a CLR DateTime or TimeSpan holds, when the type allows them.
					args.Add(Math.Min(maximumScale > 0 ? maximumScale : 7, 7).ToString(CultureInfo.InvariantCulture));
					break;
				case "dayprecision":
				case "yearprecision":
					args.Add("9");
					break;
				case "length":
				case "size":
				case "max length":
					length = (int) Math.Min(columnSize > 0 ? columnSize : TextLength, TextLength);
					args.Add(length.ToString(CultureInfo.InvariantCulture));
					break;
				case "precision":
				case "number of bits used to store the mantissa":
					if (columnSize <= 0)
					{
						reason = $"CreateParameters '{parameter}' with no ColumnSize to fill it";
						return false;
					}

					args.Add(columnSize.ToString(CultureInfo.InvariantCulture));
					break;
				default:
					reason = $"CreateParameters '{parameter}' is not understood";
					return false;
			}
		}

		if (string.IsNullOrEmpty(format))
		{
			column = args.Count == 0 ? typeName : $"{typeName}({string.Join(",", args)})";
		}
		else
		{
			var placeholders = Enumerable.Range(0, 10).Count(i => format.Contains("{" + i.ToString(CultureInfo.InvariantCulture) + "}"));
			if (placeholders != args.Count)
			{
				reason = $"CreateFormat '{format}' has {placeholders} placeholders but CreateParameters gives {args.Count}";
				return false;
			}

			column = string.Format(CultureInfo.InvariantCulture, format, args.Cast<object>().ToArray());
		}

		// A parameterized type the database rejects is retried by its bare name, so a wrong CreateFormat is a finding
		// rather than the end of the type's checks.
		if (args.Count > 0 && !string.Equals(column, typeName, StringComparison.OrdinalIgnoreCase))
		{
			fallback = typeName;
		}

		return true;
	}

	private static List<TypeValue> Values(DataRow row, string typeName, Type clrType, int length, int precision, int scale, bool fractionalSeconds)
	{
		var prefix = Text(row, "LiteralPrefix");
		var suffix = Text(row, "LiteralSuffix");
		var literals = Bool(row, "IsLiteralSupported") != false;
		var unsigned = Bool(row, "IsUnsigned") == true;
		var fixedLength = Bool(row, "IsFixedLength") == true && length > 0;
		var name = typeName.ToLowerInvariant();
		var values = new List<TypeValue>();

		// Numbers and text always get a literal (SQL-92 quotes when the row gives no prefix): many providers report
		// IsLiteralSupported as false for types whose literals the database accepts.
		void Number(string label, object value) =>
			values.Add(new TypeValue(label, Convert.ToString(value, CultureInfo.InvariantCulture), value));

		if (clrType == typeof(bool))
		{
			values.Add(new TypeValue("true", null, true));
			values.Add(new TypeValue("false", null, false));
		}
		else if (clrType == typeof(byte))
		{
			Number("min", byte.MinValue);
			Number("max", byte.MaxValue);
		}
		else if (clrType == typeof(sbyte))
		{
			Number("min", sbyte.MinValue);
			Number("zero", (sbyte) 0);
			Number("max", sbyte.MaxValue);
		}
		else if (clrType == typeof(short))
		{
			Number("min", short.MinValue);
			Number("zero", (short) 0);
			Number("max", short.MaxValue);
		}
		else if (clrType == typeof(ushort))
		{
			Number("zero", (ushort) 0);
			Number("max", ushort.MaxValue);
		}
		else if (clrType == typeof(int))
		{
			Number("min", int.MinValue);
			Number("zero", 0);
			Number("max", int.MaxValue);
		}
		else if (clrType == typeof(uint))
		{
			Number("zero", 0u);
			Number("max", uint.MaxValue);
		}
		else if (clrType == typeof(long))
		{
			Number("min", long.MinValue);
			Number("zero", 0L);
			Number("max", long.MaxValue);
		}
		else if (clrType == typeof(ulong))
		{
			Number("zero", 0UL);
			Number("max", ulong.MaxValue);
		}
		else if (clrType == typeof(decimal))
		{
			var p = precision > 0 ? precision : 18;
			var s = precision > 0 ? scale : 2;
			var digits = new string('9', p - s) + (s > 0 ? "." + new string('9', s) : string.Empty);
			var max = decimal.Parse(digits, CultureInfo.InvariantCulture);
			Number("zero", 0m);
			Number("max", max);
			if (!unsigned)
			{
				Number("min", -max);
			}

			if (s > 0)
			{
				Number("smallest step", decimal.Parse("0." + new string('0', s - 1) + "1", CultureInfo.InvariantCulture));
			}
		}
		else if (clrType == typeof(double))
		{
			Number("zero", 0d);
			values.Add(new TypeValue("0.1", "0.1", 0.1));
			values.Add(new TypeValue("1e200", "1E+200", 1e200));
			values.Add(new TypeValue("minus 1e200", "-1E+200", -1e200));
			values.Add(new TypeValue("max", "1.7976931348623157E+308", double.MaxValue));
		}
		else if (clrType == typeof(float))
		{
			Number("zero", 0f);
			values.Add(new TypeValue("1.5", "1.5", 1.5f));
			values.Add(new TypeValue("large", "3.4E+38", 3.4E+38f));
		}
		else if (clrType == typeof(string))
		{
			void Str(string label, string value)
			{
				var expected = fixedLength && value.Length < length ? value.PadRight(length) : value;
				values.Add(new TypeValue(label, StringLiteral(value, prefix, suffix), expected).Binding(value));
			}

			Str("empty", string.Empty);
			Str("ascii", "abc");
			Str("quote", "it's");
			// A fixed-length type may count bytes rather than characters (Oracle's CHAR does), so its padding of a
			// non-BMP character is not predictable.
			if (!fixedLength)
			{
				Str("non-BMP", "\U0001F600");
			}

			if (length > 0)
			{
				Str("full length", new string('x', length));
			}
		}
		else if (clrType == typeof(char))
		{
			values.Add(new TypeValue("letter", StringLiteral("a", prefix, suffix), 'a'));
		}
		else if (clrType == typeof(byte[]))
		{
			void Bin(string label, byte[] value)
			{
				var expected = fixedLength && value.Length < length ? value.Concat(new byte[length - value.Length]).ToArray() : value;
				var literal = literals && !string.IsNullOrEmpty(prefix) ? prefix + Hex(value) + suffix : null;
				values.Add(new TypeValue(label, literal, expected).Binding(value));
			}

			Bin("bytes", new byte[] { 0x00, 0xFF, 0x10 });
			if (length > 0)
			{
				Bin("full length", Enumerable.Range(0, length).Select(i => (byte) i).ToArray());
			}
		}
		else if (clrType == typeof(Guid))
		{
			values.Add(new TypeValue("empty", null, Guid.Empty));
			values.Add(new TypeValue("mixed bytes", null, new Guid("33221100-5544-7766-9988-aabbccddeeff")));
		}
		else if (clrType == typeof(DateTime))
		{
			values.Add(new TypeValue("leap day", null, new DateTime(2024, 2, 29)));
			if (!name.Contains("date") || name.Contains("time"))
			{
				values.Add(new TypeValue("whole seconds", null, new DateTime(2024, 2, 29, 12, 34, 56)));
				if (fractionalSeconds)
				{
					values.Add(new TypeValue("milliseconds", null, new DateTime(2024, 2, 29, 12, 34, 56, 123)));
				}
			}
		}
		else if (clrType == typeof(DateTimeOffset))
		{
			values.Add(new TypeValue("utc", null, new DateTimeOffset(2024, 2, 29, 12, 34, 56, TimeSpan.Zero)));
			values.Add(new TypeValue("plus five hours", null, new DateTimeOffset(2024, 2, 29, 12, 34, 56, TimeSpan.FromHours(5))));
		}
		else if (clrType == typeof(TimeSpan))
		{
			values.Add(new TypeValue("time of day", null, new TimeSpan(13, 45, 30)));
			if (fractionalSeconds || name.Contains("interval"))
			{
				values.Add(new TypeValue("milliseconds", null, new TimeSpan(0, 13, 45, 30, 123)));
			}
			if (name.Contains("interval"))
			{
				values.Add(new TypeValue("days", null, new TimeSpan(1, 2, 3, 4)));
				values.Add(new TypeValue("negative", null, TimeSpan.FromHours(-2)));
			}
		}
		else
		{
			return null;
		}

		values.Add(new TypeValue("null", "NULL", null));
		return values;
	}

	/// <summary>
	/// True when the type takes a fractional-seconds parameter. Without one, the database's own precision decides what a
	/// fraction becomes (MySQL's DATETIME keeps whole seconds, SQL Server's datetime 3.33 ms steps), so only whole
	/// seconds are checked.
	/// </summary>
	private static bool HasFractionalSeconds(DataRow row)
	{
		var parameters = (Text(row, "CreateParameters") ?? string.Empty).ToLowerInvariant();
		return parameters.Contains("fractional") || parameters.Contains("secondsprecision") || parameters.Trim() == "scale";
	}

	/// <summary>The DbType a NULL of <paramref name="clrType"/> is bound with; a non-NULL value's is inferred by the provider.</summary>
	private static DbType DbTypeFor(Type clrType) =>
		clrType == typeof(bool) ? DbType.Boolean
		: clrType == typeof(byte) ? DbType.Byte
		: clrType == typeof(sbyte) ? DbType.SByte
		: clrType == typeof(short) ? DbType.Int16
		: clrType == typeof(ushort) ? DbType.UInt16
		: clrType == typeof(int) ? DbType.Int32
		: clrType == typeof(uint) ? DbType.UInt32
		: clrType == typeof(long) ? DbType.Int64
		: clrType == typeof(ulong) ? DbType.UInt64
		: clrType == typeof(decimal) ? DbType.Decimal
		: clrType == typeof(double) ? DbType.Double
		: clrType == typeof(float) ? DbType.Single
		: clrType == typeof(byte[]) ? DbType.Binary
		: clrType == typeof(Guid) ? DbType.Guid
		: clrType == typeof(DateTime) ? DbType.DateTime
		: clrType == typeof(DateTimeOffset) ? DbType.DateTimeOffset
		: clrType == typeof(TimeSpan) ? DbType.Time
		: DbType.String;

	private static string StringLiteral(string value, string prefix, string suffix)
	{
		if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix))
		{
			prefix = suffix = "'";
		}

		// The closing quote is doubled inside the value, as SQL-92 escapes a quote in a character literal.
		var quote = string.IsNullOrEmpty(suffix) ? null : suffix.Substring(0, 1);
		var escaped = quote is null ? value : value.Replace(quote, quote + quote);
		return prefix + escaped + suffix;
	}

	private static string Hex(byte[] bytes)
	{
		var text = new StringBuilder(bytes.Length * 2);
		foreach (var b in bytes)
		{
			text.Append(b.ToString("X2", CultureInfo.InvariantCulture));
		}

		return text.ToString();
	}

	private static string Text(DataRow row, string column) =>
		row.Table.Columns.Contains(column) && row[column] is string text ? text : null;

	private static bool? Bool(DataRow row, string column) =>
		row.Table.Columns.Contains(column) && row[column] is not DBNull and not null ? Convert.ToBoolean(row[column], CultureInfo.InvariantCulture) : null;

	private static long Long(DataRow row, string column)
	{
		if (!row.Table.Columns.Contains(column) || row[column] is DBNull or null)
		{
			return 0;
		}

		try
		{
			return Convert.ToInt64(row[column], CultureInfo.InvariantCulture);
		}
		catch (OverflowException)
		{
			return long.MaxValue;
		}
	}
}
