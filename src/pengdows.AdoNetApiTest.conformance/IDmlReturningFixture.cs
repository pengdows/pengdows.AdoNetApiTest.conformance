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
