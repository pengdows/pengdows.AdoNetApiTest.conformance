using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlClient5.Tests;

public sealed class MicrosoftSqlClient5DmlTests : DmlTestBase<DmlFixture<MicrosoftSqlClient5DbFactoryFixture>>
{
	public MicrosoftSqlClient5DmlTests(DmlFixture<MicrosoftSqlClient5DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
