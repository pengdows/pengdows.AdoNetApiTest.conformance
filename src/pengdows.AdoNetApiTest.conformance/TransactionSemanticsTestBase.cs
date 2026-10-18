using System;
using System.Data.Common;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Tests transaction effects for providers that opt in with
/// <see cref="ITransactionSemanticsFixture"/>.
/// </summary>
public abstract partial class TransactionSemanticsTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, ITransactionSemanticsFixture
{
	protected TransactionSemanticsTestBase(TFixture fixture)
		: base(fixture)
	{
	}

}
