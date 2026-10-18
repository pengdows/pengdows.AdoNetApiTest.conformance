using System.Collections.Generic;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlClient3.Tests;

public class MicrosoftSqlClient3TypeFidelityFixture : MicrosoftSqlClient3DbFactoryFixture, ITypeFidelityFixture
{
	public IReadOnlyList<TypeSpec> Types => SqlServerTypes.Types;

	public IReadOnlyList<UncoveredType> UncoveredTypes => SqlServerTypes.UncoveredTypes;

	public string ParameterMarker(string name) => SqlServerTypes.ParameterMarker(name);

	public string CreateTableSql(string table, string columnSql) => SqlServerTypes.CreateTableSql(table, columnSql);

	public string DropTableSql(string table) => SqlServerTypes.DropTableSql(table);
}

public sealed class MicrosoftSqlClient3TypeFidelityTests : TypeFidelityTestBase<MicrosoftSqlClient3TypeFidelityFixture>
{
	public MicrosoftSqlClient3TypeFidelityTests(MicrosoftSqlClient3TypeFidelityFixture fixture)
		: base(fixture)
	{
	}
}
