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
	public virtual void ConnectionString_gets_and_sets_value()
	{
		using var connection = CreateConnection();
		connection.ConnectionString = ConnectionString;
		// The getter returns the string used to establish the connection; a provider may normalise or reorder it.
		Assert.False(string.IsNullOrWhiteSpace(connection.ConnectionString));
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

	/// <summary>
	/// Certification requires usable metadata collections. A provider may have
	/// an application-level workaround for missing schema metadata, but that
	/// workaround does not make the provider ADO.NET-conformant.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.getschema.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.getschema")]
	public virtual void GetSchema_parameterless_returns_proper_MetaDataCollections()
	{
		using var connection = CreateOpenConnection();
		CheckMetaDataCollectionsSchema(connection.GetSchema());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.getschema")]
	public virtual void GetSchema_returns_proper_MetaDataCollections()
	{
		using var connection = CreateOpenConnection();
		CheckMetaDataCollectionsSchema(connection.GetSchema("MetaDataCollections"));
	}

#if NET10_0_OR_GREATER
	/// <summary>
	/// Asynchronous schema metadata is required for certification and must
	/// preserve the metadata shape of the synchronous API.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.getschemaasync.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.getschemaasync")]
	public virtual async Task GetSchemaAsync_returns_metadata()
	{
		using var connection = CreateOpenConnection();
		CheckMetaDataCollectionsSchema(await connection.GetSchemaAsync().ConfigureAwait(false));
	}
#endif

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

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.close")]
	public virtual void Close_can_be_called_more_than_once()
	{
		using var connection = CreateOpenConnection();
		connection.Close();
		connection.Close();
	}

	/// <summary>
	/// StateChange "occurs when the state of the connection changes from closed to opened or from opened to closed", so
	/// closing a connection that is already closed raises nothing.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.statechange.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.statechange")]
	public virtual void StateChange_is_not_raised_by_closing_a_closed_connection()
	{
		using var connection = CreateOpenConnection();
		connection.Close();
		var raised = 0;
		connection.StateChange += (_, _) => raised++;

		connection.Close();

		Assert.Equal(0, raised);
		Assert.Equal(ConnectionState.Closed, connection.State);
	}

#if !NETSTANDARD2_0
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

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection")]
	public virtual void CreateCommand_returns_command()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		Assert.NotNull(command);
		Assert.Same(connection, command.Connection);
	}

}
