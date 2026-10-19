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

	/// <summary>
	/// Creates the provider's <see cref="DbDataSource"/>. Returns <c>null</c>, after recording "not implemented", when the
	/// provider does not expose one; the calling test then ends as a pass.
	/// </summary>
	protected virtual DbDataSource TryCreateDataSource()
	{
		try
		{
			var dataSource = Fixture.Factory.CreateDataSource(ConnectionString);
			if (dataSource is not null)
			{
				return dataSource;
			}
		}
		catch (NotSupportedException)
		{
		}

		ConformanceOutcome.NotImplemented("The provider does not expose DbDataSource.");
		return null;
	}
}
#endif
