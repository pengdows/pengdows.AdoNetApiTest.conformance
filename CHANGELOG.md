# Changelog

Changes since the fork from [mysql-net/AdoNetApiTest](https://github.com/mysql-net/AdoNetApiTest). The original
project's release notes are in [UPSTREAM-VERSION-HISTORY.md](UPSTREAM-VERSION-HISTORY.md).

## Unreleased

* Renamed to `pengdows.AdoNetApiTest.conformance`; copyright line for Pengdows added to the MIT license, upstream
  notice kept.
* Every shared test is Required, Optional or Common, each naming the documentation page it is based on.
* Nothing is skipped: optional capabilities a provider lacks pass as `not-implemented`.
* Added the `conformance` runner: a results matrix (JSON and XML) and a simple PASS/FAIL.
* Added type-fidelity tests (SQL Server through every SqlClient, both SQLite clients).
* Added flatfile, DuckDB, Microsoft.Data.SqlClient 2-5, Microsoft.Data.Sqlite 5-8 and Npgsql 4-8 test projects.
* Removed the strong-name signing (the upstream `MySqlConnector.snk` key) and the Azure Pipelines definition.
