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

		ConformanceOutcome.Different("The provider uses a non-empty default connection string.");
		return;
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
			ConformanceOutcome.Different("The provider rejects a null connection string; null coercion is not a portable ADO.NET contract.");
			return;
		}
		if (string.IsNullOrEmpty(connection.ConnectionString))
			return;

		ConformanceOutcome.Different("The provider preserves a non-empty connection string after assigning null.");
		return;
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.database", Because = "closed-connection Database values are provider-specific")]
	public virtual void Database_returns_empty_when_closed()
	{
		using var connection = CreateConnection();
		var database = connection.Database;
		if (string.IsNullOrEmpty(database))
			return;

		ConformanceOutcome.Different("The provider reports a database name while closed; an empty value is not required by the ADO.NET contract.");
		return;
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.datasource", Because = "closed-connection DataSource values are provider-specific")]
	public virtual void DataSource_returns_empty_when_closed()
	{
		using var connection = CreateConnection();
		if (string.IsNullOrEmpty(connection.DataSource))
			return;

		ConformanceOutcome.Different("The provider reports a data source while closed; an empty value is not universally required.");
		return;
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

		ConformanceOutcome.Different("The provider exposes ServerVersion while closed; the base ADO.NET contract does not require a closed-state exception.");
		return;
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

		ConformanceOutcome.Different("The provider accepts an empty connection string; this is outside the shared ADO.NET conformance contract.");
		return;
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

		ConformanceOutcome.Different("The provider defers connection-string validation; setter rejection is not a universal ADO.NET requirement.");
		return;
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

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection", Because = "The documented default is an empty string; no page says an empty string must be accepted when assigned.")]
	public void ConnectionString_can_be_set_to_empty_string()
	{
		using var connection = Fixture.Factory.CreateConnection();
		connection.ConnectionString = "";
		Assert.Equal("", connection.ConnectionString);
	}

	/// <summary>
	/// ConnectionTimeout is expressed in seconds and must never be negative.
	/// Providers may choose the actual timeout value, but they must expose a valid
	/// value through the common connection contract.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.connectiontimeout.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.connectiontimeout", Because = "No page states a range for ConnectionTimeout.")]
	public virtual void ConnectionTimeout_is_non_negative()
	{
		using var connection = CreateOpenConnection();
		Assert.True(connection.ConnectionTimeout >= 0);
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection", Because = "Close may be called more than once without an exception; no page covers Close before Open.")]
	public virtual void Close_can_be_called_before_open()
	{
		using var connection = CreateConnection();
		connection.Close();
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection", Because = "Only IDbConnection says it can be set only while closed; no page names the exception.")]
	public virtual void ConnectionString_setter_throws_when_open()
	{
		using var connection = CreateOpenConnection();
		Assert.ThrowsAny<InvalidOperationException>(() => connection.ConnectionString = ConnectionString + ";");
	}

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection", Because = "Reads the protected DbConnection.DbProviderFactory by reflection.")]
	public virtual void DbProviderFactory_has_correct_value()
	{
		using var connection = CreateConnection();
		var property = connection.GetType().GetProperty("DbProviderFactory", BindingFlags.Instance | BindingFlags.NonPublic);
		var dbProviderFactory = property.GetValue(connection);
		Assert.Same(Fixture.Factory, dbProviderFactory);
	}
}
