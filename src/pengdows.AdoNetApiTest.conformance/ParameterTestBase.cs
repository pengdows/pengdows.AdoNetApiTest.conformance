using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.IO;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class ParameterTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	protected ParameterTestBase(TFixture fixture)
		: base(fixture)
	{
	}

	private static void AssertNullParameterPolicy(Action action, string operation)
	{
		try
		{
			action();
		}
		catch (ArgumentNullException)
		{
			return;
		}
		catch (ArgumentException)
		{
			return;
		}
		catch (InvalidCastException)
		{
			SoftWarning.Report($"The provider rejects null for DbParameterCollection.{operation} with a provider-specific exception.");
			return;
		}
		catch (NullReferenceException)
		{
			SoftWarning.Report($"The provider does not define a portable null policy for DbParameterCollection.{operation}.");
			return;
		}

		SoftWarning.Report($"The provider accepts or ignores null for DbParameterCollection.{operation}; the base ADO.NET contract does not require rejection.");
	}

}
