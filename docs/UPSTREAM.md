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
| DuckDB.NET | [Giorgi/DuckDB.NET#382](https://github.com/Giorgi/DuckDB.NET/pull/382) | ![open](https://img.shields.io/github/issues/detail/state/Giorgi/DuckDB.NET/382?label=) | `DbType.Decimal`/`DateTime2` bind as their types, not current-culture text; `decimal` infers `Decimal`, not `Currency` |
| Microsoft.Data.Sqlite | [dotnet/efcore#39190](https://github.com/dotnet/efcore/issues/39190), [dotnet/efcore#39188](https://github.com/dotnet/efcore/pull/39188) | ![merged](https://img.shields.io/github/issues/detail/state/dotnet/efcore/39188?label=) | ADO.NET schema collections |
| Microsoft.Data.Sqlite | [dotnet/efcore#39219](https://github.com/dotnet/efcore/pull/39219) | ![merged](https://img.shields.io/github/issues/detail/state/dotnet/efcore/39219?label=) | Remaining schema metadata corrections |
| Microsoft.Data.Sqlite | [dotnet/efcore#39336](https://github.com/dotnet/efcore/issues/39336), [dotnet/efcore#39337](https://github.com/dotnet/efcore/pull/39337) | ![open](https://img.shields.io/github/issues/detail/state/dotnet/efcore/39337?label=) | A reader outlives its disposed command |
| Microsoft.Data.Sqlite | [dotnet/efcore#39343](https://github.com/dotnet/efcore/issues/39343), [#39353](https://github.com/dotnet/efcore/pull/39353) | ![status](https://img.shields.io/github/issues/detail/state/dotnet/efcore/39353?label=) | `GetOrdinal` throws `IndexOutOfRangeException` for an unknown name |
| Microsoft.Data.Sqlite | [dotnet/efcore#39352](https://github.com/dotnet/efcore/issues/39352), [#39354](https://github.com/dotnet/efcore/pull/39354) | ![status](https://img.shields.io/github/issues/detail/state/dotnet/efcore/39354?label=) | `GetValues` copies only what fits in a short array |
| Microsoft.Data.Sqlite | [dotnet/efcore#39344](https://github.com/dotnet/efcore/pull/39344) | ![status](https://img.shields.io/github/issues/detail/state/dotnet/efcore/39344?label=) | Combined `GetOrdinal`/`GetValues` fix, split into #39353 and #39354 |
| Microsoft.Data.Sqlite | [dotnet/efcore#39376](https://github.com/dotnet/efcore/issues/39376), [#39377](https://github.com/dotnet/efcore/pull/39377) | ![status](https://img.shields.io/github/issues/detail/state/dotnet/efcore/39377?label=) | `Size = 0` sends the whole value, not an empty string or blob |
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
| MySql.Data | [mysql/mysql-connector-net#95](https://github.com/mysql/mysql-connector-net/pull/95), [Bug #121583](https://bugs.mysql.com/121583) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/95?label=) | `GetOrdinal` performs exact-case lookup before case-insensitive fallback |
| MySql.Data | [mysql/mysql-connector-net#96](https://github.com/mysql/mysql-connector-net/pull/96), [Bug #121584](https://bugs.mysql.com/121584) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/96?label=) | `ParameterName` defaults to `""` and `SourceVersion` to `Current` |
| MySql.Data | [mysql/mysql-connector-net#97](https://github.com/mysql/mysql-connector-net/pull/97), [Bug #121585](https://bugs.mysql.com/121585) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/97?label=) | Reader value access throws `InvalidOperationException` before and after rows |
| MySql.Data | [mysql/mysql-connector-net#98](https://github.com/mysql/mysql-connector-net/pull/98), [Bug #121586](https://bugs.mysql.com/121586) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/98?label=) | Undefined `UpdatedRowSource` values are rejected |
| MySql.Data | [mysql/mysql-connector-net#99](https://github.com/mysql/mysql-connector-net/pull/99), [Bug #121587](https://bugs.mysql.com/121587) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/99?label=) | Unsupported savepoints report `SupportsSavepoints = false` and throw consistently |
| MySql.Data 8.x | [mysql/mysql-connector-net#100](https://github.com/mysql/mysql-connector-net/pull/100), [Bug #121583](https://bugs.mysql.com/121583) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/100?label=) | `GetOrdinal` performs exact-case lookup before case-insensitive fallback |
| MySql.Data 8.x | [mysql/mysql-connector-net#101](https://github.com/mysql/mysql-connector-net/pull/101), [Bug #121584](https://bugs.mysql.com/121584) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/101?label=) | `ParameterName` defaults to `""` and `SourceVersion` to `Current` |
| MySql.Data 8.x | [mysql/mysql-connector-net#102](https://github.com/mysql/mysql-connector-net/pull/102), [Bug #121585](https://bugs.mysql.com/121585) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/102?label=) | Reader value access throws `InvalidOperationException` before and after rows |
| MySql.Data 8.x | [mysql/mysql-connector-net#103](https://github.com/mysql/mysql-connector-net/pull/103), [Bug #121586](https://bugs.mysql.com/121586) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/103?label=) | Undefined `UpdatedRowSource` values are rejected |
| MySql.Data 8.x | [mysql/mysql-connector-net#104](https://github.com/mysql/mysql-connector-net/pull/104), [Bug #121587](https://bugs.mysql.com/121587) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/104?label=) | Unsupported savepoints report `SupportsSavepoints = false` and throw consistently |
| MySql.Data trunk | [mysql/mysql-connector-net#105](https://github.com/mysql/mysql-connector-net/pull/105), [Bug #121583](https://bugs.mysql.com/121583) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/105?label=) | `GetOrdinal` performs exact-case lookup before case-insensitive fallback |
| MySql.Data trunk | [mysql/mysql-connector-net#106](https://github.com/mysql/mysql-connector-net/pull/106), [Bug #121584](https://bugs.mysql.com/121584) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/106?label=) | `ParameterName` defaults to `""` and `SourceVersion` to `Current` |
| MySql.Data trunk | [mysql/mysql-connector-net#107](https://github.com/mysql/mysql-connector-net/pull/107), [Bug #121585](https://bugs.mysql.com/121585) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/107?label=) | Reader value access throws `InvalidOperationException` before and after rows |
| MySql.Data trunk | [mysql/mysql-connector-net#108](https://github.com/mysql/mysql-connector-net/pull/108), [Bug #121586](https://bugs.mysql.com/121586) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/108?label=) | Undefined `UpdatedRowSource` values are rejected |
| MySql.Data trunk | [mysql/mysql-connector-net#109](https://github.com/mysql/mysql-connector-net/pull/109), [Bug #121587](https://bugs.mysql.com/121587) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/109?label=) | Unsupported savepoints report `SupportsSavepoints = false` and throw consistently |
| MySql.Data | [mysql/mysql-connector-net#110](https://github.com/mysql/mysql-connector-net/pull/110) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/110?label=) | Opening raises `StateChange` with `Connecting` to `Open`, not `Open` to `Open` |
| MySql.Data 8.x | [mysql/mysql-connector-net#111](https://github.com/mysql/mysql-connector-net/pull/111) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/111?label=) | Opening raises `StateChange` with `Connecting` to `Open`, not `Open` to `Open` |
| MySql.Data trunk | [mysql/mysql-connector-net#112](https://github.com/mysql/mysql-connector-net/pull/112) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/112?label=) | Opening raises `StateChange` with `Connecting` to `Open`, not `Open` to `Open` |
| MySql.Data | [mysql/mysql-connector-net#113](https://github.com/mysql/mysql-connector-net/pull/113) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/113?label=) | A `char` parameter is inferred as `StringFixedLength`, not `Object` (blob) |
| MySql.Data 8.x | [mysql/mysql-connector-net#114](https://github.com/mysql/mysql-connector-net/pull/114) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/114?label=) | A `char` parameter is inferred as `StringFixedLength`, not `Object` (blob) |
| MySql.Data trunk | [mysql/mysql-connector-net#115](https://github.com/mysql/mysql-connector-net/pull/115) | ![status](https://img.shields.io/github/issues/detail/state/mysql/mysql-connector-net/115?label=) | A `char` parameter is inferred as `StringFixedLength`, not `Object` (blob) |
| MySqlConnector | [mysql-net/MySqlConnector#1676](https://github.com/mysql-net/MySqlConnector/issues/1676) (issue) | ![status](https://img.shields.io/github/issues/detail/state/mysql-net/MySqlConnector/1676?label=) | `SupportsSavepoints` is `false` although the savepoint methods work |
| MySqlConnector | [mysql-net/MySqlConnector#1677](https://github.com/mysql-net/MySqlConnector/issues/1677) (issue) | ![status](https://img.shields.io/github/issues/detail/state/mysql-net/MySqlConnector/1677?label=) | SingleStore: savepoint statements succeed but do nothing, so rolled-back work is committed |
| MySqlConnector | [mysql-net/MySqlConnector#1679](https://github.com/mysql-net/MySqlConnector/issues/1679), [#1680](https://github.com/mysql-net/MySqlConnector/pull/1680) | ![status](https://img.shields.io/github/issues/detail/state/mysql-net/MySqlConnector/1680?label=) | `DbType.Object` maps to `MySqlDbType.Null`, not `Vector`, so a prepared NULL parameter is accepted (2.5.0–2.6.2) |
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

The current released packages still show the findings addressed by MySQL Connector/NET PRs [#95–#99](https://github.com/mysql/mysql-connector-net/pulls?q=is%3Apr+author%3Aalaricd)
for `9.x`, [#100–#104](https://github.com/mysql/mysql-connector-net/pulls?q=is%3Apr+author%3Aalaricd)
for `8.x`, and [#105–#109](https://github.com/mysql/mysql-connector-net/pulls?q=is%3Apr+author%3Aalaricd)
for `trunk`. Each PR includes the required attribution:
“found using [pengdows.AdoNetApiTest.conformance](https://github.com/pengdows/pengdows.AdoNetApiTest.conformance)”.

`Connection.Open_works` also fails on the current source of every branch: `Open` sets the state to `Open` without an
event and then raises `Open → Open`. PRs [#110–#112](https://github.com/mysql/mysql-connector-net/pulls?q=is%3Apr+author%3Aalaricd)
fix it on `9.x`, `8.x` and `trunk`, with regression tests in `EventTests`. Built from `9.x` source with #95–#99 merged,
MySql.Data passed 121 of 122 Required tests; `Open_works` was the one left.

### System.Data.SQLite: [System.Data.SQLite forum report](https://system.data.sqlite.org/home/info/dc14fc92ed1d5a58)

System.Data.SQLite takes no pull requests. Per its [contribution page](https://system.data.sqlite.org/home/doc/trunk/www/contribute.wiki),
patches are emailed to drh@sqlite.org and are applied only with a signed Contributor Agreement on file (otherwise
"used for reference only"); bugs go to the System.Data.SQLite forum. The consolidated report is [System.Data.SQLite ADO.NET contract findings](https://system.data.sqlite.org/home/info/dc14fc92ed1d5a58).

`GetSchema()` and `GetSchemaAsync()` are also required for certification. The current Microsoft.Data.Sqlite run fails all
three connection-level schema checks because the base implementation throws `NotSupportedException`; pengdows.crud can
work around that limitation, but the workaround does not make the provider conformant. System.Data.SQLite passes these
schema checks in the current run. The corresponding Microsoft.Data.Sqlite work demonstrates that this is actionable provider work:
[dotnet/efcore#39188](https://github.com/dotnet/efcore/pull/39188) and
[dotnet/efcore#39219](https://github.com/dotnet/efcore/pull/39219). The pending forum report should be amended with this
finding when moderation makes the post editable.

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

#### Amendment: three data findings (System.Data.SQLite 1.0.119, confirmed 2026-10-22/23)

> 6. A `char` parameter with no `DbType` is stored as its code point: `'~'` is stored as the integer `126` and reads back
>    as a number. The [type inference table](https://learn.microsoft.com/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types)
>    infers `DbType.StringFixedLength` for `Char`. Found by `Dml.Text_column_stores_a_char_as_that_character`.
> 7. A `DateTime` declared `DbType.String` (or `DbType.DateTimeOffset`) is converted with the current culture:
>    `1/2/2024 11:45:00 AM` under en-US, `02.01.2024 13:04:05` under de-DE. The same row is stored differently on two
>    machines, and System.Data.SQLite's own reader then fails on it (`FormatException: String '1/2/2024 11:45:00 AM' was
>    not recognized as a valid DateTime`). Converting with the invariant culture (or ISO 8601, as for a `DateTime` value
>    with no `DbType`) would make it stable.
> 8. A `TimeSpan` declared `DbType.Time` throws `InvalidCastException: Unable to cast object of type 'System.TimeSpan' to
>    type 'System.IConvertible'`; with no `DbType` it is stored as `01:02:03`.

### ODP.NET (Oracle.ManagedDataAccess.Core): Oracle Support or the [ODP.NET forum](https://forums.oracle.com/ords/apexds/domain/dev-community/category/odp-dot-net)

> **OracleCommand.Prepare does not throw without an open connection**
>
> Oracle.ManagedDataAccess.Core 23.8 (Oracle Database 23ai Free): `Prepare()` on an `OracleCommand` with no connection, or
> a closed one, returns normally. [IDbCommand.Prepare](https://learn.microsoft.com/dotnet/api/system.data.idbcommand.prepare)
> documents `InvalidOperationException`: "The Connection is not set. -or- The Connection is not Open()." This is the only
> Required test ODP.NET fails in the pengdows ADO.NET conformance suite: [`Prepare_throws_when_no_connection`](https://github.com/pengdows/pengdows.AdoNetApiTest.conformance/blob/5d2276130647c1cd65f679d909a25352133c050a/src/pengdows.AdoNetApiTest.conformance/Required/CommandTestBase.Required.cs#L142).

> **A UTC DateTime declared DbType.DateTimeOffset is stored with the session's offset**
>
> ODP.NET 23 (Oracle Database 23ai Free): a `DateTime` with `Kind = Utc` (2024-01-02 11:45 UTC) bound with
> `DbType.DateTimeOffset` to a `TIMESTAMP WITH TIME ZONE` column is stored as `2024-01-02 11:45 -06:00`, the session's
> offset attached to the UTC wall time, which is a different instant. `new DateTimeOffset(dateTime)` honours `Kind` and
> gives `+00:00`. Found by the conformance suite's type-fidelity probe "DateTime declared DateTimeOffset".

### IBM Db2 (Net.IBM.Data.Db2-lnx): IBM Support

> **A TimeSpan outside a day is stored in a TIME column as another time**
>
> Net.IBM.Data.Db2-lnx 8.0.0.500 (IBM.Data.Db2) with Db2 LUW 11.5.8: a `TimeSpan` of `1.01:00:00` bound with
> `DbType.Time` to a `TIME` column is stored as `01:00:00`, with no error. A TIME holds a time of day, so a value outside
> `00:00:00`-`23:59:59` should be refused rather than wrapped. Found by the conformance suite's type-fidelity probe
> "beyond a day".
