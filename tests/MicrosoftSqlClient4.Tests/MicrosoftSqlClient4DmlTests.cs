using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlClient4.Tests;

public sealed class MicrosoftSqlClient4DmlTests : DmlTestBase<DmlFixture<MicrosoftSqlClient4DbFactoryFixture>>
{
	public MicrosoftSqlClient4DmlTests(DmlFixture<MicrosoftSqlClient4DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
