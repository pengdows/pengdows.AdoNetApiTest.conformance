using pengdows.AdoNetApiTest.conformance;

namespace FaithlifeSqlite.Tests;

public sealed class FaithlifeSqliteConnectionStringBuilderTests : ConnectionStringTestBase<FaithlifeSqliteDbFactoryFixture>
{
	public FaithlifeSqliteConnectionStringBuilderTests(FaithlifeSqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
