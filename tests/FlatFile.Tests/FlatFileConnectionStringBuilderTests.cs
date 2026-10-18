using pengdows.AdoNetApiTest.conformance;

namespace FlatFile.Tests;

public sealed class FlatFileConnectionStringBuilderTests : ConnectionStringTestBase<FlatFileDbFactoryFixture>
{
    public FlatFileConnectionStringBuilderTests(FlatFileDbFactoryFixture fixture)
        : base(fixture)
    {
    }
}
