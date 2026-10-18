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





	protected virtual void CheckMetaDataCollectionsSchema(DataTable table)
	{
		Assert.Equal("MetaDataCollections", table.TableName);
		Assert.Collection(table.Columns.Cast<DataColumn>(),
			c =>
			{
				Assert.Equal("CollectionName", c.ColumnName);
				Assert.Same(typeof(string), c.DataType);
			},
			c =>
			{
				Assert.Equal("NumberOfRestrictions", c.ColumnName);
				Assert.Same(typeof(int), c.DataType);
			},
			c =>
			{
				Assert.Equal("NumberOfIdentifierParts", c.ColumnName);
				Assert.Same(typeof(int), c.DataType);
			});
	}
}
