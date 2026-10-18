using System;
using System.Data.Common;
using System.Transactions;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Tests explicit transaction enlistment for providers that opt in through
/// <see cref="IEnlistmentFixture"/>.
/// </summary>
public abstract partial class EnlistmentTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IEnlistmentFixture
{
	protected EnlistmentTestBase(TFixture fixture)
		: base(fixture)
	{
	}

}
