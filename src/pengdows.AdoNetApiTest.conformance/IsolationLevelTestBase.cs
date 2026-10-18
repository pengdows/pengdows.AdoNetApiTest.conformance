using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Tests explicit isolation-level transactions for providers that opt in with
/// <see cref="IIsolationLevelFixture"/>.
/// </summary>
public abstract partial class IsolationLevelTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IIsolationLevelFixture
{
	protected IsolationLevelTestBase(TFixture fixture)
		: base(fixture)
	{
	}

}
