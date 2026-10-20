namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Optional on a fixture whose database needs other SQL than the SQL-92 defaults the generated <c>DataTypes</c> catalog
/// uses: <c>create table t (Id integer not null primary key, V type)</c>, <c>drop table t</c> and
/// <c>CAST(V AS VARCHAR(4000))</c> to render a stored value as text.
/// </summary>
public interface IDataTypesFidelityFixture
{
	/// <summary>A statement that creates <paramref name="table"/> with an <c>Id</c> primary key and a nullable column <c>V</c>.</summary>
	string CreateTableSql(string table, string columnSql);

	/// <summary>A statement that drops <paramref name="table"/>.</summary>
	string DropTableSql(string table);

	/// <summary>SQL that renders a stored value as text, with <c>{0}</c> for the column.</summary>
	string RenderAsText { get; }
}
