using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlite8.Tests;

public sealed class MicrosoftSqlite8TransactionTests(MicrosoftSqlite8DbFactoryFixture fixture)
	: TransactionTestBase<MicrosoftSqlite8DbFactoryFixture>(fixture)
{
}
