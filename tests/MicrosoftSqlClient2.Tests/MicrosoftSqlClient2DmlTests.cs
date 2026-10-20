using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlClient2.Tests;

public sealed class MicrosoftSqlClient2DmlTests : DmlTestBase<DmlFixture<MicrosoftSqlClient2DbFactoryFixture>>
{
	public MicrosoftSqlClient2DmlTests(DmlFixture<MicrosoftSqlClient2DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
