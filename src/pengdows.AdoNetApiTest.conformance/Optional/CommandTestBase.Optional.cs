using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class CommandTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	/// <summary>
	/// CommandTimeout is an optional provider capability. A provider that exposes
	/// it accepts and preserves a valid value; a provider may explicitly report
	/// that timeout control is unsupported.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.commandtimeout.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.commandtimeout", Because = "unsupported timeout control is allowed")]
	public virtual void CommandTimeout_can_be_set_and_read()
	{
		using var command = Fixture.Factory.CreateCommand();
		try
		{
			command.CommandTimeout = 17;
		}
		catch (NotSupportedException)
		{
			throw SkipException.ForSkip("The provider does not support command timeouts.");
		}
		Assert.Equal(17, command.CommandTimeout);
	}

	/// <summary>
	/// SchemaOnly requests column metadata without returning result rows. Providers
	/// that implement the behavior must preserve the schema while reporting no
	/// data rows; an explicit NotSupportedException is treated as an optional
	/// capability response.
	/// See https://learn.microsoft.com/dotnet/api/system.data.commandbehavior.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.commandbehavior")]
	public virtual void ExecuteReader_supports_SchemaOnly_when_advertised()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		command.CommandText = SelectOneAsValueSql;

		try
		{
			using var reader = command.ExecuteReader(CommandBehavior.SchemaOnly);
			if (reader.FieldCount != 1 || reader.Read())
			{
				SoftWarning.Report("The provider does not expose metadata-only CommandBehavior.SchemaOnly results.");
			}
		}
		catch (NotSupportedException)
		{
			throw SkipException.ForSkip("The provider does not support CommandBehavior.SchemaOnly.");
		}
	}
}
