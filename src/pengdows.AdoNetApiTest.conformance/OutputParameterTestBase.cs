using System.Data;
using System.Data.Common;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Tests the documented output-parameter execution contract for providers that
/// explicitly opt in through <see cref="IOutputParameterFixture"/>.
/// </summary>
public abstract partial class OutputParameterTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IOutputParameterFixture
{
	protected OutputParameterTestBase(TFixture fixture)
		: base(fixture)
	{
	}

}
