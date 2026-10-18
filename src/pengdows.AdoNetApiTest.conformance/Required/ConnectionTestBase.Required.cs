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
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public void ConnectionString_can_be_set_to_empty_string()
	{
		using var connection = Fixture.Factory.CreateConnection();
		connection.ConnectionString = "";
		Assert.Equal("", connection.ConnectionString);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void ConnectionString_setter_throws_when_open()
	{
		using var connection = CreateOpenConnection();
		Assert.Throws<InvalidOperationException>(() => connection.ConnectionString = ConnectionString + ";");
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void ConnectionString_gets_and_sets_value()
	{
		using var connection = CreateConnection();
		connection.ConnectionString = ConnectionString;
		Assert.Equal(ConnectionString, connection.ConnectionString);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void Database_returns_value()
	{
		using var connection = CreateOpenConnection();
		Assert.NotNull(connection.Database);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void DataSource_returns_value()
	{
		using var connection = CreateOpenConnection();
		Assert.NotNull(connection.DataSource);
	}

	/// <summary>
	/// ConnectionTimeout is expressed in seconds and must never be negative.
	/// Providers may choose the actual timeout value, but they must expose a valid
	/// value through the common connection contract.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.connectiontimeout.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.connectiontimeout")]
	public virtual void ConnectionTimeout_is_non_negative()
	{
		using var connection = CreateOpenConnection();
		Assert.True(connection.ConnectionTimeout >= 0);
	}

	/// <summary>
	/// An open connection must expose a non-empty server-version value; this is
	/// metadata required by consumers that select provider-specific SQL behavior.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.serverversion.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void ServerVersion_returns_value()
	{
		using var connection = CreateOpenConnection();
		Assert.NotNull(connection.ServerVersion);
		Assert.NotEmpty(connection.ServerVersion);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void State_closed_by_default()
	{
		using var connection = CreateConnection();
		Assert.Equal(ConnectionState.Closed, connection.State);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void Open_works()
	{
		using var connection = CreateConnection();
		var raised = false;
		var previousState = ConnectionState.Closed;

		void Handler(object sender, StateChangeEventArgs e)
		{
			raised = true;

			Assert.Equal(connection, sender);
			Assert.Equal(previousState, e.OriginalState);
			if (previousState == ConnectionState.Closed && (e.CurrentState == ConnectionState.Connecting || e.CurrentState == ConnectionState.Open))
				previousState = e.CurrentState;
			else if (previousState == ConnectionState.Connecting && e.CurrentState == ConnectionState.Open)
				previousState = e.CurrentState;
			else
				Assert.Equal(ConnectionState.Open, e.CurrentState);
		}

		connection.StateChange += Handler;
		try
		{
			connection.ConnectionString = ConnectionString;
			connection.Open();

			Assert.True(raised);
			Assert.Equal(ConnectionState.Open, connection.State);
		}
		finally
		{
			connection.StateChange -= Handler;
		}
	}

	/// <summary>
	/// A successful OpenAsync call must leave the connection open, just as Open
	/// does. This tests the required asynchronous lifecycle rather than the
	/// provider-dependent cancellation behavior below.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.openasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.openasync")]
	public virtual async Task OpenAsync_works()
	{
		using var connection = CreateConnection();
		connection.ConnectionString = ConnectionString;

		await connection.OpenAsync().ConfigureAwait(false);

		Assert.Equal(ConnectionState.Open, connection.State);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void Close_works()
	{
		using var connection = CreateOpenConnection();
		var raised = false;

		void Handler(object sender, StateChangeEventArgs e)
		{
			raised = true;

			Assert.Equal(connection, sender);
			Assert.Equal(ConnectionState.Open, e.OriginalState);
			Assert.Equal(ConnectionState.Closed, e.CurrentState);
		}

		connection.StateChange += Handler;
		try
		{
			connection.Close();

			Assert.True(raised);
			Assert.Equal(ConnectionState.Closed, connection.State);
		}
		finally
		{
			connection.StateChange -= Handler;
		}
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void Close_can_be_called_before_open()
	{
		using var connection = CreateConnection();
		connection.Close();
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void Close_can_be_called_more_than_once()
	{
		using var connection = CreateOpenConnection();
		connection.Close();
		connection.Close();
	}

#if NETSTANDARD2_1_OR_GREATER
	/// <summary>
	/// CloseAsync must complete the asynchronous connection lifecycle and leave
	/// the connection closed.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.closeasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.closeasync")]
	public virtual async Task CloseAsync_works()
	{
		using var connection = CreateOpenConnection();

		await connection.CloseAsync().ConfigureAwait(false);

		Assert.Equal(ConnectionState.Closed, connection.State);
	}
#endif

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void Dispose_can_be_called_more_than_once()
	{
		var connection = CreateOpenConnection();
		connection.Dispose();
		connection.Dispose();
	}

	/// <summary>
	/// Disposal raises the standard DbConnection Disposed event exactly once.
	/// See https://learn.microsoft.com/dotnet/api/system.componentmodel.component.disposed.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void Dispose_raises_Disposed()
	{
		var connection = CreateOpenConnection();
		var disposedCount = 0;
		connection.Disposed += (s, e) => disposedCount++;
		connection.Dispose();
		Assert.Equal(1, disposedCount);
	}

#if NETSTANDARD2_1_OR_GREATER
	/// <summary>
	/// DisposeAsync follows the same component-disposal event contract as Dispose.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.disposeasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual async Task DisposeAsync_raises_Disposed()
	{
		var connection = CreateOpenConnection();
		var disposedCount = 0;
		connection.Disposed += (s, e) => disposedCount++;
		await connection.DisposeAsync().ConfigureAwait(true);
		Assert.Equal(1, disposedCount);
	}
#endif

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void CreateCommand_returns_command()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		Assert.NotNull(command);
		Assert.Same(connection, command.Connection);
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void DbProviderFactory_has_correct_value()
	{
		using var connection = CreateConnection();
		var property = connection.GetType().GetProperty("DbProviderFactory", BindingFlags.Instance | BindingFlags.NonPublic);
		var dbProviderFactory = property.GetValue(connection);
		Assert.Same(Fixture.Factory, dbProviderFactory);
	}
}
