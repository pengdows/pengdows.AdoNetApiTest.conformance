using System.Threading.Tasks;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class ChangeDatabaseTestBase<TFixture>
	where TFixture : class, IChangeDatabaseFixture
{
	/// <summary>
	/// ChangeDatabase changes the database associated with an open connection.
	/// The fixture supplies the valid target and provider-specific verification.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.changedatabase.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.changedatabase")]
	public virtual void ChangeDatabase_changes_open_connection_database()
	{
		using var connection = CreateOpenConnection();
		connection.ChangeDatabase(Fixture.ChangeDatabaseTarget);
		Fixture.AssertChangedDatabase(connection);
	}

#if NETSTANDARD2_1_OR_GREATER
	/// <summary>
	/// ChangeDatabaseAsync must perform the same database transition as the
	/// synchronous API for a provider that supports changing databases.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.changedatabaseasync.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.changedatabaseasync")]
	public virtual async Task ChangeDatabaseAsync_changes_open_connection_database()
	{
		using var connection = CreateOpenConnection();
		await connection.ChangeDatabaseAsync(Fixture.ChangeDatabaseTarget).ConfigureAwait(false);
		Fixture.AssertChangedDatabase(connection);
	}
#endif
}
