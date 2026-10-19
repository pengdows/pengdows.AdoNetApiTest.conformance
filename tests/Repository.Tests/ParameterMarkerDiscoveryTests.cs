using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace Repository.Tests;

/// <summary>
/// ParameterMarkerPattern is a regular expression. A "?" inside it (a lookahead such as "(?=") says nothing about
/// positional parameters; only a leading literal does.
/// </summary>
public class ParameterMarkerDiscoveryTests
{
	// What SQL Server, System.Data.SQLite and Npgsql really advertise.
	private const string RealPattern = @"@[\p{Lo}\p{Lu}\p{Ll}\p{Lm}_@#][\p{Lo}\p{Lu}\p{Ll}\p{Lm}\p{Nd}＿_@#\$]*(?=\s+|$)";

	[Fact]
	public void A_regex_lookahead_is_not_a_positional_marker()
	{
		var marker = ParameterMarkerDiscovery.Discover("{0}", RealPattern);

		Assert.NotNull(marker);
		Assert.False(marker.IsPositional);
		Assert.Equal("@", marker.Prefix);
	}

	[Theory]
	[InlineData("?", null)]
	[InlineData("?", "?")]
	[InlineData("{0}", "?")]
	[InlineData("{0}", @"\?")]
	public void An_advertised_question_mark_is_positional(string format, string pattern)
	{
		var marker = ParameterMarkerDiscovery.Discover(format, pattern);

		Assert.NotNull(marker);
		Assert.True(marker.IsPositional);
	}

	[Theory]
	[InlineData("@{0}", null, "@")]
	[InlineData(":{0}", "", ":")]
	[InlineData("${0}", null, "$")]
	[InlineData("{0}", ":[a-zA-Z_]\\w*", ":")]
	[InlineData("{0}", @"\$[a-z]+", "$")]
	[InlineData("{0}", "^@\\w+", "@")]
	[InlineData("{0}", "(@\\w+)(?=\\s)", "@")]
	public void The_prefix_comes_from_the_format_or_the_patterns_leading_literal(string format, string pattern, string prefix)
	{
		var marker = ParameterMarkerDiscovery.Discover(format, pattern);

		Assert.NotNull(marker);
		Assert.False(marker.IsPositional);
		Assert.Equal(prefix, marker.Prefix);
	}

	[Theory]
	[InlineData(null, null)]
	[InlineData("", null)]
	[InlineData("{0}", null)]
	[InlineData("{0}", "")]
	[InlineData("{0}", @"[a-z]+")]
	[InlineData("name", null)]
	public void Nothing_usable_is_reported_as_undetermined_not_guessed(string format, string pattern)
	{
		Assert.Null(ParameterMarkerDiscovery.Discover(format, pattern));
	}
}
