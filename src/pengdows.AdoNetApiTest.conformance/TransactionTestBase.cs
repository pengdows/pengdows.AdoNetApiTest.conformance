using System;
using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public partial class TransactionTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	public TransactionTestBase(TFixture fixture)
		: base(fixture)
	{
		Fixture = fixture;
	}

	protected new TFixture Fixture { get; }



}
