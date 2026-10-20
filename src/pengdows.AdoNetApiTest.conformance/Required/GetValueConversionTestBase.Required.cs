using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class GetValueConversionTestBase<TFixture>
	where TFixture : class, ISelectValueFixture
{

	/// <summary>
	/// GetFieldType describes the CLR type returned by GetValue for the current
	/// column. Provider-specific native value types are valid; reporting a type
	/// that GetValue cannot return is not.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldtype
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getvalue.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldtype")]
	public virtual void GetFieldType_matches_GetValue_for_supported_values()
	{
		foreach (var dbType in Fixture.SupportedDbTypes)
		{
			using var connection = CreateOpenConnection();
			using var command = connection.CreateCommand();
			command.CommandText = Fixture.CreateSelectSql(dbType, ValueKind.One);
			using var reader = command.ExecuteReader();

			Assert.True(reader.Read(), $"{dbType}: the value row is missing.");
			var value = reader.GetValue(0);
			Assert.NotSame(DBNull.Value, value);
			// IDataRecord.GetFieldType: "the type of Object that would be returned from GetValue" (a base type is allowed).
			Assert.True(reader.GetFieldType(0).IsInstanceOfType(value),
				$"{dbType}: GetFieldType returns {reader.GetFieldType(0)}, but GetValue returns {value.GetType()}.");
		}
	}
}
