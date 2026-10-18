using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class IsolationLevelTestBase<TFixture>
	where TFixture : class, IIsolationLevelFixture
{
	/// <summary>
	/// BeginTransaction(level) must create a transaction using the supported level
	/// supplied by the provider fixture.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.begintransaction.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.begintransaction")]
	public virtual void BeginTransaction_uses_supported_isolation_level()
	{
		using var connection = CreateOpenConnection();
		using var transaction = connection.BeginTransaction(Fixture.SupportedIsolationLevel);

		Assert.Equal(Fixture.SupportedIsolationLevel, transaction.IsolationLevel);
	}
}
