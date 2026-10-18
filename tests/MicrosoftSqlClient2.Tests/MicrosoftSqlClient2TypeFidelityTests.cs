using System.Collections.Generic;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlClient2.Tests;

public class MicrosoftSqlClient2TypeFidelityFixture : MicrosoftSqlClient2DbFactoryFixture, ITypeFidelityFixture
{
	public IReadOnlyList<TypeSpec> Types => SqlServerTypes.Types;

	public IReadOnlyList<UncoveredType> UncoveredTypes => SqlServerTypes.UncoveredTypes;

	public string ParameterMarker(string name) => SqlServerTypes.ParameterMarker(name);

	public string CreateTableSql(string table, string columnSql) => SqlServerTypes.CreateTableSql(table, columnSql);

	public string DropTableSql(string table) => SqlServerTypes.DropTableSql(table);
}

public sealed class MicrosoftSqlClient2TypeFidelityTests : TypeFidelityTestBase<MicrosoftSqlClient2TypeFidelityFixture>
{
	public MicrosoftSqlClient2TypeFidelityTests(MicrosoftSqlClient2TypeFidelityFixture fixture)
		: base(fixture)
	{
	}
}
