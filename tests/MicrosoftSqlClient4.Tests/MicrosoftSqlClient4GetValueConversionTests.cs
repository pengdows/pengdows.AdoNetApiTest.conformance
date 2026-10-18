using System;
using pengdows.AdoNetApiTest.conformance;

namespace MicrosoftSqlClient4.Tests;

public sealed class MicrosoftSqlClient4GetValueConversionTests : GetValueConversionTestBase<MicrosoftSqlClient4SelectValueFixture>
{
	public MicrosoftSqlClient4GetValueConversionTests(MicrosoftSqlClient4SelectValueFixture fixture)
		: base(fixture)
	{
	}
}
