using System.Collections.Generic;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Npgsql5.Tests;

public class Npgsql5TypeFidelityFixture : Npgsql5DbFactoryFixture, ITypeFidelityFixture
{
	public IReadOnlyList<TypeSpec> Types => PostgreSqlTypes.Types;

	public IReadOnlyList<UncoveredType> UncoveredTypes => PostgreSqlTypes.UncoveredTypes;

	public string ParameterMarker(string name) => PostgreSqlTypes.ParameterMarker(name);

	public string CreateTableSql(string table, string columnSql) => PostgreSqlTypes.CreateTableSql(table, columnSql);

	public string DropTableSql(string table) => PostgreSqlTypes.DropTableSql(table);
}

public sealed class Npgsql5TypeFidelityTests : TypeFidelityTestBase<Npgsql5TypeFidelityFixture>
{
	public Npgsql5TypeFidelityTests(Npgsql5TypeFidelityFixture fixture)
		: base(fixture)
	{
	}
}
