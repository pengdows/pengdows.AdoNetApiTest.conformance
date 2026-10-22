namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// DML statements that also return rows (<c>RETURNING</c>, <c>OUTPUT</c>), over the <see cref="IDmlFixture"/> table and its
/// rows. They are still INSERT and DELETE statements, so their affected-row counts follow the same contract. The syntax
/// is not SQL-92; a fixture whose database has none returns null and the tests are not applicable.
/// </summary>
public interface IDmlReturningFixture : IDmlFixture
{
	/// <summary>An INSERT of exactly one row that returns a result set, or null.</summary>
	string DmlInsertReturningSql { get; }

	/// <summary>A DELETE of both setup rows that returns a result set, or null.</summary>
	string DmlDeleteReturningSql { get; }

	/// <summary>
	/// An INSERT of exactly one row that returns a result set, with its key and text values as parameters: <c>{0}</c> stands
	/// for the key parameter's marker and <c>{1}</c> for the text parameter's. Null when the database has no RETURNING form.
	/// </summary>
	string DmlInsertReturningParameterSql { get; }
}

/// <summary>
/// A SQL:2003 MERGE over the <see cref="IDmlFixture"/> table that updates one setup row and inserts one new row. A database
/// that rejects it has no MERGE, and the tests are not applicable.
/// </summary>
public interface IMergeFixture : IDmlFixture
{
	string DmlMergeSql { get; }
}

/// <summary>
/// The form a database gives DML that returns rows, for a fixture whose DML statements are generated: <c>{0}</c> is the
/// statement, such as <c>{0} RETURNING Id</c>.
/// </summary>
public interface IReturningDialectFixture
{
	string ReturningFormat { get; }
}

/// <summary>
/// Parameterized statements over the <see cref="IDmlFixture"/> table, whose <c>V</c> column holds text, for tests of what a
/// provider stores for a parameter value rather than what it echoes (some databases cannot type an untyped parameter in
/// a bare <c>SELECT</c>).
/// </summary>
public interface IDmlParameterFixture : IDmlFixture
{
	/// <summary>An INSERT of one row: <c>{0}</c> stands for the key parameter's marker, <c>{1}</c> for the text parameter's.</summary>
	string DmlInsertParameterSql { get; }

	/// <summary>A SELECT of <c>V</c> for the row whose key is <c>{0}</c>, a literal integer.</summary>
	string DmlSelectValueSql { get; }
}
