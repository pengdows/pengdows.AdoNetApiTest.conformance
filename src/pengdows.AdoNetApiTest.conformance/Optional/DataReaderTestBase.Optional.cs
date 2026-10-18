using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

public partial class DataReaderTestBase<TFixture>
	where TFixture : class, ISelectValueFixture, IDeleteFixture
{
#if NET10_0_OR_GREATER
	/// <summary>
	/// The asynchronous schema API must expose the same column metadata as the
	/// synchronous API. Providers without schema-table support may return null.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getschematableasync.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getschematableasync")]
	public virtual async Task GetSchemaTableAsync_returns_metadata_when_supported()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneAsIdSql;
		using var reader = command.ExecuteReader();
		var schema = await reader.GetSchemaTableAsync().ConfigureAwait(false);

		if (schema is null)
			throw SkipException.ForSkip("The provider does not expose reader schema tables asynchronously.");

		Assert.NotEmpty(schema.Columns);
		Assert.NotEmpty(schema.Rows);
	}
#endif
}
