using System.Collections.Generic;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace SqlClient.Tests;

public class SqlClientTypeFidelityFixture : SqlClientDbFactoryFixture, ITypeFidelityFixture
{
	public IReadOnlyList<TypeSpec> Types => SqlServerTypes.Types;

	public IReadOnlyList<UncoveredType> UncoveredTypes => SqlServerTypes.UncoveredTypes;

	public string ParameterMarker(string name) => SqlServerTypes.ParameterMarker(name);

	public string CreateTableSql(string table, string columnSql) => SqlServerTypes.CreateTableSql(table, columnSql);

	public string DropTableSql(string table) => SqlServerTypes.DropTableSql(table);
}

public sealed class SqlClientTypeFidelityTests : TypeFidelityTestBase<SqlClientTypeFidelityFixture>
{
	public SqlClientTypeFidelityTests(SqlClientTypeFidelityFixture fixture)
		: base(fixture)
	{
	}
}
