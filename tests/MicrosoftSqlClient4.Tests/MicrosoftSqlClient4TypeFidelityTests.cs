using System.Collections.Generic;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlClient4.Tests;

public class MicrosoftSqlClient4TypeFidelityFixture : MicrosoftSqlClient4DbFactoryFixture, ITypeFidelityFixture
{
	public IReadOnlyList<TypeSpec> Types => SqlServerTypes.Types;

	public IReadOnlyList<UncoveredType> UncoveredTypes => SqlServerTypes.UncoveredTypes;

	public string ParameterMarker(string name) => SqlServerTypes.ParameterMarker(name);

	public string CreateTableSql(string table, string columnSql) => SqlServerTypes.CreateTableSql(table, columnSql);

	public string DropTableSql(string table) => SqlServerTypes.DropTableSql(table);
}

public sealed class MicrosoftSqlClient4TypeFidelityTests : TypeFidelityTestBase<MicrosoftSqlClient4TypeFidelityFixture>
{
	public MicrosoftSqlClient4TypeFidelityTests(MicrosoftSqlClient4TypeFidelityFixture fixture)
		: base(fixture)
	{
	}
}
