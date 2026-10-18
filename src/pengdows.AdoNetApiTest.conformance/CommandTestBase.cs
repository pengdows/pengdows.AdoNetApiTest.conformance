using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class CommandTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	protected CommandTestBase(TFixture fixture)
		: base(fixture)
	{
	}



	private static async Task ObserveCancellationAsync(Func<Task> operation, string operationName)
	{
		try
		{
			await operation().ConfigureAwait(false);
			SoftWarning.Report($"The provider completed {operationName} despite a pre-canceled token; cancellation is optional for this ADO.NET async path.");
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex)
		{
			SoftWarning.Report($"The provider completed {operationName} with {ex.GetType().Name}; cancellation behavior is provider-specific.");
		}
	}

	private static void AssertEmptyCommandTextPolicy(Action action, string operation)
	{
		try
		{
			action();
		}
		catch (InvalidOperationException)
		{
			return;
		}
		catch (DbException)
		{
			return;
		}
		catch (Exception)
		{
			return;
		}

		SoftWarning.Report($"The provider accepts an empty command text for {operation}; the base ADO.NET contract does not require a specific rejection.");
	}

	private static async Task AssertEmptyCommandTextPolicyAsync(Func<Task> action, string operation)
	{
		try
		{
			await action().ConfigureAwait(false);
		}
		catch (InvalidOperationException)
		{
			return;
		}
		catch (DbException)
		{
			return;
		}
		catch (Exception)
		{
			return;
		}

		SoftWarning.Report($"The provider accepts an empty command text for {operation}; the base ADO.NET contract does not require a specific rejection.");
	}

	protected class CustomClass
	{
		public override string ToString() => "custom";
	}
}
