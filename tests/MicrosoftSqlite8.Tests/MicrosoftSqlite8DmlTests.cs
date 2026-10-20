using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace MicrosoftSqlite8.Tests;

public sealed class MicrosoftSqlite8DmlTests : DmlTestBase<DmlFixture<MicrosoftSqlite8DbFactoryFixture>>
{
	public MicrosoftSqlite8DmlTests(DmlFixture<MicrosoftSqlite8DbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
