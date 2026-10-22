using System;
using System.Data.Common;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Advisory numeric-normalization checks for consumers such as pengdows.crud.
/// These are deliberately not ADO.NET conformance requirements: they describe a
/// lossless conversion policy across providers.
/// </summary>
public abstract class NumericConversionPolicyTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	private const string Policy = "https://github.com/pengdows/pengdows.crud";

	protected NumericConversionPolicyTestBase(TFixture fixture)
		: base(fixture)
	{
	}

	[CommonFact(Contract = Policy, Because = "A lossless numeric policy permits an in-range integral value to narrow or widen.")]
	public void Integral_value_can_narrow_or_widen_without_loss()
	{
		Report(
			ExpectValue("CAST(47 AS BIGINT)", reader => reader.GetFieldValue<byte>(0), (byte)47, "BIGINT -> Byte"),
			ExpectValue("CAST(47 AS BIGINT)", reader => reader.GetInt64(0), 47L, "BIGINT -> Int64"),
			ExpectValue("CAST(47 AS BIGINT)", reader => reader.GetFieldValue<decimal>(0), 47m, "BIGINT -> Decimal"),
			ExpectValue("CAST(47 AS BIGINT)", reader => reader.GetDouble(0), 47d, "BIGINT -> Double"));
	}

	[CommonFact(Contract = Policy, Because = "A numeric value outside the target integral range cannot be narrowed safely.")]
	public void Integral_overflow_is_rejected()
		=> Report(ExpectRejected("CAST(256 AS BIGINT)", reader => reader.GetFieldValue<byte>(0), "BIGINT 256 -> Byte"));

	[CommonFact(Contract = Policy, Because = "A value with no fractional component can be represented by an integral target.")]
	public void Integral_valued_fraction_can_be_read_as_integral()
		=> Report(ExpectValue("CAST(47.0 AS FLOAT)", reader => reader.GetInt32(0), 47, "FLOAT 47.0 -> Int32"));

	[CommonFact(Contract = Policy, Because = "A fractional value must remain fractional; converting it between fractional-capable types is lossless for these values.")]
	public void Fractional_value_can_be_read_as_fractional_type()
	{
		Report(
			ExpectValue("CAST(1.5 AS FLOAT)", reader => reader.GetDouble(0), 1.5d, "FLOAT -> Double"),
			ExpectValue("CAST(1.5 AS FLOAT)", reader => reader.GetFieldValue<decimal>(0), 1.5m, "FLOAT -> Decimal"));
	}

	[CommonFact(Contract = Policy, Because = "Rounding or truncating a fractional value into an integral type changes its value and is not lossless.")]
	public void Fractional_value_is_rejected_by_integral_target()
		=> Report(ExpectRejected("CAST(1.5 AS FLOAT)", reader => reader.GetInt32(0), "FLOAT 1.5 -> Int32"));

	[CommonFact(Contract = Policy, Because = "A fractional decimal value must not be silently rounded or truncated into an integral type.")]
	public void Fractional_decimal_is_rejected_by_integral_target()
		=> Report(ExpectRejected("CAST(47.5 AS DECIMAL(10, 1))", reader => reader.GetFieldValue<byte>(0), "DECIMAL 47.5 -> Byte"));

	private static void Report(params string[] differences)
	{
		var reported = Array.FindAll(differences, difference => !string.IsNullOrEmpty(difference));
		if (reported.Length != 0)
			ConformanceOutcome.Different(string.Join(" ", reported));
	}

	private string ExpectValue<T>(string expression, Func<DbDataReader, T> getter, T expected, string operation)
	{
		try
		{
			using var connection = CreateOpenConnection();
			using var command = connection.CreateCommand();
			command.CommandText = SelectSql(expression + " AS value");
			using var reader = command.ExecuteReader();
			if (!reader.Read())
				return $"Numeric policy probe {operation} returned no row.";

			var actual = getter(reader);
			return Equals(actual, expected)
				? null
				: $"Numeric policy differs for {operation}: expected {expected}, got {actual}.";
		}
		catch (Exception ex)
		{
			return $"Numeric policy differs for {operation}: expected {expected}, got {ex.GetType().Name}: {ex.Message}";
		}
	}

	private string ExpectRejected<T>(string expression, Func<DbDataReader, T> getter, string operation)
	{
		try
		{
			using var connection = CreateOpenConnection();
			using var command = connection.CreateCommand();
			command.CommandText = SelectSql(expression + " AS value");
			using var reader = command.ExecuteReader();
			if (!reader.Read())
				return $"Numeric policy probe {operation} returned no row.";

			var actual = getter(reader);
			return $"Numeric policy differs for {operation}: expected rejection, got {actual}.";
		}
		catch (InvalidCastException)
		{
			return null;
		}
		catch (OverflowException)
		{
			return null;
		}
		catch (Exception ex)
		{
			return $"Numeric policy differs for {operation}: expected InvalidCastException or OverflowException, got {ex.GetType().Name}: {ex.Message}";
		}
	}
}
