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
		CheckCommonSchemaCollections(connection, connection.GetSchema());
	}

	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.getschema")]
	public virtual void GetSchema_returns_proper_MetaDataCollections()
	{
		using var connection = CreateOpenConnection();
		CheckCommonSchemaCollections(connection, connection.GetSchema("MetaDataCollections"));
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
		CheckCommonSchemaCollections(connection, await connection.GetSchemaAsync().ConfigureAwait(false));
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

	/// <summary>
	/// StateChange "occurs when the state of the connection changes from closed to opened or from opened to closed", so
	/// each open and each close is reported once, as it happens: by the time Open or Close returns, and again on the next
	/// cycle. A consumer counting open connections from the event (pengdows.crud does) is off by one for every missing or
	/// repeated report.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.statechange.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.statechange")]
	public virtual void StateChange_reports_each_open_and_close_once()
	{
		using var connection = CreateConnection();
		connection.ConnectionString = ConnectionString;
		var events = new System.Collections.Generic.List<StateChangeEventArgs>();
		connection.StateChange += (_, e) => events.Add(e);

		for (var cycle = 1; cycle <= 2; cycle++)
		{
			connection.Open();
			AssertOneOpenAndClosesPerCycle(events, cycle, cycle - 1);
			connection.Close();
			AssertOneOpenAndClosesPerCycle(events, cycle, cycle);
		}
	}

	/// <summary>
	/// OpenAsync and CloseAsync change the connection's state just as Open and Close do, so they report it the same way:
	/// once each, by the time the returned task completes.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.statechange.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.statechange")]
	public virtual async Task StateChange_reports_each_async_open_and_close_once()
	{
		using var connection = CreateConnection();
		connection.ConnectionString = ConnectionString;
		var events = new System.Collections.Generic.List<StateChangeEventArgs>();
		connection.StateChange += (_, e) =>
		{
			lock (events)
				events.Add(e);
		};

		for (var cycle = 1; cycle <= 2; cycle++)
		{
			await connection.OpenAsync().ConfigureAwait(false);
			lock (events)
				AssertOneOpenAndClosesPerCycle(events, cycle, cycle - 1);
#if NETSTANDARD2_0
			connection.Close();
#else
			await connection.CloseAsync().ConfigureAwait(false);
#endif
			lock (events)
				AssertOneOpenAndClosesPerCycle(events, cycle, cycle);
		}
	}

	/// <summary>
	/// Disposing a connection that is already closed does not change its state, so it raises no StateChange; a consumer
	/// that closes before disposing would otherwise count the close twice.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.statechange.
	/// </summary>
	[RequiredFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.statechange")]
	public virtual void StateChange_is_not_raised_by_disposing_a_closed_connection()
	{
		var connection = CreateOpenConnection();
		connection.Close();
		var raised = 0;
		connection.StateChange += (_, _) => raised++;

		connection.Dispose();

		Assert.Equal(0, raised);
	}

	/// <summary>
	/// Asserts <paramref name="opens"/> reports that the connection became Open, each from Closed or Connecting, and
	/// <paramref name="closes"/> that it went from Open to Closed. Connecting is a state of its own and may be reported.
	/// </summary>
	private static void AssertOneOpenAndClosesPerCycle(System.Collections.Generic.List<StateChangeEventArgs> events, int opens, int closes)
	{
		var opened = events.Where(e => e.CurrentState == ConnectionState.Open).ToList();
		var closed = events.Where(e => e.CurrentState == ConnectionState.Closed).ToList();
		Assert.Equal(opens, opened.Count);
		Assert.Equal(closes, closed.Count);
		Assert.All(opened, e => Assert.Contains(e.OriginalState, new[] { ConnectionState.Closed, ConnectionState.Connecting }));
		Assert.All(closed, e => Assert.Equal(ConnectionState.Open, e.OriginalState));
	}

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
