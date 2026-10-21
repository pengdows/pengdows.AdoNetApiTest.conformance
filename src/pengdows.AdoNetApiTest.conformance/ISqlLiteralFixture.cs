namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// A provider whose database spells text literals or quoted identifiers differently from SQL-92 states it here once.
/// Without it the shared tests write <c>'text'</c> and <c>"name"</c>. YQL (YDB) reads <c>'text'</c> as a byte string and
/// needs <c>'text'u</c> for text, and quotes names with backticks.
/// </summary>
public interface ISqlLiteralFixture
{
	/// <summary>A literal of the database's text type holding <paramref name="text"/>, such as <c>'test'</c>.</summary>
	string TextLiteral(string text);

	/// <summary><paramref name="name"/> as a quoted, case-preserving identifier, such as <c>"value"</c>.</summary>
	string QuotedIdentifier(string name);
}

/// <summary>The SQL-92 spellings, for a fixture that does not implement <see cref="ISqlLiteralFixture"/>.</summary>
public static class SqlLiterals
{
	public static string Text(object fixture, string text) => fixture is ISqlLiteralFixture literals
		? literals.TextLiteral(text)
		: "'" + text.Replace("'", "''") + "'";

	public static string Quoted(object fixture, string name) => fixture is ISqlLiteralFixture literals
		? literals.QuotedIdentifier(name)
		: "\"" + name.Replace("\"", "\"\"") + "\"";
}
