using System;
using System.Data.Common;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class DbProviderFactoryTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	protected DbProviderFactoryTestBase(TFixture fixture)
		: base(fixture)
	{
	}

	/// <summary>The capability flag must agree with whether the factory created the object.</summary>
	private static void AssertCapabilityMatches(bool capability, bool created, string feature)
	{
		if (capability)
		{
			Assert.True(created, $"The provider advertises {feature} support but did not create it.");
			return;
		}

		Assert.False(created, $"The provider created {feature} support while advertising it as unavailable.");
	}

	/// <summary>
	/// Checks the agreement, then records "not implemented" when the provider does not expose the capability.
	/// Returns whether the capability exists, so the caller knows whether to go on.
	/// </summary>
	private static bool AssertOptionalFactoryObject(bool capability, bool created, string feature)
	{
		AssertCapabilityMatches(capability, created, feature);
		if (capability)
		{
			return true;
		}

		ConformanceOutcome.NotImplemented($"The provider does not expose the optional {feature} capability.");
		return false;
	}
}
