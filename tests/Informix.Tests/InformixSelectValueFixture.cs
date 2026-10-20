using System.Collections.Generic;
using System.Data;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Informix.Tests;

/// <summary>Informix has no <c>varbinary</c> (its <c>byte</c> type takes no literal), so binary columns are left out.</summary>
public sealed class InformixSelectValueFixture : SelectValueFixture<InformixDbFactoryFixture>
{
	protected override IReadOnlyDictionary<DbType, string> ColumnTypes { get; } =
		SelectValueTables.WithOverrides(new Dictionary<string, string> { ["Binary"] = "" });
}
