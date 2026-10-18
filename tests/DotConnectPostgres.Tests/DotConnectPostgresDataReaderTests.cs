using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace DotConnectPostgres.Tests;

public sealed class DotConnectPostgresDataReaderTests : DataReaderTestBase<DotConnectPostgresSelectValueFixture>
{
	public DotConnectPostgresDataReaderTests(DotConnectPostgresSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
