using pengdows.AdoNetApiTest.conformance;

namespace FaithlifeSqlite.Tests;

public sealed class FaithlifeSqliteGetValueConversionTests : GetValueConversionTestBase<FaithlifeSqliteSelectValueFixture>
{
	public FaithlifeSqliteGetValueConversionTests(FaithlifeSqliteSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
