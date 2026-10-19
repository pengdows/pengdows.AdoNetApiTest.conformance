using System.Collections.Generic;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Npgsql6.Tests;

public class Npgsql6TypeFidelityFixture : Npgsql6DbFactoryFixture, ITypeFidelityFixture
{
	public IReadOnlyList<TypeSpec> Types => PostgreSqlTypes.Types;

	public IReadOnlyList<UncoveredType> UncoveredTypes => PostgreSqlTypes.UncoveredTypes;

	public string ParameterMarker(string name) => PostgreSqlTypes.ParameterMarker(name);

	public string CreateTableSql(string table, string columnSql) => PostgreSqlTypes.CreateTableSql(table, columnSql);

	public string DropTableSql(string table) => PostgreSqlTypes.DropTableSql(table);
}

public sealed class Npgsql6TypeFidelityTests : TypeFidelityTestBase<Npgsql6TypeFidelityFixture>
{
	public Npgsql6TypeFidelityTests(Npgsql6TypeFidelityFixture fixture)
		: base(fixture)
	{
	}
}
