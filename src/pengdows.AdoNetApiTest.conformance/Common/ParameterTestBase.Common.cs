using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
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
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types")]
	public virtual void Bind_requires_set_name()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
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
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types")]
	public virtual void Bind_accepts_DBNull_value()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
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
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types")]
	public virtual void Bind_requires_set_value()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
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
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.idataparametercollection.item")]
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
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types")]
	public virtual void Bind_throws_when_unknown()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
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
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types")]
	public virtual void Bind_works_with_string()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
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
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types")]
	public virtual void Bind_works_with_byte_array()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
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
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types")]
	public virtual void Bind_works_with_stream()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
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

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter", Because = "DbParameter documents no exception or result for Value = null.")]
	public virtual void Parameter_Value_can_be_set_to_null()
	{
		var parameter = Fixture.Factory.CreateParameter();
		parameter.Value = null;
		Assert.True(parameter.Value is null or DBNull);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter", Because = "DbParameter documents no exception or result for ParameterName = null.")]
	public virtual void ParameterName_can_be_set_to_null()
	{
		Fixture.Factory.CreateParameter().ParameterName = null;
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter", Because = "No page documents the result of Contains(null).")]
	public virtual void ParameterCollection_Contains_object_returns_false_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.False(command.Parameters.Contains(default(object)));
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter", Because = "No page documents the result of Contains(null).")]
	public virtual void ParameterCollection_Contains_string_returns_false_for_null()
	{
		using var command = Fixture.Factory.CreateCommand();
		Assert.False(command.Parameters.Contains(default(string)));
	}

	/// <summary>
	/// AddRange must add each supplied parameter in order, and CopyTo must expose
	/// the same collection order through the standard collection API.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection.addrange
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection.copyto.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection", Because = "No page states the order AddRange or CopyTo produce.")]
	public virtual void ParameterCollection_AddRange_and_CopyTo_preserve_order()
	{
		using var command = Fixture.Factory.CreateCommand();
		var first = command.CreateParameter();
		first.ParameterName = "first";
		var second = command.CreateParameter();
		second.ParameterName = "second";

		command.Parameters.AddRange(new[] { first, second });

		Assert.Equal(2, command.Parameters.Count);
		Assert.Same(first, command.Parameters[0]);
		Assert.Same(second, command.Parameters[1]);

		var copy = Array.CreateInstance(first.GetType(), 2);
		command.Parameters.CopyTo(copy, 0);
		Assert.Same(first, copy.GetValue(0));
		Assert.Same(second, copy.GetValue(1));
	}

	/// <summary>
	/// The parameter collection implements the collection enumeration contract
	/// and enumerates its parameters in collection order.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection.getenumerator.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparametercollection.getenumerator", Because = "No page states the order the enumerator follows.")]
	public virtual void ParameterCollection_GetEnumerator_preserves_order()
	{
		using var command = Fixture.Factory.CreateCommand();
		var first = command.CreateParameter();
		var second = command.CreateParameter();
		command.Parameters.Add(first);
		command.Parameters.Add(second);

		var enumerator = command.Parameters.GetEnumerator();
		Assert.True(enumerator.MoveNext());
		Assert.Same(first, enumerator.Current);
		Assert.True(enumerator.MoveNext());
		Assert.Same(second, enumerator.Current);
		Assert.False(enumerator.MoveNext());
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter", Because = "The documentation says the default size is inferred from the value, not 0.")]
	public virtual void Parameter_default_Size_is_zero()
	{
		Assert.Equal(0, Fixture.Factory.CreateParameter().Size);
	}

	/// <summary>
	/// A value that does not fit the declared <see cref="DbType"/> is refused or sent as given, never changed to fit:
	/// System.Data.SQLite 1.0.119 sent 1.5 declared Int32 as 2.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.dbtype.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.dbtype", Because = "No page says whether a provider converts a value to its DbType or refuses it.")]
	public virtual void Bind_does_not_round_a_value_to_its_DbType()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
		var parameter = command.CreateParameter();
		parameter.ParameterName = "Parameter";
		parameter.DbType = DbType.Int32;
		parameter.Value = 1.5m;
		command.Parameters.Add(parameter);

		object result;
		try
		{
			result = command.ExecuteScalar();
		}
		catch (Exception exception) when (!(exception is Xunit.Sdk.XunitException))
		{
			ConformanceOutcome.Different("The provider refused a value that does not fit the declared DbType: " + exception.GetType().Name + ": " + FirstLine(exception.Message));
			return;
		}

		Assert.Equal(1.5m, Convert.ToDecimal(result, System.Globalization.CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// Every <see cref="DbType"/> member can be assigned. ODP.NET rejects Guid, DateTime2 and Xml, IBM Db2 rejects Guid and
	/// FirebirdClient rejects VarNumeric the moment they are assigned, which pengdows.crud works around by remapping them.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.dbtype.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.dbtype", Because = "the page does not say which DbType values a provider must accept")]
	public virtual void DbType_accepts_every_DbType_value()
	{
		var rejected = new List<string>();
		foreach (DbType type in Enum.GetValues(typeof(DbType)))
		{
			var parameter = Fixture.Factory.CreateParameter();
			try
			{
				parameter.DbType = type;
			}
			catch (Exception ex)
			{
				rejected.Add($"{type} ({ex.GetType().Name})");
			}
		}

		if (rejected.Count > 0)
			ConformanceOutcome.Different("DbType rejects: " + string.Join(", ", rejected) + ".");
	}

	/// <summary>
	/// A NULL value binds under every DbType the provider accepts; IBM Db2 rejected DbType.DateTimeOffset even for NULL.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "no page says a NULL must bind under every DbType")]
	public virtual void Bind_works_with_DBNull_for_every_DbType()
	{
		using var connection = CreateOpenConnection();
		var failed = new List<string>();
		foreach (DbType type in Enum.GetValues(typeof(DbType)))
		{
			using var command = connection.CreateCommand();
			command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
			var parameter = command.CreateParameter();
			parameter.ParameterName = "Parameter";
			try
			{
				parameter.DbType = type;
			}
			catch (Exception)
			{
				// DbType_accepts_every_DbType_value reports the setter.
				continue;
			}

			parameter.Value = DBNull.Value;
			command.Parameters.Add(parameter);
			try
			{
				var result = command.ExecuteScalar();
				if (result is not null && result is not DBNull)
					failed.Add($"{type} (returned {result})");
			}
			catch (Exception ex)
			{
				failed.Add($"{type} ({ex.GetType().Name}: {FirstLine(ex.Message)})");
			}
		}

		if (failed.Count > 0)
			ConformanceOutcome.Different("A DBNull parameter did not come back as NULL for: " + string.Join("; ", failed));
	}

	/// <summary>
	/// A parameter reused with a wider decimal sends the new value in full. Given a first value's smaller Precision and
	/// Scale, SAP HANA refused a larger value or cut it short silently; pengdows.crud widens them itself.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.value.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.value", Because = "no page says how Precision and Scale follow a new Value")]
	public virtual void Reused_decimal_parameter_sends_a_wider_value_exactly()
	{
		using var connection = CreateOpenConnection();
		var lost = new List<string>();
		foreach (var prepare in new[] { false, true })
		{
			using var command = connection.CreateCommand();
			command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
			var parameter = command.CreateParameter();
			parameter.ParameterName = "Parameter";
			parameter.DbType = DbType.Decimal;
			parameter.Value = 1.5m;
			command.Parameters.Add(parameter);
			var label = prepare ? "prepared" : "unprepared";
			try
			{
				if (prepare)
				{
					try
					{
						command.Prepare();
					}
					catch (NotSupportedException)
					{
						continue;
					}
				}

				foreach (var value in new[] { 1.5m, 12345678.0625m, 0.000001m, -98765.4321m })
				{
					parameter.Value = value;
					var result = command.ExecuteScalar();
					var actual = result is string text ? decimal.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture) : Convert.ToDecimal(result, CultureInfo.InvariantCulture);
					if (actual != value)
						lost.Add($"{label}: sent {value.ToString(CultureInfo.InvariantCulture)} after 1.5, got {actual.ToString(CultureInfo.InvariantCulture)}");
				}
			}
			catch (Exception ex)
			{
				lost.Add($"{label}: {ex.GetType().Name}: {FirstLine(ex.Message)}");
			}
		}

		if (lost.Count > 0)
			ConformanceOutcome.Different("A reused decimal parameter lost its new value: " + string.Join("; ", lost));
	}

	/// <summary>
	/// A double outside the range of a decimal NUMBER binds exactly. ODP.NET bound DbType.Double as NUMBER, so a value
	/// beyond 1e126 overflowed; pengdows.crud binds it as BINARY_DOUBLE.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "no page fixes the range a provider binds a double with")]
	public virtual void Bind_works_with_large_double()
	{
		using var connection = CreateOpenConnection();
		var lost = new List<string>();
		foreach (var value in new[] { 1e200, -1e200, double.MaxValue, double.Epsilon })
		{
			using var command = connection.CreateCommand();
			command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
			var parameter = command.CreateParameter();
			parameter.ParameterName = "Parameter";
			parameter.DbType = DbType.Double;
			parameter.Value = value;
			command.Parameters.Add(parameter);
			var shown = value.ToString("R", CultureInfo.InvariantCulture);
			try
			{
				var result = command.ExecuteScalar();
				var actual = result is string text ? double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture) : Convert.ToDouble(result, CultureInfo.InvariantCulture);
				if (BitConverter.DoubleToInt64Bits(actual) != BitConverter.DoubleToInt64Bits(value))
					lost.Add($"{shown} came back as {actual.ToString("R", CultureInfo.InvariantCulture)}");
			}
			catch (Exception ex)
			{
				lost.Add($"{shown}: {ex.GetType().Name}: {FirstLine(ex.Message)}");
			}
		}

		if (lost.Count > 0)
			ConformanceOutcome.Different("A double parameter did not come back exactly: " + string.Join("; ", lost));
	}

	/// <summary>
	/// A DateTimeOffset with a non-zero offset binds and keeps its instant. Npgsql 6 and later reject it for timestamptz
	/// ("Offset" must be 0), which pengdows.crud works around by converting to UTC first.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "no page says how a provider binds a DateTimeOffset's offset")]
	public virtual void Bind_works_with_DateTimeOffset_with_offset()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
		var parameter = command.CreateParameter();
		parameter.ParameterName = "Parameter";
		parameter.DbType = DbType.DateTimeOffset;
		var value = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.FromHours(5));
		parameter.Value = value;
		command.Parameters.Add(parameter);

		object result;
		try
		{
			result = command.ExecuteScalar();
		}
		catch (Exception ex)
		{
			ConformanceOutcome.Different($"Binding {value:o} threw {ex.GetType().Name}: {FirstLine(ex.Message)}");
			return;
		}

		var instant = result switch
		{
			DateTimeOffset offset => offset.UtcTicks,
			string text when DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) => parsed.UtcTicks,
			DateTime { Kind: DateTimeKind.Utc } utc => utc.Ticks,
			_ => (long?) null,
		};
		if (instant is null)
			ConformanceOutcome.Different($"Binding {value:o} came back as {result?.GetType().Name} {result}, which does not say which instant it is.");
		else if (instant != value.UtcTicks)
			ConformanceOutcome.Different($"Binding {value:o} came back as {result}, a different instant.");
	}

	/// <summary>
	/// A TimeSpan binds as DbType.Time. SingleStore rejected the typed TIME MySqlConnector sends and Spanner has no TIME, so
	/// pengdows.crud binds a time of day as text there.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "the table maps TimeSpan to DbType.Time for the .NET Framework providers only")]
	public virtual void Bind_works_with_TimeSpan_as_Time()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
		var parameter = command.CreateParameter();
		parameter.ParameterName = "Parameter";
		parameter.DbType = DbType.Time;
		var value = new TimeSpan(13, 45, 30);
		parameter.Value = value;
		command.Parameters.Add(parameter);

		object result;
		try
		{
			result = command.ExecuteScalar();
		}
		catch (Exception ex)
		{
			ConformanceOutcome.Different($"Binding {value} as DbType.Time threw {ex.GetType().Name}: {FirstLine(ex.Message)}");
			return;
		}

		var time = result switch
		{
			TimeSpan span => span,
			DateTime date => date.TimeOfDay,
			string text when TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var parsed) => parsed,
			_ => (TimeSpan?) null,
		};
		if (time != value)
			ConformanceOutcome.Different($"Binding {value} as DbType.Time came back as {result?.GetType().Name} {result}.");
	}

	/// <summary>
	/// Setting only Value infers DbType as the table shows (Int32 for an int, Guid for a Guid, Binary for a byte array).
	/// pengdows.crud copies DbType when it clones a parameter, which broke when ODP.NET reported Int64 for a parameter
	/// carrying text.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "the inference table describes the .NET Framework providers")]
	public virtual void DbType_is_inferred_from_Value()
	{
		var cases = new (object Value, DbType Expected)[]
		{
			(true, DbType.Boolean), ((byte) 1, DbType.Byte), (new byte[] { 1 }, DbType.Binary), (new DateTime(2024, 1, 1), DbType.DateTime),
			(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), DbType.DateTimeOffset), (1.5m, DbType.Decimal), (1.5d, DbType.Double),
			(1.5f, DbType.Single), (Guid.Empty, DbType.Guid), ((short) 1, DbType.Int16), (1, DbType.Int32), (1L, DbType.Int64),
			("text", DbType.String), (TimeSpan.FromHours(1), DbType.Time),
		};
		var differ = new List<string>();
		foreach (var (value, expected) in cases)
		{
			var parameter = Fixture.Factory.CreateParameter();
			try
			{
				parameter.Value = value;
				if (parameter.DbType != expected)
					differ.Add($"{value.GetType().Name} gives {parameter.DbType}, not {expected}");
			}
			catch (Exception ex)
			{
				differ.Add($"{value.GetType().Name} throws {ex.GetType().Name}");
			}
		}

		if (differ.Count > 0)
			ConformanceOutcome.Different("DbType inferred from Value: " + string.Join("; ", differ) + ".");
	}

	/// <summary>
	/// A DbType set before Value stays set: DuckDB.NET 1.5.6.1 replaced Int64 with Int32 for an int and Time with Object
	/// for a TimeSpan. pengdows.crud's dialects set DbType and then Value, so the type they chose was lost.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "the page says the type is inferred from Value or from DbType, not which wins when both are set")]
	public virtual void Value_does_not_replace_an_explicit_DbType()
	{
		var cases = new (DbType DbType, object Value)[]
		{
			(DbType.Int64, 5), (DbType.Time, new TimeSpan(13, 45, 30)), (DbType.Decimal, 1.5d), (DbType.String, 5),
		};
		var differ = new List<string>();
		foreach (var (dbType, value) in cases)
		{
			var parameter = Fixture.Factory.CreateParameter();
			try
			{
				parameter.DbType = dbType;
				parameter.Value = value;
				if (parameter.DbType != dbType)
					differ.Add($"{dbType} then {value.GetType().Name} gives {parameter.DbType}");
			}
			catch (Exception ex)
			{
				differ.Add($"{dbType} then {value.GetType().Name} throws {ex.GetType().Name}");
			}
		}

		if (differ.Count > 0)
			ConformanceOutcome.Different("Setting Value replaced the DbType set before it: " + string.Join("; ", differ) + ".");
	}

	/// <summary>
	/// ResetDbType on a parameter with a Value goes back to the DbType inferred from that Value, as the type it had before
	/// DbType was set; DuckDB.NET 1.5.6.1 went back to String for an int.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.resetdbtype.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.resetdbtype", Because = "the page says \"its original settings\" without saying whether those follow Value")]
	public virtual void ResetDbType_infers_DbType_from_Value()
	{
		var parameter = Fixture.Factory.CreateParameter();
		parameter.Value = 5;
		var inferred = parameter.DbType;
		parameter.DbType = DbType.Time;

		parameter.ResetDbType();

		if (parameter.DbType != inferred)
			ConformanceOutcome.Different($"ResetDbType with an int Value gives {parameter.DbType}, not {inferred} as before DbType was set.");
	}

	/// <summary>
	/// A value bound under its own DbType comes back unchanged whatever the current culture, with DbType set before or after
	/// Value. With Value set first, DuckDB.NET 1.5.6.1 sent a decimal declared Decimal and a DateTime declared DateTime2 as
	/// text where the statement gives the marker no type, and threw for a DateTime declared Date; System.Data.SQLite 1.0.119
	/// sent a DateTime declared DateTime2 as text in the current culture's format. Text is accepted when it reads back
	/// invariantly as the same value, as SQLite stores these.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types", Because = "no page says a bound value may not depend on the current culture")]
	public virtual void Bind_works_with_values_of_their_own_DbType_in_any_culture()
	{
		var when = new DateTime(2026, 10, 18, 13, 45, 30);
		var cases = new (DbType DbType, object Value, Func<object, bool> Same)[]
		{
			(DbType.Decimal, 1.5m, r => AsDecimal(r) == 1.5m),
			(DbType.Double, 1.5d, r => AsDecimal(r) == 1.5m),
			(DbType.Single, 1.5f, r => AsDecimal(r) == 1.5m),
			(DbType.Date, when.Date, r => AsDateTime(r)?.Date == when.Date),
			(DbType.DateTime, when, r => AsDateTime(r) == when),
			(DbType.DateTime2, when, r => AsDateTime(r) == when),
		};

		var culture = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
		var lost = new List<string>();
		try
		{
			using var connection = CreateOpenConnection();
			foreach (var (dbType, value, same) in cases)
			foreach (var valueFirst in new[] { false, true })
			{
				using var command = connection.CreateCommand();
				command.CommandText = SelectSql(MakeParameterName(connection, "Parameter"));
				var parameter = command.CreateParameter();
				parameter.ParameterName = "Parameter";
				if (valueFirst)
					parameter.Value = value;
				parameter.DbType = dbType;
				parameter.Value = value;
				command.Parameters.Add(parameter);
				var shown = $"{value.GetType().Name} {Convert.ToString(value, CultureInfo.InvariantCulture)} as {dbType}{(valueFirst ? " (Value set first)" : "")}";
				try
				{
					var result = command.ExecuteScalar();
					if (!same(result))
						lost.Add($"{shown} came back as {result?.GetType().Name} {Convert.ToString(result, CultureInfo.InvariantCulture)}");
				}
				catch (Exception ex)
				{
					lost.Add($"{shown}: {ex.GetType().Name}: {FirstLine(ex.Message)}");
				}
			}
		}
		finally
		{
			CultureInfo.CurrentCulture = culture;
		}

		if (lost.Count > 0)
			ConformanceOutcome.Different("A value bound under its own DbType (current culture de-DE) did not come back unchanged: " + string.Join("; ", lost));

		static decimal? AsDecimal(object result) => result switch
		{
			string text => decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : null,
			IConvertible number when result is not DateTime and not bool => Convert.ToDecimal(number, CultureInfo.InvariantCulture),
			_ => null,
		};

		static DateTime? AsDateTime(object result) => result switch
		{
			DateTime date => date,
#if NET6_0_OR_GREATER
			DateOnly date => date.ToDateTime(TimeOnly.MinValue),
#endif
			DateTimeOffset offset => offset.DateTime,
			string text => DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null,
			_ => null,
		};
	}

	private static string FirstLine(string message)
	{
		var end = message.IndexOfAny(new[] { '\r', '\n' });
		return end < 0 ? message : message.Substring(0, end);
	}
}
