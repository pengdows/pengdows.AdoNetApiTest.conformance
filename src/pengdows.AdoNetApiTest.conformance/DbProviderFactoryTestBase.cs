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

	private static void AssertOptionalFactoryObject(bool capability, bool created, string feature)
	{
		if (capability)
		{
			Assert.True(created, $"The provider advertises {feature} support but did not create it.");
			return;
		}

		Assert.False(created, $"The provider created {feature} support while advertising it as unavailable.");

		throw Xunit.Sdk.SkipException.ForSkip($"The provider does not expose the optional {feature} capability.");
	}
}
