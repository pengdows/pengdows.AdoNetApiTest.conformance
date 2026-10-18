using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite8.Tests;

public sealed class MicrosoftSqlite8ConnectionTests(MicrosoftSqlite8DbFactoryFixture fixture)
	: ConnectionTestBase<MicrosoftSqlite8DbFactoryFixture>(fixture)
{
}
