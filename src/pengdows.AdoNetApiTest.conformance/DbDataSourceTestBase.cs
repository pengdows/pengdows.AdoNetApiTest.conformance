#if NET10_0_OR_GREATER
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Tests the modern <see cref="DbDataSource"/> contract for providers that
/// expose <see cref="DbProviderFactory.CreateDataSource(string)"/>.
/// </summary>
public abstract partial class DbDataSourceTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	protected DbDataSourceTestBase(TFixture fixture)
		: base(fixture)
	{
	}

	protected virtual DbDataSource CreateDataSourceOrSkip()
	{
		try
		{
			return Fixture.Factory.CreateDataSource(Fixture.ConnectionString)
				?? throw Xunit.Sdk.SkipException.ForSkip("The provider does not expose DbDataSource.");
		}
		catch (NotSupportedException)
		{
			throw Xunit.Sdk.SkipException.ForSkip("The provider does not expose DbDataSource.");
		}
	}
}
#endif
