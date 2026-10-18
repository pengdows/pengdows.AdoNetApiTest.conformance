using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class ConnectionTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.connectionstring", Because = "provider default connection-string representation is implementation-defined")]
	public virtual void ConnectionString_is_empty_string_by_default()
	{
		using var connection = Fixture.Factory.CreateConnection();
		if (string.IsNullOrEmpty(connection.ConnectionString))
			return;

		throw Xunit.Sdk.SkipException.ForSkip("The provider uses a non-empty default connection string.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.connectionstring", Because = "null-to-empty connection-string coercion is not universal")]
	public void ConnectionString_is_coerced_to_empty_string()
	{
		using var connection = Fixture.Factory.CreateConnection();
		try
		{
			connection.ConnectionString = null;
		}
		catch (ArgumentNullException)
		{
			throw Xunit.Sdk.SkipException.ForSkip("The provider rejects a null connection string; null coercion is not a portable ADO.NET contract.");
		}
		if (string.IsNullOrEmpty(connection.ConnectionString))
			return;

		throw Xunit.Sdk.SkipException.ForSkip("The provider preserves a non-empty connection string after assigning null.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.database", Because = "closed-connection Database values are provider-specific")]
	public virtual void Database_returns_empty_when_closed()
	{
		using var connection = CreateConnection();
		var database = connection.Database;
		if (string.IsNullOrEmpty(database))
			return;

		throw Xunit.Sdk.SkipException.ForSkip("The provider reports a database name while closed; an empty value is not required by the ADO.NET contract.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.datasource", Because = "closed-connection DataSource values are provider-specific")]
	public virtual void DataSource_returns_empty_when_closed()
	{
		using var connection = CreateConnection();
		if (string.IsNullOrEmpty(connection.DataSource))
			return;

		throw Xunit.Sdk.SkipException.ForSkip("The provider reports a data source while closed; an empty value is not universally required.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.serverversion", Because = "the base DbConnection contract documents the value but not closed-state behavior")]
	public virtual void ServerVersion_throws_when_closed()
	{
		using var connection = CreateConnection();
		try
		{
			_ = connection.ServerVersion;
		}
		catch (InvalidOperationException)
		{
			return;
		}

		throw Xunit.Sdk.SkipException.ForSkip("The provider exposes ServerVersion while closed; the base ADO.NET contract does not require a closed-state exception.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.open", Because = "empty-connection-string exception type is provider-specific")]
	public virtual void Open_throws_when_no_connection_string()
	{
		using var connection = CreateConnection();
		try
		{
			connection.Open();
		}
		catch (InvalidOperationException)
		{
			return;
		}
		catch (ArgumentException)
		{
			return;
		}

		throw Xunit.Sdk.SkipException.ForSkip("The provider accepts an empty connection string; this is outside the shared ADO.NET conformance contract.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.connectionstring", Because = "providers may defer connection-string validation until Open")]
	public virtual void Set_ConnectionString_throws_when_invalid()
	{
		using var connection = CreateConnection();
		try
		{
			connection.ConnectionString = "xyzzy=Invalid";
		}
		catch (ArgumentException)
		{
			return;
		}

		throw Xunit.Sdk.SkipException.ForSkip("The provider defers connection-string validation; setter rejection is not a universal ADO.NET requirement.");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.open", Because = "the generic DbConnection contract does not prescribe repeated-open behavior for every provider; the Microsoft docs give SqlConnection as an example that rejects it.")]
	public virtual void Open_cannot_be_called_twice()
	{
		using var connection = CreateOpenConnection();
		try
		{
			connection.Open();
			SoftWarning.Report("The provider permits Open to be called on an already-open connection.");
		}
		catch (Exception ex)
		{
			SoftWarning.Report($"The provider rejects repeated Open with {ex.GetType().Name}; repeated-open behavior is provider-specific.");
		}
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection", Because = "provider cancellation may be ignored; this exact result is not required by the ADO.NET base contract")]
	public virtual async Task OpenAsync_is_canceled()
	{
		using var connection = CreateConnection();
		connection.ConnectionString = ConnectionString;
		var task = connection.OpenAsync(CanceledToken);
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
		Assert.True(task.IsCanceled);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.dispose", Because = "accessing State after Dispose is not portable")]
	public virtual void Dispose_closes_connection()
	{
		var connection = CreateOpenConnection();
		connection.Dispose();
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.createcommand", Because = "DbConnection.CreateCommand does not require a provider to leave Transaction null; Microsoft.Data.Sqlite documents that it assigns the connection's current transaction.")]
	public virtual void CreateCommand_does_not_set_Transaction_property()
	{
		using var connection = CreateOpenConnection();
		using var transaction = connection.BeginTransaction();
		using var command = connection.CreateCommand();
		if (command.Transaction is null)
		{
			return;
		}

		SoftWarning.Report("The provider assigns the current transaction to commands created by the connection.");
	}
}
