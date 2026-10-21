# Repository Guidelines

## Project Structure

- `src/pengdows.AdoNetApiTest.conformance/` contains reusable xUnit v3 base classes and fixtures for ADO.NET behavior.
- `src/pengdows.AdoNetApiTest.conformance.Databases/` contains database setup and SQL helpers, organized into `Configuration/`, `Lifecycle/`, and `Sql/`; `src/pengdows.AdoNetApiTest.conformance.Runner/` contains the `conformance` command-line runner (`run`, `report`) organized into `Execution/`, `Matrix/`, `Certification/`, and `Site/`.
- `tests/<Provider>.Tests/` contains provider-specific fixtures and derived tests (for example, `tests/Sqlite.Tests/`).
- Server-backed provider projects start their own MySQL, PostgreSQL or SQL Server container (`DatabaseServer.cs` in the Databases project), so Docker must be running. Solution-wide settings are in `Directory.Build.props`, `global.json`, and `pengdows.AdoNetApiTest.conformance.sln`.

## Build, Test, and Development Commands

Use the SDK version pinned by `global.json`:

```bash
dotnet restore pengdows.AdoNetApiTest.conformance.sln
dotnet build pengdows.AdoNetApiTest.conformance.sln
dotnet test tests/Repository.Tests/Repository.Tests.csproj
dotnet run --project src/pengdows.AdoNetApiTest.conformance.Runner -- run Sqlite
```

Run the individual provider test project relevant to a change. Database-backed suites need the matching service running and usually read `ConnectionString` from the environment. CI (GitHub Actions) builds, runs the repository guard tests and the runner tests, and publishes the results matrix.

## Coding Style & Naming

Use four-space indentation, standard C# formatting, nullable-safe code where applicable, and the existing file-scoped/project conventions. Name types and public members in `PascalCase`, locals and parameters in `camelCase`, and provider test classes with the provider prefix plus the behavior under test (for example, `SqliteDataReaderTests`). Keep shared behavior in `src/pengdows.AdoNetApiTest.conformance`; put provider-specific exceptions or overrides in that provider’s test project.

## Testing Guidelines

Tests use xUnit v3 with `Microsoft.NET.Test.Sdk`. Add or extend the appropriate `XTestBase<T>` coverage and use the existing fixture interfaces (`IDbFactoryFixture` and `ISelectValueFixture`). Test files conventionally end in `Tests.cs`; run the narrow project first, then the solution or affected provider matrix. There is no separate coverage threshold configured.

## Commits and Pull Requests

Use short, imperative commit subjects such as `Build with .NET 10.` or `Update local Docker images.` Keep unrelated changes separate. Pull requests should explain the behavioral change, identify affected providers, link the relevant issue when one exists, and include the test commands/results. Mention required database services or environment variables. Nothing is skipped; a test that does not apply is reported as not applicable or not implemented by the shared tests.

## Configuration and Security

Do not commit credentials or connection strings containing secrets. Use the containers the test projects start, or the `ConnectionString` environment variable, for database access; review `.github/workflows/build.yaml` before changing CI connection settings.
