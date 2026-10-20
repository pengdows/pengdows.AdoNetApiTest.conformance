namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// A provider whose database does not accept a bare <c>SELECT 1;</c> states its shape here once, instead of overriding
/// the SELECT hooks in every test class: Oracle needs <c>FROM dual</c> and no terminator, Firebird <c>FROM rdb$database</c>.
/// </summary>
public interface ISqlDialectFixture
{
	/// <summary>A statement selecting <paramref name="projection"/> as one row, without a terminator, such as <c>SELECT 1 FROM dual</c>.</summary>
	string SelectStatement(string projection);

	/// <summary>The text that ends a statement: <c>;</c>, or empty for a database that rejects it.</summary>
	string StatementTerminator { get; }
}
