using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class ConnectionTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	protected ConnectionTestBase(TFixture fixture)
		: base(fixture)
	{
	}





	/// <summary>
	/// The common schema collections page lists the columns MetaDataCollections has; a provider may add its own after
	/// them (ODP.NET and FirebirdClient add PopulationMechanism and PopulationString).
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/common-schema-collections.
	/// </summary>
	protected virtual void CheckMetaDataCollectionsSchema(DataTable table)
	{
		Assert.Equal("MetaDataCollections", table.TableName);
		Assert.Same(typeof(string), Assert.IsType<DataColumn>(table.Columns["CollectionName"]).DataType);
		Assert.Same(typeof(int), Assert.IsType<DataColumn>(table.Columns["NumberOfRestrictions"]).DataType);
		Assert.Same(typeof(int), Assert.IsType<DataColumn>(table.Columns["NumberOfIdentifierParts"]).DataType);
	}
}
