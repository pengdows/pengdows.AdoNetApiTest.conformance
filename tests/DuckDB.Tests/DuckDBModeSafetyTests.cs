using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DuckDB.NET.Data;
using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace DuckDB.Tests;

public sealed class DuckDBModeSafetyTests
{
    [ModeSafetyFact]
    public async Task FileBackedDuckDb_ReportsSafeModes()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pengdows_mode_probe_{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={path}";
        try
        {
            var standard = await ModeSafetyProbe.ConcurrentWritesAsync(
                DuckDBClientFactory.Instance, connectionString,
                "CREATE TABLE mode_probe (id INTEGER PRIMARY KEY, value INTEGER)",
                "INSERT INTO mode_probe VALUES (1, 0)",
                "SELECT value FROM mode_probe WHERE id = 1",
                "UPDATE mode_probe SET value = value + 1 WHERE id = 1");
            Record("DuckDB", "Standard", standard);

            var singleWriter = await ModeSafetyProbe.SerializedWritesAsync(
                DuckDBClientFactory.Instance, connectionString,
                "CREATE TABLE mode_probe_single (id INTEGER PRIMARY KEY, value INTEGER)",
                "INSERT INTO mode_probe_single VALUES (1, 0)",
                "UPDATE mode_probe_single SET value = value + 1 WHERE id = 1");
            Record("DuckDB", "SingleWriter", singleWriter);

            ModeSafetyOutcome.Record("DuckDB", "PreventDatabaseUnload", standard.Safe ? "not-tested" : "unsafe",
                standard.Safe ? "idle unload requires a provider-specific lifecycle probe" : "requires Standard-mode concurrency safety first");
            ModeSafetyOutcome.Record("DuckDB", "SingleConnection", "not-tested", "this probe uses a file-backed database; test :memory: separately");
            var safeModes = new List<string>();
            if (standard.Safe) safeModes.Add("Standard");
            if (singleWriter.Safe) safeModes.Add("SingleWriter");
            ModeSafetyOutcome.Record("DuckDB", "safe-modes", string.Join(", ", safeModes), "independent mode probes completed");
        }
        finally
        {
            File.Delete(path);
        }
    }

    private void Record(string database, string mode, ModeSafetyProbeResult result)
    {
        ModeSafetyOutcome.Record(database, mode, result.Safe ? "safe" : "unsafe", result.Detail);
    }
}
