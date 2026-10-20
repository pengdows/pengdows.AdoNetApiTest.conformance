namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Declared by a fixture whose database allows one write connection at a time (SQLite, DuckDB, Access,
/// pengdows.flatfile). Its project runs its test classes one after another
/// (<c>[assembly: CollectionBehavior(DisableTestParallelization = true)]</c>), and a test that needs two connections
/// writing at once reports itself not applicable instead of waiting on the database's lock.
/// </summary>
public interface ISingleWriterFixture
{
}
