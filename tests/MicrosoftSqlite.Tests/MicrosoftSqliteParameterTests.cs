using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace MicrosoftSqlite.Tests;

public sealed class MicrosoftSqliteParameterTests : ParameterTestBase<MicrosoftSqliteDbFactoryFixture>
{
	public MicrosoftSqliteParameterTests(MicrosoftSqliteDbFactoryFixture fixture)
		: base(fixture)
	{
	}
}
