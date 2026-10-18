using pengdows.AdoNetApiTest.conformance;

namespace FaithlifeSqlite.Tests;

public sealed class FaithlifeSqliteParameterTests : ParameterTestBase<FaithlifeSqliteDbFactoryFixture>
{
	public FaithlifeSqliteParameterTests(FaithlifeSqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
