using System;
using System.Collections.Generic;
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

	/// <summary>
	/// The ADO.NET common-schema documentation defines these five collections
	/// for managed providers. Providers may add columns, but they must retain
	/// the documented required columns and CLR types.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/common-schema-collections.
	/// </summary>
	protected virtual void CheckCommonSchemaCollections(DbConnection connection, DataTable metadataCollections)
	{
		CheckMetaDataCollectionsSchema(metadataCollections);

		var advertised = new HashSet<string>(metadataCollections.Rows.Cast<DataRow>()
			.Select(row => row["CollectionName"] as string)
			.Where(name => !string.IsNullOrWhiteSpace(name))
			, StringComparer.OrdinalIgnoreCase);

		var common = new[]
		{
			"MetaDataCollections",
			"DataSourceInformation",
			"DataTypes",
			"Restrictions",
			"ReservedWords"
		};

		foreach (var collectionName in common)
		{
			Assert.Contains(collectionName, advertised);
			var table = connection.GetSchema(collectionName);
			Assert.Equal(collectionName, table.TableName);
			CheckRequiredColumns(table, collectionName);
		}
	}

	private static void CheckRequiredColumns(DataTable table, string collectionName)
	{
		void Check(string name, Type type)
		{
			Assert.True(table.Columns.Contains(name), $"{collectionName} must contain the required column {name}.");
			Assert.Same(type, Assert.IsType<DataColumn>(table.Columns[name]).DataType);
		}

		void CheckEnum<TEnum>(string name) where TEnum : struct, Enum
		{
			Assert.True(table.Columns.Contains(name), $"{collectionName} must contain the required column {name}.");
			var type = Assert.IsType<DataColumn>(table.Columns[name]).DataType;
			// DataTable providers commonly expose enum-valued metadata using the enum's underlying
			// integral CLR type. The contract requires the documented value, not a particular boxing type.
			Assert.True(type == typeof(TEnum) || type == Enum.GetUnderlyingType(typeof(TEnum)),
				$"{collectionName}.{name} must use {typeof(TEnum).Name} or its underlying type, but was {type.Name}.");
		}

		switch (collectionName)
		{
			case "DataSourceInformation":
				Check("CompositeIdentifierSeparatorPattern", typeof(string));
				Check("DataSourceProductName", typeof(string));
				Check("DataSourceProductVersion", typeof(string));
				Check("DataSourceProductVersionNormalized", typeof(string));
					CheckEnum<GroupByBehavior>("GroupByBehavior");
					Check("IdentifierPattern", typeof(string));
					CheckEnum<IdentifierCase>("IdentifierCase");
				Check("OrderByColumnsInSelect", typeof(bool));
				Check("ParameterMarkerFormat", typeof(string));
				Check("ParameterNameMaxLength", typeof(int));
				Check("ParameterNamePattern", typeof(string));
				Check("QuotedIdentifierPattern", typeof(string));
					CheckEnum<IdentifierCase>("QuotedIdentifierCase");
				Check("StatementSeparatorPattern", typeof(string));
				Check("StringLiteralPattern", typeof(string));
					CheckEnum<SupportedJoinOperators>("SupportedJoinOperators");
				break;
			case "DataTypes":
				Check("TypeName", typeof(string));
				Check("ProviderDbType", typeof(int));
				Check("ColumnSize", typeof(long));
				Check("CreateFormat", typeof(string));
				Check("CreateParameters", typeof(string));
				Check("DataType", typeof(string));
				Check("IsAutoincrementable", typeof(bool));
				Check("IsBestMatch", typeof(bool));
				Check("IsCaseSensitive", typeof(bool));
				Check("IsFixedLength", typeof(bool));
				Check("IsFixedPrecisionScale", typeof(bool));
				Check("IsLong", typeof(bool));
				Check("IsNullable", typeof(bool));
				Check("IsSearchable", typeof(bool));
				Check("IsSearchableWithLike", typeof(bool));
				Check("IsUnsigned", typeof(bool));
				Check("MaximumScale", typeof(short));
				Check("MinimumScale", typeof(short));
				Check("IsConcurrencyType", typeof(bool));
				Check("IsLiteralSupported", typeof(bool));
				Check("LiteralPrefix", typeof(string));
				Check("LiteralSuffix", typeof(string));
				break;
			case "Restrictions":
				Check("CollectionName", typeof(string));
				Check("RestrictionName", typeof(string));
				Check("RestrictionDefault", typeof(string));
				Check("RestrictionNumber", typeof(int));
				break;
			case "ReservedWords":
				Check("ReservedWord", typeof(string));
				break;
		}
	}
}
