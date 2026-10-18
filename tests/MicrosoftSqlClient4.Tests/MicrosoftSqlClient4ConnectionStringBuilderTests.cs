using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlClient4.Tests;

public sealed class MicrosoftSqlClient4ConnectionStringBuilderTests : ConnectionStringTestBase<MicrosoftSqlClient4DbFactoryFixture>
{
	public MicrosoftSqlClient4ConnectionStringBuilderTests(MicrosoftSqlClient4DbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
