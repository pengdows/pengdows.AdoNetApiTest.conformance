using System.Data.Common;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Supplies a valid alternate database target for providers that support
/// changing the database on an open connection.
/// </summary>
public interface IChangeDatabaseFixture : IDbFactoryFixture
{
	/// <summary>Returns a database name valid for the configured server.</summary>
	string ChangeDatabaseTarget { get; }

	/// <summary>
	/// Verifies the provider-specific database identity after the change.
	/// </summary>
	void AssertChangedDatabase(DbConnection connection);
}
