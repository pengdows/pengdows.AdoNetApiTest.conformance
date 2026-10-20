using pengdows.AdoNetApiTest.conformance;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace DotConnectPostgres.Tests;

public sealed class DotConnectPostgresDmlTests : DmlTestBase<DmlFixture<DotConnectPostgresDbFactoryFixture>>
{
	public DotConnectPostgresDmlTests(DmlFixture<DotConnectPostgresDbFactoryFixture> fixture)
		: base(fixture)
	{
	}
}
