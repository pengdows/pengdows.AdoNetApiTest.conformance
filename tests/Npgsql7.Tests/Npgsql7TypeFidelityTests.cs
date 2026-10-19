using System.Collections.Generic;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Npgsql7.Tests;

public class Npgsql7TypeFidelityFixture : Npgsql7DbFactoryFixture, ITypeFidelityFixture
{
	public IReadOnlyList<TypeSpec> Types => PostgreSqlTypes.Types;

	public IReadOnlyList<UncoveredType> UncoveredTypes => PostgreSqlTypes.UncoveredTypes;

	public string ParameterMarker(string name) => PostgreSqlTypes.ParameterMarker(name);

	public string CreateTableSql(string table, string columnSql) => PostgreSqlTypes.CreateTableSql(table, columnSql);

	public string DropTableSql(string table) => PostgreSqlTypes.DropTableSql(table);
}

public sealed class Npgsql7TypeFidelityTests : TypeFidelityTestBase<Npgsql7TypeFidelityFixture>
{
	public Npgsql7TypeFidelityTests(Npgsql7TypeFidelityFixture fixture)
		: base(fixture)
	{
	}
}
