using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Small provider-neutral probes for database-mode diagnostics. These probes return evidence; callers should record
/// the result with <see cref="ModeSafetyOutcome"/> and must not turn an expected unsafe mode into a conformance failure.
/// </summary>
public static class ModeSafetyProbe
{
	public static async Task<ModeSafetyProbeResult> SerializedWritesAsync(
		DbProviderFactory factory,
		string connectionString,
		string createSql,
		string seedSql,
		string updateSql,
		int writerCount = 8)
	{
		try
		{
			using (var setup = Create(factory, connectionString))
			{
				await setup.OpenAsync().ConfigureAwait(false);
				await ExecuteAsync(setup, createSql).ConfigureAwait(false);
				await ExecuteAsync(setup, seedSql).ConfigureAwait(false);
			}

			using (var gate = new SemaphoreSlim(1, 1))
		{
				var failures = new List<Exception>();
				var tasks = new Task[writerCount];
				for (var i = 0; i < writerCount; i++)
				{
					tasks[i] = Task.Run(async () =>
					{
						await gate.WaitAsync().ConfigureAwait(false);
						try
						{
							using (var connection = Create(factory, connectionString))
							{
								await connection.OpenAsync().ConfigureAwait(false);
								await ExecuteAsync(connection, updateSql).ConfigureAwait(false);
							}
						}
						catch (Exception exception)
						{
							lock (failures)
								failures.Add(exception);
						}
						finally
						{
							gate.Release();
						}
					});
				}

				await Task.WhenAll(tasks).ConfigureAwait(false);
				return failures.Count == 0
					? new ModeSafetyProbeResult(true, $"{writerCount} writes completed through a one-writer gate")
					: new ModeSafetyProbeResult(false, $"{failures.Count}/{writerCount} serialized writers failed: {failures[0].GetType().Name}: {failures[0].Message}");
			}
		}
		catch (Exception exception)
		{
			return new ModeSafetyProbeResult(false, $"probe setup failed: {exception.GetType().Name}: {exception.Message}");
		}
	}

	public static async Task<ModeSafetyProbeResult> ConcurrentWritesAsync(
		DbProviderFactory factory,
		string connectionString,
		string createSql,
		string seedSql,
		string readSql,
		string updateSql,
		int writerCount = 8,
		TimeSpan? timeout = null)
	{
		try
		{
			using (var setup = Create(factory, connectionString))
			{
				await setup.OpenAsync().ConfigureAwait(false);
				await ExecuteAsync(setup, createSql).ConfigureAwait(false);
				await ExecuteAsync(setup, seedSql).ConfigureAwait(false);
			}

			var release = new TaskCompletionSource<bool>();
			var arrived = 0;
			var failures = new List<Exception>();
			var gate = new object();
			var tasks = new Task[writerCount];
			for (var i = 0; i < writerCount; i++)
			{
				tasks[i] = Task.Run(async () =>
				{
					try
					{
						using (var connection = Create(factory, connectionString))
						{
							await connection.OpenAsync().ConfigureAwait(false);
							using (var transaction = connection.BeginTransaction())
							{
								await ExecuteAsync(connection, readSql, transaction).ConfigureAwait(false);
								if (Interlocked.Increment(ref arrived) == writerCount)
									release.TrySetResult(true);
								await release.Task.ConfigureAwait(false);
								await ExecuteAsync(connection, updateSql, transaction).ConfigureAwait(false);
								transaction.Commit();
							}
						}
					}
					catch (Exception exception)
					{
						lock (gate)
							failures.Add(exception);
						if (Interlocked.Increment(ref arrived) == writerCount)
							release.TrySetResult(false);
					}
				});
			}

			var all = Task.WhenAll(tasks);
			var completed = await Task.WhenAny(all, Task.Delay(timeout ?? TimeSpan.FromSeconds(30))).ConfigureAwait(false);
			if (completed != all)
			{
				release.TrySetResult(false);
				return new ModeSafetyProbeResult(false, "concurrent writer probe timed out; at least one writer could not complete");
			}

			await all.ConfigureAwait(false);
			return failures.Count == 0
				? new ModeSafetyProbeResult(true, $"{writerCount} concurrent transactions committed successfully")
				: new ModeSafetyProbeResult(false, $"{failures.Count}/{writerCount} concurrent writers failed: {failures[0].GetType().Name}: {failures[0].Message}");
		}
		catch (Exception exception)
		{
			return new ModeSafetyProbeResult(false, $"probe setup failed: {exception.GetType().Name}: {exception.Message}");
		}
	}

	private static DbConnection Create(DbProviderFactory factory, string connectionString)
	{
		var connection = factory.CreateConnection() ?? throw new InvalidOperationException("The provider returned a null DbConnection.");
		connection.ConnectionString = connectionString;
		return connection;
	}

	private static async Task ExecuteAsync(DbConnection connection, string sql, DbTransaction transaction = null)
	{
		using (var command = connection.CreateCommand())
		{
			command.CommandText = sql;
			command.Transaction = transaction;
			await command.ExecuteNonQueryAsync().ConfigureAwait(false);
		}
	}
}

public sealed class ModeSafetyProbeResult
{
	public ModeSafetyProbeResult(bool safe, string detail)
	{
		Safe = safe;
		Detail = detail;
	}

	public bool Safe { get; }
	public string Detail { get; }
}
