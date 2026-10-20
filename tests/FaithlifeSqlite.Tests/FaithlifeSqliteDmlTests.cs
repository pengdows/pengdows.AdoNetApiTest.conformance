using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace FaithlifeSqlite.Tests;

public sealed class FaithlifeSqliteDmlTests : DmlTestBase<DmlFixture<FaithlifeSqliteDbFactoryFixture>>
{
	public FaithlifeSqliteDmlTests(DmlFixture<FaithlifeSqliteDbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
