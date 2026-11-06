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

/// <summary>
/// A provider that cannot select a bare parameter marker (<c>SELECT ?</c>) states how to write one. The ACE OLE DB
/// provider faults in native code on it and needs a conversion around the marker, <c>CVar(?)</c>. Without it the
/// shared tests select the marker itself.
/// </summary>
public interface ISqlParameterProjectionFixture
{
	/// <summary>The projection that selects the parameter written as <paramref name="marker"/>.</summary>
	string ParameterProjection(string marker);
}
