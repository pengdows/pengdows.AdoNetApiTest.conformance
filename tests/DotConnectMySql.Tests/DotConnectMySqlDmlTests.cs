using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace DotConnectMySql.Tests;

public sealed class DotConnectMySqlDmlTests : DmlTestBase<DmlFixture<DotConnectMySqlDbFactoryFixture>>
{
	public DotConnectMySqlDmlTests(DmlFixture<DotConnectMySqlDbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
