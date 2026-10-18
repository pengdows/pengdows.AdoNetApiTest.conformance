using System;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public partial class ConnectionStringTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	public ConnectionStringTestBase(TFixture fixture)
		: base(fixture)
	{
		Fixture = fixture;
	}

	protected new TFixture Fixture { get; }

}
