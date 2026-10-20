using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlClient3.Tests;

public sealed class MicrosoftSqlClient3DmlTests : DmlTestBase<DmlFixture<MicrosoftSqlClient3DbFactoryFixture>>
{
	public MicrosoftSqlClient3DmlTests(DmlFixture<MicrosoftSqlClient3DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
