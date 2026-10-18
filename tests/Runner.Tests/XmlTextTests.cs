using pengdows.AdoNetApiTest.conformance.Runner;
using Xunit;

namespace Runner.Tests;

public class XmlTextTests
{
	[Theory]
	[InlineData("a\u0001b", "a\\u0001b")]
	[InlineData("a\u0000b", "a\\u0000b")]
	[InlineData("a￿b", "a\\uFFFFb")]
	[InlineData("a￾b", "a\\uFFFEb")]
	public void Characters_xml_cannot_hold_become_visible_escapes(string input, string expected) =>
		Assert.Equal(expected, XmlText.Escape(input));

	[Fact]
	public void A_lone_surrogate_becomes_a_visible_escape()
	{
		// built from code: xUnit rewrites a lone surrogate in an attribute argument to U+FFFD
		Assert.Equal("a\\uD800b", XmlText.Escape("a" + (char) 0xD800 + "b"));
		Assert.Equal("a\\uDC00b", XmlText.Escape("a" + (char) 0xDC00 + "b"));
		Assert.Equal("\\uD83D", XmlText.Escape(((char) 0xD83D).ToString()));
	}

	[Theory]
	[InlineData("plain text")]
	[InlineData("tab\tnewline\ncarriage\rreturn")]
	[InlineData("café ✓ 日本語")]
	[InlineData("pair 😀 kept")]
	[InlineData("")]
	public void Everything_else_is_left_alone(string input) => Assert.Equal(input, XmlText.Escape(input));

	[Fact]
	public void Null_is_empty() => Assert.Equal(string.Empty, XmlText.Escape(null));

	[Fact]
	public void A_result_file_with_illegal_characters_anywhere_still_parses()
	{
		var xml = new XmlBuilder().Test("A", "Fail", "Required", failure: "x@F@y", output: "o@LONE@p").ToString()
			.Replace("@F@", "￿").Replace("@LONE@", "\uD800");

		var raw = Assert.Single(XunitXml.Parse(xml));

		Assert.Equal("x\\uFFFFy", raw.Failure);
		Assert.Equal("o\\uD800p", raw.Output);
	}
}
