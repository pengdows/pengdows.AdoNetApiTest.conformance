using System.Collections.Generic;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Npgsql4.Tests;

public class Npgsql4TypeFidelityFixture : Npgsql4DbFactoryFixture, ITypeFidelityFixture
{
	public IReadOnlyList<TypeSpec> Types => PostgreSqlTypes.Types;

	public IReadOnlyList<UncoveredType> UncoveredTypes => PostgreSqlTypes.UncoveredTypes;

	public string ParameterMarker(string name) => PostgreSqlTypes.ParameterMarker(name);

	public string CreateTableSql(string table, string columnSql) => PostgreSqlTypes.CreateTableSql(table, columnSql);

	public string DropTableSql(string table) => PostgreSqlTypes.DropTableSql(table);
}

public sealed class Npgsql4TypeFidelityTests : TypeFidelityTestBase<Npgsql4TypeFidelityFixture>
{
	public Npgsql4TypeFidelityTests(Npgsql4TypeFidelityFixture fixture)
		: base(fixture)
	{
	}
}
