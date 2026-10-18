using pengdows.AdoNetApiTest.conformance;

namespace FaithlifeSqlite.Tests;

public sealed class FaithlifeSqliteConnectionTests : ConnectionTestBase<FaithlifeSqliteDbFactoryFixture>
{
	public FaithlifeSqliteConnectionTests(FaithlifeSqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
