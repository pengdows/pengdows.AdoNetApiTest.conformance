using System.Collections.Generic;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Npgsql8.Tests;

public class Npgsql8TypeFidelityFixture : Npgsql8DbFactoryFixture, ITypeFidelityFixture
{
	public IReadOnlyList<TypeSpec> Types => PostgreSqlTypes.Types;

	public IReadOnlyList<UncoveredType> UncoveredTypes => PostgreSqlTypes.UncoveredTypes;

	public string ParameterMarker(string name) => PostgreSqlTypes.ParameterMarker(name);

	public string CreateTableSql(string table, string columnSql) => PostgreSqlTypes.CreateTableSql(table, columnSql);

	public string DropTableSql(string table) => PostgreSqlTypes.DropTableSql(table);
}

public sealed class Npgsql8TypeFidelityTests : TypeFidelityTestBase<Npgsql8TypeFidelityFixture>
{
	public Npgsql8TypeFidelityTests(Npgsql8TypeFidelityFixture fixture)
		: base(fixture)
	{
	}
}
