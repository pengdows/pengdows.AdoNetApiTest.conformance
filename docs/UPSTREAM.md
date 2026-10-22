# Upstream reports

Where a provider fails a Required test, we send the fix to its maintainers. Each fix comes with a regression test in the
provider's own suite and the learn.microsoft.com sentence the provider departs from. This page lists what has been
submitted, and holds drafts for the trackers that do not take pull requests.

## Pull requests and issues

The status badges come from GitHub through shields.io each time the page is viewed.

| Provider | Link | Status | Contract |
|---|---|---|---|
| DuckDB.NET | [Giorgi/DuckDB.NET#357](https://github.com/Giorgi/DuckDB.NET/pull/357) | ![merged](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/357?label=) | `CommandBehavior.CloseConnection` closes the connection last (regression test) |
| DuckDB.NET | [Giorgi/DuckDB.NET#359](https://github.com/Giorgi/DuckDB.NET/pull/359) | ![merged](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/359?label=) | Negative intervals read and write |
| DuckDB.NET | [Giorgi/DuckDB.NET#360](https://github.com/Giorgi/DuckDB.NET/pull/360) | ![merged](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/360?label=) | A `TimeSpan` parameter with no inferred type binds as an interval |
| DuckDB.NET | [Giorgi/DuckDB.NET#364](https://github.com/Giorgi/DuckDB.NET/pull/364) | ![merged](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/364?label=) | `GetSchema("DataTypes")` and metadata probing (replaces #363) |
| DuckDB.NET | [Giorgi/DuckDB.NET#369](https://github.com/Giorgi/DuckDB.NET/pull/369) | ![open](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/369?label=) | A `DbType` set explicitly survives assigning `Value` afterwards |
| DuckDB.NET | [Giorgi/DuckDB.NET#372](https://github.com/Giorgi/DuckDB.NET/pull/372) | ![merged](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/372?label=) | `CommandType` defaults to `Text`, `Direction` to `Input` |
| DuckDB.NET | [Giorgi/DuckDB.NET#373](https://github.com/Giorgi/DuckDB.NET/pull/373) | ![merged](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/373?label=) | `Close` can be called more than once |
| DuckDB.NET | [Giorgi/DuckDB.NET#374](https://github.com/Giorgi/DuckDB.NET/pull/374) | ![merged](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/374?label=) | `ExecuteNonQuery` and `RecordsAffected` report -1 for non-DML |
| DuckDB.NET | [Giorgi/DuckDB.NET#375](https://github.com/Giorgi/DuckDB.NET/pull/375) | ![merged](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/375?label=) | `GetOrdinal` case-insensitive fallback, `IndexOutOfRangeException` |
| DuckDB.NET | [Giorgi/DuckDB.NET#376](https://github.com/Giorgi/DuckDB.NET/pull/376) | ![merged](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/376?label=) | `GetValues` copies only what the array holds |
| DuckDB.NET | [Giorgi/DuckDB.NET#377](https://github.com/Giorgi/DuckDB.NET/pull/377) | ![merged](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/377?label=) | `GetBytes` and `GetChars` |
| DuckDB.NET | [Giorgi/DuckDB.NET#378](https://github.com/Giorgi/DuckDB.NET/pull/378) | ![merged](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/378?label=) | `Prepare` requires an open connection |
| DuckDB.NET | [Giorgi/DuckDB.NET#379](https://github.com/Giorgi/DuckDB.NET/pull/379) | ![merged](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/379?label=) | Closing a never-opened connection (regression test) |
| Microsoft.Data.Sqlite | [dotnet/efcore#39190](https://github.com/dotnet/efcore/issues/39190), [dotnet/efcore#39188](https://github.com/dotnet/efcore/pull/39188) | ![merged](https://img.shields.io/github/issues/detail/state/dotnet/efcore/39188?label=) | ADO.NET schema collections |
| Microsoft.Data.Sqlite | [dotnet/efcore#39219](https://github.com/dotnet/efcore/pull/39219) | ![merged](https://img.shields.io/github/issues/detail/state/dotnet/efcore/39219?label=) | Remaining schema metadata corrections |
| Microsoft.Data.Sqlite | [dotnet/efcore#39336](https://github.com/dotnet/efcore/issues/39336), [dotnet/efcore#39337](https://github.com/dotnet/efcore/pull/39337) | ![open](https://img.shields.io/github/issues/detail/state/dotnet/efcore/39337?label=) | A reader outlives its disposed command |
| Microsoft.Data.Sqlite | [dotnet/efcore#39343](https://github.com/dotnet/efcore/issues/39343), [#39353](https://github.com/dotnet/efcore/pull/39353) | ![status](https://img.shields.io/github/issues/detail/state/dotnet/efcore/39353?label=) | `GetOrdinal` throws `IndexOutOfRangeException` for an unknown name |
| Microsoft.Data.Sqlite | [dotnet/efcore#39352](https://github.com/dotnet/efcore/issues/39352), [#39354](https://github.com/dotnet/efcore/pull/39354) | ![status](https://img.shields.io/github/issues/detail/state/dotnet/efcore/39354?label=) | `GetValues` copies only what fits in a short array |
| Microsoft.Data.Sqlite | [dotnet/efcore#39344](https://github.com/dotnet/efcore/pull/39344) | ![status](https://img.shields.io/github/issues/detail/state/dotnet/efcore/39344?label=) | Combined `GetOrdinal`/`GetValues` fix, split into #39353 and #39354 |
| Microsoft.Data.SqlClient | [dotnet/SqlClient#4874](https://github.com/dotnet/SqlClient/pull/4874) | ![merged](https://img.shields.io/github/issues/detail/state/dotnet/SqlClient/4874?label=) | `ParameterCollection.RemoveAt(int)` throws `ArgumentOutOfRangeException` |
| FirebirdClient | [FirebirdSQL/NETProvider#1276](https://github.com/FirebirdSQL/NETProvider/pull/1276) | ![closed](https://img.shields.io/github/issues/detail/state/FirebirdSQL/NETProvider/1276?label=) | Provider substitutability (superseded) |
| FirebirdClient | [FirebirdSQL/NETProvider#1282](https://github.com/FirebirdSQL/NETProvider/issues/1282), [FirebirdSQL/NETProvider#1283](https://github.com/FirebirdSQL/NETProvider/pull/1283) | ![closed](https://img.shields.io/github/issues/detail/state/FirebirdSQL/NETProvider/1283?label=) | Combined fix, split into the four below |
| FirebirdClient | [FirebirdSQL/NETProvider#1284](https://github.com/FirebirdSQL/NETProvider/issues/1284), [FirebirdSQL/NETProvider#1288](https://github.com/FirebirdSQL/NETProvider/pull/1288) | ![open](https://img.shields.io/github/issues/detail/state/FirebirdSQL/NETProvider/1288?label=) | `HasRows` reports whether rows exist |
| FirebirdClient | [FirebirdSQL/NETProvider#1285](https://github.com/FirebirdSQL/NETProvider/issues/1285), [FirebirdSQL/NETProvider#1289](https://github.com/FirebirdSQL/NETProvider/pull/1289) | ![open](https://img.shields.io/github/issues/detail/state/FirebirdSQL/NETProvider/1289?label=) | `RecordsAffected` |
| FirebirdClient | [FirebirdSQL/NETProvider#1286](https://github.com/FirebirdSQL/NETProvider/issues/1286), [FirebirdSQL/NETProvider#1290](https://github.com/FirebirdSQL/NETProvider/pull/1290) | ![open](https://img.shields.io/github/issues/detail/state/FirebirdSQL/NETProvider/1290?label=) | `GetFieldType` is `byte[]` for OCTETS |
| FirebirdClient | [FirebirdSQL/NETProvider#1287](https://github.com/FirebirdSQL/NETProvider/issues/1287), [FirebirdSQL/NETProvider#1291](https://github.com/FirebirdSQL/NETProvider/pull/1291) | ![open](https://img.shields.io/github/issues/detail/state/FirebirdSQL/NETProvider/1291?label=) | `RemoveAt(int)` follows `IList` |

| AdoNetCore.AseClient | [DataAction/AdoNetCore.AseClient#241](https://github.com/DataAction/AdoNetCore.AseClient/pull/241) | ![open](https://img.shields.io/github/issues/detail/state/DataAction/AdoNetCore.AseClient/241?label=) | Read BIGTIME columns (TDS_BIGTIMEN 0xBC) |
| AdoNetCore.AseClient | [DataAction/AdoNetCore.AseClient#243](https://github.com/DataAction/AdoNetCore.AseClient/pull/243) | ![status](https://img.shields.io/github/issues/detail/state/DataAction/AdoNetCore.AseClient/243?label=) | `CommandType` defaults to `Text` |
| AdoNetCore.AseClient | [DataAction/AdoNetCore.AseClient#244](https://github.com/DataAction/AdoNetCore.AseClient/pull/244) | ![status](https://img.shields.io/github/issues/detail/state/DataAction/AdoNetCore.AseClient/244?label=) | `ParameterName` defaults to `""` |
| AdoNetCore.AseClient | [DataAction/AdoNetCore.AseClient#245](https://github.com/DataAction/AdoNetCore.AseClient/pull/245) | ![status](https://img.shields.io/github/issues/detail/state/DataAction/AdoNetCore.AseClient/245?label=) | `AseException` derives from `DbException` |
| AdoNetCore.AseClient | [DataAction/AdoNetCore.AseClient#246](https://github.com/DataAction/AdoNetCore.AseClient/pull/246) | ![status](https://img.shields.io/github/issues/detail/state/DataAction/AdoNetCore.AseClient/246?label=) | Execute methods throw `InvalidOperationException` without an open connection |
| AdoNetCore.AseClient | [DataAction/AdoNetCore.AseClient#247](https://github.com/DataAction/AdoNetCore.AseClient/pull/247) | ![status](https://img.shields.io/github/issues/detail/state/DataAction/AdoNetCore.AseClient/247?label=) | Value access throws `InvalidOperationException` / `IndexOutOfRangeException` |
| AdoNetCore.AseClient | [DataAction/AdoNetCore.AseClient#248](https://github.com/DataAction/AdoNetCore.AseClient/pull/248) | ![status](https://img.shields.io/github/issues/detail/state/DataAction/AdoNetCore.AseClient/248?label=) | `GetOrdinal` throws `IndexOutOfRangeException` for an unknown name |
| AdoNetCore.AseClient | [DataAction/AdoNetCore.AseClient#249](https://github.com/DataAction/AdoNetCore.AseClient/pull/249) | ![status](https://img.shields.io/github/issues/detail/state/DataAction/AdoNetCore.AseClient/249?label=) | `CommandBehavior.CloseConnection` closes the connection |
| AdoNetCore.AseClient | [DataAction/AdoNetCore.AseClient#250](https://github.com/DataAction/AdoNetCore.AseClient/pull/250) | ![status](https://img.shields.io/github/issues/detail/state/DataAction/AdoNetCore.AseClient/250?label=) | `ExecuteNonQuery` returns -1 for non-DML |
| AdoNetCore.AseClient | [DataAction/AdoNetCore.AseClient#251](https://github.com/DataAction/AdoNetCore.AseClient/pull/251) | ![status](https://img.shields.io/github/issues/detail/state/DataAction/AdoNetCore.AseClient/251?label=) | `Prepare` requires an open connection |
| AdoNetCore.AseClient | [DataAction/AdoNetCore.AseClient#242](https://github.com/DataAction/AdoNetCore.AseClient/pull/242) | ![status](https://img.shields.io/github/issues/detail/state/DataAction/AdoNetCore.AseClient/242?label=) | Combined contract fixes, split into #243–#251 |
| Npgsql | [npgsql/npgsql#6682](https://github.com/npgsql/npgsql/pull/6682) | ![open](https://img.shields.io/github/issues/detail/state/npgsql/npgsql/6682?label=) | Reading an interval with negative months as `TimeSpan` throws instead of losing data |
| Npgsql | [npgsql/npgsql#6683](https://github.com/npgsql/npgsql/pull/6683) | ![open](https://img.shields.io/github/issues/detail/state/npgsql/npgsql/6683?label=) | `DataTypes` advertises type modifiers from `pg_type.typmodin` |
| Npgsql | [npgsql/npgsql#6684](https://github.com/npgsql/npgsql/pull/6684) | ![open](https://img.shields.io/github/issues/detail/state/npgsql/npgsql/6684?label=) | `DataTypes` quotes the internal `"char"` type's `CreateFormat` |
| Ydb.Sdk | [ydb-platform/ydb-dotnet-sdk#758](https://github.com/ydb-platform/ydb-dotnet-sdk/issues/758), [#762](https://github.com/ydb-platform/ydb-dotnet-sdk/pull/762) | ![status](https://img.shields.io/github/issues/detail/state/ydb-platform/ydb-dotnet-sdk/762?label=) | `Release` throws `NotSupportedException` when `SupportsSavepoints` is false |
| Ydb.Sdk | [ydb-platform/ydb-dotnet-sdk#759](https://github.com/ydb-platform/ydb-dotnet-sdk/issues/759), [#763](https://github.com/ydb-platform/ydb-dotnet-sdk/pull/763) | ![status](https://img.shields.io/github/issues/detail/state/ydb-platform/ydb-dotnet-sdk/763?label=) | `GetOrdinal` case-insensitive fallback |
| Ydb.Sdk | [ydb-platform/ydb-dotnet-sdk#471](https://github.com/ydb-platform/ydb-dotnet-sdk/issues/471), [#764](https://github.com/ydb-platform/ydb-dotnet-sdk/pull/764) | ![status](https://img.shields.io/github/issues/detail/state/ydb-platform/ydb-dotnet-sdk/764?label=) | `ServerVersion` returns the server's version |
| Ydb.Sdk | [ydb-platform/ydb-dotnet-sdk#760](https://github.com/ydb-platform/ydb-dotnet-sdk/issues/760), [#765](https://github.com/ydb-platform/ydb-dotnet-sdk/pull/765) | ![status](https://img.shields.io/github/issues/detail/state/ydb-platform/ydb-dotnet-sdk/765?label=) | `CommandBehavior.CloseConnection` closes the connection with the reader |
| Ydb.Sdk | [ydb-platform/ydb-dotnet-sdk#761](https://github.com/ydb-platform/ydb-dotnet-sdk/issues/761), [#766](https://github.com/ydb-platform/ydb-dotnet-sdk/pull/766) | ![status](https://img.shields.io/github/issues/detail/state/ydb-platform/ydb-dotnet-sdk/766?label=) | `ParameterName` reads back as set (marked breaking upstream) |
| Ydb.Sdk | [ydb-platform/ydb-api-protos#84](https://github.com/ydb-platform/ydb-api-protos/pull/84) → [ydb-platform/ydb-dotnet-genproto#29](https://github.com/ydb-platform/ydb-dotnet-genproto/pull/29) → [ydb-platform/ydb-dotnet-sdk#768](https://github.com/ydb-platform/ydb-dotnet-sdk/pull/768) | ![status](https://img.shields.io/github/issues/detail/state/ydb-platform/ydb-dotnet-sdk/768?label=) | Expose affected row counts so `ExecuteNonQuery` and `RecordsAffected` report DML rows |
| MySql.Data | [pengdows/mysql-connector-net#1](https://github.com/pengdows/mysql-connector-net/pull/1) | ![status](https://img.shields.io/github/issues/detail/state/pengdows/mysql-connector-net/1?label=) | `GetOrdinal` performs exact-case lookup before case-insensitive fallback |
| MySql.Data | [pengdows/mysql-connector-net#2](https://github.com/pengdows/mysql-connector-net/pull/2) | ![status](https://img.shields.io/github/issues/detail/state/pengdows/mysql-connector-net/2?label=) | `ParameterName` defaults to `""` and `SourceVersion` to `Current` |
| MySql.Data | [pengdows/mysql-connector-net#3](https://github.com/pengdows/mysql-connector-net/pull/3) | ![status](https://img.shields.io/github/issues/detail/state/pengdows/mysql-connector-net/3?label=) | Reader value access throws `InvalidOperationException` before and after rows |
| MySql.Data | [pengdows/mysql-connector-net#4](https://github.com/pengdows/mysql-connector-net/pull/4) | ![status](https://img.shields.io/github/issues/detail/state/pengdows/mysql-connector-net/4?label=) | Undefined `UpdatedRowSource` values are rejected |
| MySql.Data | [pengdows/mysql-connector-net#5](https://github.com/pengdows/mysql-connector-net/pull/5) | ![status](https://img.shields.io/github/issues/detail/state/pengdows/mysql-connector-net/5?label=) | Unsupported savepoints report `SupportsSavepoints = false` and throw consistently |
| MySqlConnector | [mysql-net/MySqlConnector#1676](https://github.com/mysql-net/MySqlConnector/issues/1676) (issue) | ![status](https://img.shields.io/github/issues/detail/state/mysql-net/MySqlConnector/1676?label=) | `SupportsSavepoints` is `false` although the savepoint methods work |
| MySqlConnector | [mysql-net/MySqlConnector#1677](https://github.com/mysql-net/MySqlConnector/issues/1677) (issue) | ![status](https://img.shields.io/github/issues/detail/state/mysql-net/MySqlConnector/1677?label=) | SingleStore: savepoint statements succeed but do nothing, so rolled-back work is committed |
| TiDB (server) | [pingcap/tidb#72091](https://github.com/pingcap/tidb/pull/72091) | ![open](https://img.shields.io/github/issues/detail/state/pingcap/tidb/72091?label=) | `VALUES()` of a BIT(57..64) column read byte-reversed in `ON DUPLICATE KEY UPDATE` |

DuckDB.NET has merged every listed contract fix except [#369](https://github.com/Giorgi/DuckDB.NET/pull/369), which
remains open and covers preserving an explicitly assigned `DbType` when `Value` is assigned later. The merged fixes are
ready for the next provider release; certification is based on the resulting released package, not on PR state alone.
With #243–#251, AseClient passes 105 of 112; the other two are ASE server behaviour and a parameter-name convention
that no contract sentence covers.

## Not sent

- **Microsoft.Data.SqlClient `Prepare`**: [SqlCommand.Prepare](https://learn.microsoft.com/dotnet/api/microsoft.data.sqlclient.sqlcommand.prepare)
  documents the behaviour it has: "Prior to Visual Studio 2010, Prepare threw an exception. Beginning in Visual Studio
  2010, this method does not throw an exception." It is the provider's documented choice, not an oversight.
- **Faithlife.System.Data.SQLite**: the repository is archived.
- **Ydb.Sdk row counts** are no longer unsent. The fix is now in the dependency chain
  [ydb-api-protos#84](https://github.com/ydb-platform/ydb-api-protos/pull/84) →
  [ydb-dotnet-genproto#29](https://github.com/ydb-platform/ydb-dotnet-genproto/pull/29) →
  [ydb-dotnet-sdk#768](https://github.com/ydb-platform/ydb-dotnet-sdk/pull/768). Until that chain is merged and released,
  `ExecuteNonQuery` and `RecordsAffected` still return -1 for INSERT, UPDATE and DELETE, and YDB does not pass the
  Required DML tests.

## Drafts for trackers without pull requests

### MySql.Data: remaining release retest

The current released packages still show the findings addressed by [PR #1](https://github.com/pengdows/mysql-connector-net/pull/1)
through [PR #5](https://github.com/pengdows/mysql-connector-net/pull/5). Each PR includes the required attribution:
“found using [pengdows.AdoNetApiTest.conformance](https://github.com/pengdows/pengdows.AdoNetApiTest.conformance)”.

The released packages also reported `Connection.Open_works`, but the current Connector/NET `9.x` source already contains
the `Closed → Connecting → Open` state transitions. That result should be retested when the next package is published,
rather than duplicated as a source PR.

### System.Data.SQLite: [SQLite forum](https://sqlite.org/forum/) (System.Data.SQLite)

System.Data.SQLite takes no pull requests. Per its [contribution page](https://system.data.sqlite.org/home/doc/trunk/www/contribute.wiki),
patches are emailed to drh@sqlite.org and are applied only with a signed Contributor Agreement on file (otherwise
"used for reference only"); bugs go to the SQLite Forum. This draft is for the forum.

> **System.Data.SQLite 1.0.119: ParameterName default, GetOrdinal, Prepare without a connection, and savepoints**
>
> 1. `new SQLiteParameter().ParameterName` is `null`. [DbParameter.ParameterName](https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.parametername):
>    "The default is an empty string ("")."
> 2. `reader.GetOrdinal("missing")` does not throw. [IDataRecord.GetOrdinal](https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getordinal):
>    "If the index of the named field is not found, an `IndexOutOfRangeException` is thrown."
> 3. `Prepare()` on a command with no connection, or a closed one, does not throw. [IDbCommand.Prepare](https://learn.microsoft.com/dotnet/api/system.data.idbcommand.prepare)
>    documents `InvalidOperationException`: "The Connection is not set. -or- The Connection is not Open()."
>
> 4. `GetOrdinal` returns the first column that matches ignoring case, even when another column matches exactly: for
>    `SELECT 1 AS a, 2 AS A`, `GetOrdinal("A")` is 0. [IDataRecord.GetOrdinal](https://learn.microsoft.com/dotnet/api/system.data.idatarecord.getordinal):
>    "`GetOrdinal` performs a case-sensitive lookup first. If it fails, a second case-insensitive search is made."
> 5. `SQLiteTransaction.SupportsSavepoints` is `false` and `Save`/`Rollback(string)` throw `NotSupportedException`, but
>    `Release(string)` returns normally (the base class does nothing). [DbTransaction.SupportsSavepoints](https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction.supportssavepoints):
>    "If `false`, the methods SaveAsync, RollbackAsync and ReleaseAsync as well as their synchronous counterparts are
>    expected to throw NotSupportedException." Since SQLite has `SAVEPOINT`, `RELEASE` and `ROLLBACK TO`, implementing
>    them (and returning `true`) would be the better fix; otherwise `Release` should throw too.
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
