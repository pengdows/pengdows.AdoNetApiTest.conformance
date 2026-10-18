using pengdows.AdoNetApiTest.conformance;

namespace FlatFile.Tests;

public sealed class FlatFileDbProviderFactoryTests : DbProviderFactoryTestBase<FlatFileDbFactoryFixture>
{
    public FlatFileDbProviderFactoryTests(FlatFileDbFactoryFixture fixture)
        : base(fixture)
    {
    }
}
