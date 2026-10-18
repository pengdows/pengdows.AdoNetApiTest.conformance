using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite7.Tests;

public sealed class MicrosoftSqlite7ConnectionTests : ConnectionTestBase<MicrosoftSqlite7DbFactoryFixture>
{
	public MicrosoftSqlite7ConnectionTests(MicrosoftSqlite7DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
