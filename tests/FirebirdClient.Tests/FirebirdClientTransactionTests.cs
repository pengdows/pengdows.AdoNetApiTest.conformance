using pengdows.AdoNetApiTest.conformance;

namespace FirebirdClient.Tests;

public sealed class FirebirdClientTransactionTests : TransactionTestBase<FirebirdClientDbFactoryFixture>
{
	public FirebirdClientTransactionTests(FirebirdClientDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
