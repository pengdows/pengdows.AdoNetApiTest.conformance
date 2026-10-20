# Upstream reports

Where a provider fails a Required test, we send the fix to its maintainers. Each fix comes with a regression test in the
provider's own suite and the learn.microsoft.com sentence the provider departs from. This page lists what has been
submitted, and holds drafts for the trackers that do not take pull requests.

## Pull requests and issues

| Provider | Link | Contract |
|---|---|---|
| DuckDB.NET | [#372](https://github.com/Giorgi/DuckDB.NET/pull/372) | `CommandType` defaults to `Text`, `Direction` to `Input` |
| DuckDB.NET | [#373](https://github.com/Giorgi/DuckDB.NET/pull/373) | `Close` can be called more than once |
| DuckDB.NET | [#374](https://github.com/Giorgi/DuckDB.NET/pull/374) | `ExecuteNonQuery` and `RecordsAffected` report -1 for non-DML |
| DuckDB.NET | [#375](https://github.com/Giorgi/DuckDB.NET/pull/375) | `GetOrdinal` case-insensitive fallback, `IndexOutOfRangeException` |
| DuckDB.NET | [#376](https://github.com/Giorgi/DuckDB.NET/pull/376) | `GetValues` copies only what the array holds |
| DuckDB.NET | [#377](https://github.com/Giorgi/DuckDB.NET/pull/377) | `GetBytes` and `GetChars` |
| DuckDB.NET | [#378](https://github.com/Giorgi/DuckDB.NET/pull/378) | `Prepare` requires an open connection |
| Microsoft.Data.Sqlite | [dotnet/efcore#39343](https://github.com/dotnet/efcore/issues/39343), [#39344](https://github.com/dotnet/efcore/pull/39344) | `GetOrdinal` throws `IndexOutOfRangeException`; `GetValues` accepts a short array |
| Microsoft.Data.Sqlite | [dotnet/efcore#39336](https://github.com/dotnet/efcore/issues/39336), [#39337](https://github.com/dotnet/efcore/pull/39337) | A reader outlives its disposed command |
| Microsoft.Data.SqlClient | [dotnet/SqlClient#4874](https://github.com/dotnet/SqlClient/pull/4874) | `ParameterCollection.RemoveAt(int)` throws `ArgumentOutOfRangeException` |
| FirebirdClient | [FirebirdSQL/NETProvider#1282](https://github.com/FirebirdSQL/NETProvider/issues/1282), [#1283](https://github.com/FirebirdSQL/NETProvider/pull/1283) | `HasRows`, `RecordsAffected`, `GetFieldType` for OCTETS, `RemoveAt(int)` |
| AdoNetCore.AseClient | [#242](https://github.com/DataAction/AdoNetCore.AseClient/pull/242) | Defaults, `DbException`, reader exceptions, `CloseConnection`, `ExecuteNonQuery` -1, `Prepare` |

With all seven DuckDB.NET pull requests applied, DuckDB.NET passes every Required test. With #242, AseClient passes
105 of 112; the other two are ASE server behaviour and a parameter-name convention that no contract sentence covers.

## Not sent

- **Microsoft.Data.SqlClient `Prepare`**: [SqlCommand.Prepare](https://learn.microsoft.com/dotnet/api/microsoft.data.sqlclient.sqlcommand.prepare)
  documents the behaviour it has: "Prior to Visual Studio 2010, Prepare threw an exception. Beginning in Visual Studio
  2010, this method does not throw an exception." It is the provider's documented choice, not an oversight.
- **Faithlife.System.Data.SQLite**: the repository is archived.

## Drafts for trackers without pull requests

### MySql.Data: [bugs.mysql.com](https://bugs.mysql.com/) (Connector / NET)

> **MySqlParameter.ParameterName defaults to null; MySqlDataReader throws MySqlException instead of InvalidOperationException**
>
> Connector/NET 9.7.0 and 26.7.0 (MySQL 8.4.11):
>
> 1. `new MySqlParameter().ParameterName` is `null`. [DbParameter.ParameterName](https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.parametername):
>    "The default is an empty string ("")."
> 2. `reader.GetValue(0)` or `reader.GetFieldValue<int>(0)` before `Read()` throws `MySqlException` ("Invalid attempt to
>    access a field before calling Read()"), and after `Read()` returned false it returns a value without throwing.
>    [DbDataReader.GetFieldValue](https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldvalue)
>    documents `InvalidOperationException` when "There is no data ready to be read (for example, the first Read()
>    hasn't been called, or it returned false)." MySqlConnector throws `InvalidOperationException` in both cases.
>
> Found by [`ParameterName_default_is_empty_string`](https://github.com/pengdows/pengdows.AdoNetApiTest.conformance/blob/5d2276130647c1cd65f679d909a25352133c050a/src/pengdows.AdoNetApiTest.conformance/Required/ParameterTestBase.Required.cs#L92) and [`GetFieldValue_throws_before_read`](https://github.com/pengdows/pengdows.AdoNetApiTest.conformance/blob/5d2276130647c1cd65f679d909a25352133c050a/src/pengdows.AdoNetApiTest.conformance/Required/DataReaderTestBase.Required.cs#L118) in the
> pengdows ADO.NET conformance suite.

### System.Data.SQLite: [SQLite forum](https://sqlite.org/forum/) (System.Data.SQLite)

> **System.Data.SQLite 1.0.119: ParameterName default and GetOrdinal for an unknown column**
>
> 1. `new SQLiteParameter().ParameterName` is `null`. [DbParameter.ParameterName](https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.parametername):
>    "The default is an empty string ("")."
> 2. `reader.GetOrdinal("missing")` does not throw. [IDataRecord.GetOrdinal](https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getordinal):
>    "If the index of the named field is not found, an `IndexOutOfRangeException` is thrown."
> 3. `Prepare()` on a command with no connection, or a closed one, does not throw. [IDbCommand.Prepare](https://learn.microsoft.com/dotnet/api/system.data.idbcommand.prepare)
>    documents `InvalidOperationException`: "The Connection is not set. -or- The Connection is not Open()."
>
> Found by [`ParameterName_default_is_empty_string`](https://github.com/pengdows/pengdows.AdoNetApiTest.conformance/blob/5d2276130647c1cd65f679d909a25352133c050a/src/pengdows.AdoNetApiTest.conformance/Required/ParameterTestBase.Required.cs#L92), [`GetOrdinal_throws_when_out_of_range`](https://github.com/pengdows/pengdows.AdoNetApiTest.conformance/blob/5d2276130647c1cd65f679d909a25352133c050a/src/pengdows.AdoNetApiTest.conformance/Required/DataReaderTestBase.Required.cs#L252) and
> [`Prepare_throws_when_no_connection`](https://github.com/pengdows/pengdows.AdoNetApiTest.conformance/blob/5d2276130647c1cd65f679d909a25352133c050a/src/pengdows.AdoNetApiTest.conformance/Required/CommandTestBase.Required.cs#L142) in the pengdows ADO.NET conformance suite.

### ODP.NET (Oracle.ManagedDataAccess.Core): Oracle Support or the [ODP.NET forum](https://forums.oracle.com/ords/apexds/domain/dev-community/category/odp-dot-net)

> **OracleCommand.Prepare does not throw without an open connection**
>
> Oracle.ManagedDataAccess.Core 23.8 (Oracle Database 23ai Free): `Prepare()` on an `OracleCommand` with no connection, or
> a closed one, returns normally. [IDbCommand.Prepare](https://learn.microsoft.com/dotnet/api/system.data.idbcommand.prepare)
> documents `InvalidOperationException`: "The Connection is not set. -or- The Connection is not Open()." This is the only
> Required test ODP.NET fails in the pengdows ADO.NET conformance suite: [`Prepare_throws_when_no_connection`](https://github.com/pengdows/pengdows.AdoNetApiTest.conformance/blob/5d2276130647c1cd65f679d909a25352133c050a/src/pengdows.AdoNetApiTest.conformance/Required/CommandTestBase.Required.cs#L142).
