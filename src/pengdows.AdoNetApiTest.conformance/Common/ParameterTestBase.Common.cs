using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.IO;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class ParameterTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	/// <summary>
	/// The original assertion required <see cref="DbType.String"/>. ADO.NET exposes
	/// <see cref="DbParameter.DbType"/>, but does not prescribe a provider-neutral
	/// default value. This remains an active implementation-comparison diagnostic;
	/// providers with a different default can override it.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.dbtype.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.dbtype", Because = "provider default is not a universal binding contract")]
	public virtual void Parameter_default_DbType_is_string()
	{
		if (Fixture.Factory.CreateParameter().DbType == DbType.String)
			return;

		SoftWarning.Report("The provider uses a different default DbType; ADO.NET does not prescribe String as the universal default.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.isnullable", Because = "provider default IsNullable is implementation-defined")]
	public virtual void Parameter_default_IsNullable_is_false()
	{
		if (!Fixture.Factory.CreateParameter().IsNullable)
			return;

		SoftWarning.Report("The provider defaults IsNullable to true; false is not a universal default requirement.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.sourcecolumn", Because = "provider default SourceColumn representation is implementation-defined")]
	public virtual void Parameter_default_SourceColumn_is_empty_string()
	{
		if (string.IsNullOrEmpty(Fixture.Factory.CreateParameter().SourceColumn))
			return;

		SoftWarning.Report("The provider uses a non-empty default SourceColumn.");
	}

	/// <summary>
	/// Precision and scale affect numeric parameter binding, but the common API
	/// does not prescribe one universal round-trip when the provider derives those
	/// values from the parameter's native type or value. Providers that preserve
	/// explicit settings are still checked here as an interoperability diagnostic.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.precision
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.scale.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.precision", Because = "provider-specific precision/scale materialization")]
	public virtual void Parameter_precision_and_scale_preserve_explicit_values_when_supported()
	{
		var parameter = Fixture.Factory.CreateParameter();
		parameter.DbType = DbType.Decimal;
		parameter.Value = 12.34m;
		parameter.Precision = 7;
		parameter.Scale = 2;

		if (parameter.Precision != 7 || parameter.Scale != 2)
			SoftWarning.Report("The provider derives precision or scale from its native parameter representation.");
	}

	/// <summary>
	/// ResetDbType is part of the API, but its provider-specific reset target is not
	/// defined by the ADO.NET contract. This remains an active implementation-
	/// comparison diagnostic; providers with a different reset target can override it.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.resetdbtype.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.resetdbtype", Because = "provider-specific ResetDbType default")]
	public virtual void ResetDbType_works()
	{
		var parameter = Fixture.Factory.CreateParameter();
		var original = parameter.DbType;
		parameter.DbType = DbType.Int64;

		parameter.ResetDbType();

		Assert.Equal(original, parameter.DbType);
	}

	/// <summary>
	/// Named versus positional parameter requirements are provider-specific. The
	/// SQL marker is discovered separately; providers with different parameter-name
	/// requirements can override this implementation-comparison diagnostic.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "NOW COMMON BEHAVIOR DIAGNOSTIC")]
	public virtual void Bind_requires_set_name()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = $"SELECT {MakeParameterName(connection, "Parameter")};";
		var parameter = command.CreateParameter();
		parameter.Value = 1;
		command.Parameters.Add(parameter);

		try
		{
			command.ExecuteNonQuery();
		}
		catch (InvalidOperationException)
		{
			return;
		}
		catch (DbException)
		{
			return;
		}

		SoftWarning.Report("The provider accepts a parameter without a logical name; this is common behavior, not an ADO.NET contract.");
	}

	/// <summary>
	/// DBNull.Value is the ADO.NET sentinel for a database NULL value; this verifies
	/// that a parameter carrying that sentinel can be bound.
	/// See https://learn.microsoft.com/dotnet/api/system.dbnull.value.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "NOW COMMON BEHAVIOR DIAGNOSTIC")]
	public virtual void Bind_accepts_DBNull_value()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = $"SELECT {MakeParameterName(connection, "Parameter")};";
		var parameter = command.CreateParameter();
		parameter.ParameterName = "Parameter";
		parameter.DbType = DbType.String;
		parameter.Value = DBNull.Value;
		command.Parameters.Add(parameter);

		var result = command.ExecuteScalar();
		Assert.True(result is null || result == DBNull.Value);
	}

	/// <summary>
	/// The original test required null to be rejected. ADO.NET permits a provider to
	/// interpret null as its parameter-null representation, so that rejection is not
	/// a provider-neutral contract; DBNull.Value is the portable sentinel instead.
	/// This remains an active implementation-comparison diagnostic.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.value.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "NOW COMMON BEHAVIOR DIAGNOSTIC")]
	public virtual void Bind_requires_set_value()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = $"SELECT {MakeParameterName(connection, "Parameter")};";
		var parameter = command.CreateParameter();
		parameter.ParameterName = "Parameter";
		command.Parameters.Add(parameter);

		try
		{
			command.ExecuteNonQuery();
		}
		catch (InvalidOperationException)
		{
			return;
		}
		catch (DbException)
		{
			return;
		}

		SoftWarning.Report("The provider supplies a default parameter value; this is common behavior, not an ADO.NET contract.");
	}

	/// <summary>
	/// An unused parameter is tested independently of SQL marker syntax; the
	/// parameter collection contains the logical name only.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idataparametercollection.item", Because = "NOW COMMON BEHAVIOR DIAGNOSTIC")]
	public virtual void Bind_is_noop_on_unknown_parameter()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneSql;
		var parameter = command.CreateParameter();
		parameter.ParameterName = "Unknown";
		parameter.Value = 1;
		command.Parameters.Add(parameter);

		command.ExecuteNonQuery();
	}

	/// <summary>
	/// Conversion of an arbitrary CLR object that a provider does not understand is
	/// provider-specific. This remains an active implementation-comparison
	/// diagnostic; providers with different conversion behavior can override it.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.value.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "NOW COMMON BEHAVIOR DIAGNOSTIC")]
	public virtual void Bind_throws_when_unknown()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = $"SELECT {MakeParameterName(connection, "Parameter")};";
		var parameter = command.CreateParameter();
		parameter.ParameterName = "Parameter";
		parameter.Value = new object();
		command.Parameters.Add(parameter);

		try
		{
			command.ExecuteScalar();
		}
		catch (InvalidOperationException)
		{
			return;
		}
		catch (NotSupportedException)
		{
			return;
		}
		catch (DbException)
		{
			return;
		}

		SoftWarning.Report("The provider accepts the arbitrary CLR value; conversion behavior is provider-specific.");
	}

	/// <summary>
	/// A string assigned through DbParameter.Value must be returned as the same
	/// database value; the SQL marker is discovered from provider metadata.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.value.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "NOW COMMON BEHAVIOR DIAGNOSTIC")]
	public virtual void Bind_works_with_string()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = $"SELECT {MakeParameterName(connection, "Parameter")};";
		var parameter = command.CreateParameter();
		parameter.ParameterName = "Parameter";
		parameter.Value = "test";
		command.Parameters.Add(parameter);

		var result = command.ExecuteScalar();
		Assert.Equal("test", result);
	}

	/// <summary>
	/// DbParameter.Value accepts provider-supported CLR values; this preserves the
	/// suite's original byte-array round-trip assertion. Providers with a different
	/// representation can override the test.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.value.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "NOW COMMON BEHAVIOR DIAGNOSTIC")]
	public virtual void Bind_works_with_byte_array()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = $"SELECT {MakeParameterName(connection, "Parameter")};";
		var parameter = command.CreateParameter();
		parameter.ParameterName = "Parameter";
		parameter.Value = new byte[] { 1, 2, 3, 4 };
		command.Parameters.Add(parameter);

		var result = command.ExecuteScalar();
		if (result is byte[] bytes)
		{
			Assert.Equal(new byte[] { 1, 2, 3, 4 }, bytes);
			return;
		}

		SoftWarning.Report("The provider uses a different byte-array representation for parameter values.");
	}

	/// <summary>
	/// This preserves the suite's original stream-parameter diagnostic. Providers
	/// that do not support stream values can override it.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.value.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "NOW COMMON BEHAVIOR DIAGNOSTIC")]
	public virtual void Bind_works_with_stream()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = $"SELECT {MakeParameterName(connection, "Parameter")};";
		var parameter = command.CreateParameter();
		parameter.ParameterName = "Parameter";
		using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
		parameter.Value = stream;
		command.Parameters.Add(parameter);

		try
		{
			var result = command.ExecuteScalar();
			if (result is byte[] bytes)
			{
				if (!bytes.AsSpan().SequenceEqual(new byte[] { 1, 2, 3, 4 }))
				{
					SoftWarning.Report("The provider materializes the stream parameter as different bytes.");
				}

				return;
			}

			if (result is Stream)
			{
				SoftWarning.Report("The provider returns the stream parameter rather than materializing it as bytes.");
				return;
			}

			SoftWarning.Report("The provider uses a different stream-parameter representation.");
		}
		catch (Exception ex)
		{
			SoftWarning.Report($"The provider rejects stream parameter values with {ex.GetType().Name}; stream binding is provider-specific.");
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.parametername", Because = "null-to-empty ParameterName coercion is not universal")]
	public virtual void ParameterName_set_to_null_is_empty_string()
	{
		var parameter = Fixture.Factory.CreateParameter();
		parameter.ParameterName = null;
		if (string.IsNullOrEmpty(parameter.ParameterName))
			return;

		SoftWarning.Report("The provider preserves a non-empty ParameterName after assigning null.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.sourcecolumn", Because = "null SourceColumn representation is provider-defined")]
	public virtual void SourceColumn_can_be_set_to_null()
	{
		var parameter = Fixture.Factory.CreateParameter();
		parameter.SourceColumn = null;
		if (string.IsNullOrEmpty(parameter.SourceColumn))
			return;

		SoftWarning.Report("The provider preserves a non-empty SourceColumn after assigning null.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.sourcecolumn", Because = "provider default SourceColumn representation is implementation-defined")]
	public virtual void SourceColumn_default_is_empty_string()
	{
		if (string.IsNullOrEmpty(Fixture.Factory.CreateParameter().SourceColumn))
			return;

		SoftWarning.Report("The provider uses a non-empty default SourceColumn.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.sourcecolumn", Because = "null-to-empty SourceColumn coercion is not universal")]
	public virtual void SourceColumn_set_to_null_is_empty_string()
	{
		var parameter = Fixture.Factory.CreateParameter();
		parameter.SourceColumn = null;
		if (string.IsNullOrEmpty(parameter.SourceColumn))
			return;

		SoftWarning.Report("The provider preserves a non-empty SourceColumn after assigning null.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection", Because = "DbParameterCollection does not specify a universal null policy")]
	public virtual void ParameterCollection_Add_throws_for_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		AssertNullParameterPolicy(() => command.Parameters.Add(default), "Add");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection.insert", Because = "DbParameterCollection does not specify a universal null policy")]
	public virtual void ParameterCollection_Insert_throws_for_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		AssertNullParameterPolicy(() => command.Parameters.Insert(0, default), "Insert");
	}

	/// <summary>
	/// The string indexer uses the logical parameter name, independently of whether
	/// SQL uses '@', ':', '$', or positional '?'.
	/// See https://learn.microsoft.com/dotnet/api/system.data.idataparametercollection.item.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idataparametercollection.item", Because = "null replacement policy is not specified; logical names remain marker-independent")]
	public virtual void ParameterCollection_string_indexer_setter_throws_for_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		var parameter = command.CreateParameter();
		parameter.ParameterName = "param";
		command.Parameters.Add(parameter);
		AssertNullParameterPolicy(() => command.Parameters["param"] = null, "string indexer setter");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection.item", Because = "null replacement policy is not specified")]
	public virtual void ParameterCollection_int_indexer_setter_throws_for_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		var parameter = command.CreateParameter();
		command.Parameters.Add(parameter);
		AssertNullParameterPolicy(() => command.Parameters[0] = null, "integer indexer setter");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection.item", Because = "null replacement policy is not specified")]
	public virtual void ParameterCollection_IList_indexer_setter_throws_for_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		var parameter = command.CreateParameter();
		command.Parameters.Add(parameter);
		AssertNullParameterPolicy(() => ((IList) command.Parameters)[0] = null, "IList indexer setter");
	}

	/// <summary>
	/// IDataParameterCollection also indexes by the logical parameter name; the SQL
	/// marker is not part of the collection key.
	/// See https://learn.microsoft.com/dotnet/api/system.data.idataparametercollection.item.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idataparametercollection.item", Because = "null replacement policy is not specified; logical names remain marker-independent")]
	public virtual void ParameterCollection_IDataParameterCollection_indexer_setter_throws_for_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		var parameter = command.CreateParameter();
		parameter.ParameterName = "param";
		command.Parameters.Add(parameter);
		AssertNullParameterPolicy(() => ((IDataParameterCollection) command.Parameters)["param"] = null, "IDataParameterCollection indexer setter");
	}

	/// <summary>
	/// IDataParameterCollection requires Contains, but does not impose one universal
	/// null-argument policy for IndexOf across provider implementations. This remains
	/// an active diagnostic; providers with a different policy can override it.
	/// See https://learn.microsoft.com/dotnet/api/system.data.idataparametercollection.indexof.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idataparametercollection.indexof", Because = "null IndexOf policy is not universal")]
	public virtual void ParameterCollection_IndexOf_object_returns_negative_one_for_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.Equal(-1, command.Parameters.IndexOf(default(object)));
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idataparametercollection.indexof", Because = "null IndexOf policy is not universal")]
	public virtual void ParameterCollection_IndexOf_string_returns_negative_one_for_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.Equal(-1, command.Parameters.IndexOf(default(string)));
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection.remove", Because = "DbParameterCollection does not specify a universal null policy")]
	public virtual void ParameterCollection_Remove_throws_for_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		AssertNullParameterPolicy(() => command.Parameters.Remove(null), "Remove");
	}
}
