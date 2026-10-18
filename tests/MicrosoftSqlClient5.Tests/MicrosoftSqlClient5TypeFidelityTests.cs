using System.Collections.Generic;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlClient5.Tests;

public class MicrosoftSqlClient5TypeFidelityFixture : MicrosoftSqlClient5DbFactoryFixture, ITypeFidelityFixture
{
	public IReadOnlyList<TypeSpec> Types => SqlServerTypes.Types;

	public IReadOnlyList<UncoveredType> UncoveredTypes => SqlServerTypes.UncoveredTypes;

	public string ParameterMarker(string name) => SqlServerTypes.ParameterMarker(name);

	public string CreateTableSql(string table, string columnSql) => SqlServerTypes.CreateTableSql(table, columnSql);

	public string DropTableSql(string table) => SqlServerTypes.DropTableSql(table);
}

public sealed class MicrosoftSqlClient5TypeFidelityTests : TypeFidelityTestBase<MicrosoftSqlClient5TypeFidelityFixture>
{
	public MicrosoftSqlClient5TypeFidelityTests(MicrosoftSqlClient5TypeFidelityFixture fixture)
		: base(fixture)
	{
	}
}
