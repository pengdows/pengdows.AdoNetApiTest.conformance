using System;
using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace Repository.Tests;

public class FidelityComparerTests
{
	private static FidelityStatus Status(object expected, object actual) => FidelityComparer.Compare(expected, actual).Status;

	[Fact]
	public void Equal_values_of_the_same_type_are_exact()
	{
		Assert.Equal(FidelityStatus.Exact, Status(5, 5));
		Assert.Equal(FidelityStatus.Exact, Status("héllo", "héllo"));
		Assert.Equal(FidelityStatus.Exact, Status(true, true));
		Assert.Equal(FidelityStatus.Exact, Status(Guid.Empty, Guid.Empty));
		Assert.Equal(FidelityStatus.Exact, Status(new byte[] { 1, 2 }, new byte[] { 1, 2 }));
		Assert.Equal(FidelityStatus.Exact, Status(new byte[0], new byte[0]));
	}

	[Fact]
	public void A_different_value_of_the_same_type_is_lossy_and_the_detail_shows_both()
	{
		var result = FidelityComparer.Compare(1.1m, 1.2m);

		Assert.Equal(FidelityStatus.Lossy, result.Status);
		Assert.Contains("1.1", result.Detail);
		Assert.Contains("1.2", result.Detail);
		Assert.Equal(FidelityStatus.Lossy, Status("abc", "abc "));
		Assert.Equal(FidelityStatus.Lossy, Status(new byte[] { 1, 2 }, new byte[] { 1 }));
	}

	[Fact]
	public void A_different_clr_type_is_wrong_type_even_when_the_number_is_the_same()
	{
		var result = FidelityComparer.Compare(5L, 5);

		Assert.Equal(FidelityStatus.WrongType, result.Status);
		Assert.Contains("Int64", result.Detail);
		Assert.Contains("Int32", result.Detail);
	}

	[Fact]
	public void Decimals_compare_by_value_not_by_scale()
	{
		Assert.Equal(FidelityStatus.Exact, Status(1.5m, 1.5000m));
		Assert.Equal(FidelityStatus.Lossy, Status(1.5m, 1.4999m));
	}

	[Fact]
	public void Floating_point_compares_bit_for_bit_so_a_rounded_value_is_lossy()
	{
		Assert.Equal(FidelityStatus.Exact, Status(0.1, 0.1));
		Assert.Equal(FidelityStatus.Exact, Status(double.NaN, double.NaN));
		Assert.Equal(FidelityStatus.Lossy, Status(0.1, 0.1000000000000001));
		Assert.Equal(FidelityStatus.Lossy, Status(0.1f, 0.10000001f));
		Assert.Equal(FidelityStatus.Lossy, Status(0.0, -0.0));
	}

	[Fact]
	public void Dates_compare_by_ticks_and_ignore_kind()
	{
		var local = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Local).AddTicks(1234567);
		var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);

		Assert.Equal(FidelityStatus.Exact, Status(local, unspecified));
		Assert.Equal(FidelityStatus.Lossy, Status(local, local.AddTicks(1)));
	}

	[Fact]
	public void A_date_time_offset_must_keep_both_the_instant_and_the_offset()
	{
		var original = new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.FromHours(5.5));

		Assert.Equal(FidelityStatus.Exact, Status(original, original));
		var sameInstantOtherOffset = original.ToOffset(TimeSpan.Zero);
		var result = FidelityComparer.Compare(original, sameInstantOtherOffset);
		Assert.Equal(FidelityStatus.Lossy, result.Status);
		Assert.Contains("offset", result.Detail);
	}

	[Fact]
	public void Null_matches_dbnull_and_nothing_else()
	{
		Assert.Equal(FidelityStatus.Exact, Status(null, DBNull.Value));
		Assert.Equal(FidelityStatus.Exact, Status(DBNull.Value, DBNull.Value));
		Assert.Equal(FidelityStatus.Exact, Status(null, null));
		Assert.Equal(FidelityStatus.Lossy, Status(null, 0));
		Assert.Equal(FidelityStatus.Lossy, Status(0, DBNull.Value));
	}

	[Fact]
	public void Time_spans_compare_by_ticks()
	{
		Assert.Equal(FidelityStatus.Exact, Status(TimeSpan.FromTicks(863999999999), TimeSpan.FromTicks(863999999999)));
		Assert.Equal(FidelityStatus.Lossy, Status(TimeSpan.FromTicks(863999999999), TimeSpan.FromTicks(863999999990)));
	}
}
