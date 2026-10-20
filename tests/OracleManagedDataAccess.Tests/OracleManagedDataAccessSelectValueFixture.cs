using System.Collections.Generic;
using System.Data;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace OracleManagedDataAccess.Tests;

/// <summary>Oracle spells a binary column <c>raw</c> and its literal <c>hextoraw</c>; <c>binary_double</c> holds the double range NUMBER cannot.</summary>
public sealed class OracleManagedDataAccessSelectValueFixture : SelectValueFixture<OracleManagedDataAccessDbFactoryFixture>
{
	protected override IReadOnlyDictionary<DbType, string> ColumnTypes { get; } =
		SelectValueTables.WithOverrides(new Dictionary<string, string> { ["Binary"] = "raw(100)", ["Double"] = "binary_double" });

	protected override string BinaryLiteral => "hextoraw('{0}')";
}
