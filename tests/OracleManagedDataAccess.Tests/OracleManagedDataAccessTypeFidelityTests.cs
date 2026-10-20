using System.Collections.Generic;
using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace OracleManagedDataAccess.Tests;

public class OracleManagedDataAccessTypeFidelityFixture : OracleManagedDataAccessDbFactoryFixture, ITypeFidelityFixture
{
	public IReadOnlyList<TypeSpec> Types => OracleTypes.Types;

	public IReadOnlyList<UncoveredType> UncoveredTypes => OracleTypes.UncoveredTypes;

	public string ParameterMarker(string name) => OracleTypes.ParameterMarker(name);

	public string CreateTableSql(string table, string columnSql) => OracleTypes.CreateTableSql(table, columnSql);

	public string DropTableSql(string table) => OracleTypes.DropTableSql(table);
}

public sealed class OracleManagedDataAccessTypeFidelityTests : TypeFidelityTestBase<OracleManagedDataAccessTypeFidelityFixture>
{
	public OracleManagedDataAccessTypeFidelityTests(OracleManagedDataAccessTypeFidelityFixture fixture)
		: base(fixture)
	{
	}
}
