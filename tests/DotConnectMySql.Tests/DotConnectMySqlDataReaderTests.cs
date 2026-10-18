using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace DotConnectMySql.Tests;

public sealed class DotConnectMySqlDataReaderTests : DataReaderTestBase<DotConnectMySqlSelectValueFixture>
{
	public DotConnectMySqlDataReaderTests(DotConnectMySqlSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
