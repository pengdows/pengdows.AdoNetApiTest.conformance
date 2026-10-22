using System.Collections.Generic;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>SQL used to verify that savepoint rollback changes transaction state, not only that the methods return.</summary>
public interface ISavepointSemanticsFixture : IDbFactoryFixture
{
	IReadOnlyList<string> SavepointSetupSql { get; }

	IReadOnlyList<string> SavepointCleanupSql { get; }

	string SavepointFirstInsertSql { get; }

	string SavepointSecondInsertSql { get; }

	string SavepointRowCountSql { get; }
}
