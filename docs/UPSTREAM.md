# Upstream reports

Where a provider fails a Required test, we send the fix to its maintainers. Each fix comes with a regression test in the
provider's own suite and the learn.microsoft.com sentence the provider departs from. This page lists what has been
submitted, and holds drafts for the trackers that do not take pull requests.

## Pull requests and issues

Status as of 2026-10-21.

| Provider | Link | Status | Contract |
|---|---|---|---|
| DuckDB.NET | [Giorgi/DuckDB.NET#357](https://github.com/Giorgi/DuckDB.NET/pull/357) | Merged | `CommandBehavior.CloseConnection` closes the connection last (regression test) |
| DuckDB.NET | [Giorgi/DuckDB.NET#359](https://github.com/Giorgi/DuckDB.NET/pull/359) | Merged | Negative intervals read and write |
| DuckDB.NET | [Giorgi/DuckDB.NET#360](https://github.com/Giorgi/DuckDB.NET/pull/360) | Merged | A `TimeSpan` parameter with no inferred type binds as an interval |
| DuckDB.NET | [Giorgi/DuckDB.NET#364](https://github.com/Giorgi/DuckDB.NET/pull/364) | Merged | `GetSchema("DataTypes")` and metadata probing (replaces #363) |
| DuckDB.NET | [Giorgi/DuckDB.NET#369](https://github.com/Giorgi/DuckDB.NET/pull/369) | Open | A `DbType` set explicitly survives assigning `Value` afterwards |
| DuckDB.NET | [Giorgi/DuckDB.NET#372](https://github.com/Giorgi/DuckDB.NET/pull/372) | Merged | `CommandType` defaults to `Text`, `Direction` to `Input` |
| DuckDB.NET | [Giorgi/DuckDB.NET#373](https://github.com/Giorgi/DuckDB.NET/pull/373) | Merged | `Close` can be called more than once |
| DuckDB.NET | [Giorgi/DuckDB.NET#374](https://github.com/Giorgi/DuckDB.NET/pull/374) | Merged | `ExecuteNonQuery` and `RecordsAffected` report -1 for non-DML |
| DuckDB.NET | [Giorgi/DuckDB.NET#375](https://github.com/Giorgi/DuckDB.NET/pull/375) | Merged | `GetOrdinal` case-insensitive fallback, `IndexOutOfRangeException` |
| DuckDB.NET | [Giorgi/DuckDB.NET#376](https://github.com/Giorgi/DuckDB.NET/pull/376) | Merged | `GetValues` copies only what the array holds |
| DuckDB.NET | [Giorgi/DuckDB.NET#377](https://github.com/Giorgi/DuckDB.NET/pull/377) | Merged | `GetBytes` and `GetChars` |
| DuckDB.NET | [Giorgi/DuckDB.NET#378](https://github.com/Giorgi/DuckDB.NET/pull/378) | Merged | `Prepare` requires an open connection |
| DuckDB.NET | [Giorgi/DuckDB.NET#379](https://github.com/Giorgi/DuckDB.NET/pull/379) | Merged | Closing a never-opened connection (regression test) |
| Microsoft.Data.Sqlite | [dotnet/efcore#39190](https://github.com/dotnet/efcore/issues/39190), [dotnet/efcore#39188](https://github.com/dotnet/efcore/pull/39188) | Merged | ADO.NET schema collections |
| Microsoft.Data.Sqlite | [dotnet/efcore#39219](https://github.com/dotnet/efcore/pull/39219) | Merged | Remaining schema metadata corrections |
| Microsoft.Data.Sqlite | [dotnet/efcore#39336](https://github.com/dotnet/efcore/issues/39336), [dotnet/efcore#39337](https://github.com/dotnet/efcore/pull/39337) | Open | A reader outlives its disposed command |
| Microsoft.Data.Sqlite | [dotnet/efcore#39343](https://github.com/dotnet/efcore/issues/39343), [dotnet/efcore#39344](https://github.com/dotnet/efcore/pull/39344) | Open | `GetOrdinal` throws `IndexOutOfRangeException`; `GetValues` accepts a short array |
| Microsoft.Data.SqlClient | [dotnet/SqlClient#4874](https://github.com/dotnet/SqlClient/pull/4874) | Merged | `ParameterCollection.RemoveAt(int)` throws `ArgumentOutOfRangeException` |
| FirebirdClient | [FirebirdSQL/NETProvider#1276](https://github.com/FirebirdSQL/NETProvider/pull/1276) | Closed | Provider substitutability (superseded) |
| FirebirdClient | [FirebirdSQL/NETProvider#1282](https://github.com/FirebirdSQL/NETProvider/issues/1282), [FirebirdSQL/NETProvider#1283](https://github.com/FirebirdSQL/NETProvider/pull/1283) | Closed | Combined fix, split into the four below |
| FirebirdClient | [FirebirdSQL/NETProvider#1284](https://github.com/FirebirdSQL/NETProvider/issues/1284), [FirebirdSQL/NETProvider#1288](https://github.com/FirebirdSQL/NETProvider/pull/1288) | Open | `HasRows` reports whether rows exist |
| FirebirdClient | [FirebirdSQL/NETProvider#1285](https://github.com/FirebirdSQL/NETProvider/issues/1285), [FirebirdSQL/NETProvider#1289](https://github.com/FirebirdSQL/NETProvider/pull/1289) | Open | `RecordsAffected` |
| FirebirdClient | [FirebirdSQL/NETProvider#1286](https://github.com/FirebirdSQL/NETProvider/issues/1286), [FirebirdSQL/NETProvider#1290](https://github.com/FirebirdSQL/NETProvider/pull/1290) | Open | `GetFieldType` is `byte[]` for OCTETS |
| FirebirdClient | [FirebirdSQL/NETProvider#1287](https://github.com/FirebirdSQL/NETProvider/issues/1287), [FirebirdSQL/NETProvider#1291](https://github.com/FirebirdSQL/NETProvider/pull/1291) | Open | `RemoveAt(int)` follows `IList` |
| AdoNetCore.AseClient | [DataAction/AdoNetCore.AseClient#241](https://github.com/DataAction/AdoNetCore.AseClient/pull/241) | Open | Read BIGTIME columns (TDS_BIGTIMEN 0xBC) |
| AdoNetCore.AseClient | [DataAction/AdoNetCore.AseClient#242](https://github.com/DataAction/AdoNetCore.AseClient/pull/242) | Open | Defaults, `DbException`, reader exceptions, `CloseConnection`, `ExecuteNonQuery` -1, `Prepare` |
| Npgsql | [npgsql/npgsql#6682](https://github.com/npgsql/npgsql/pull/6682) | Open | Reading an interval with negative months as `TimeSpan` throws instead of losing data |
| Npgsql | [npgsql/npgsql#6683](https://github.com/npgsql/npgsql/pull/6683) | Open | `DataTypes` advertises type modifiers from `pg_type.typmodin` |
| Npgsql | [npgsql/npgsql#6684](https://github.com/npgsql/npgsql/pull/6684) | Open | `DataTypes` quotes the internal `"char"` type's `CreateFormat` |
| TiDB (server) | [pingcap/tidb#72091](https://github.com/pingcap/tidb/pull/72091) | Open | `VALUES()` of a BIT(57..64) column read byte-reversed in `ON DUPLICATE KEY UPDATE` |

With its contract pull requests merged, DuckDB.NET passes every Required test. With #242, AseClient passes
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
