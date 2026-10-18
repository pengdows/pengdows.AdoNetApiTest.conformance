using pengdows.AdoNetApiTest.conformance;

namespace FlatFile.Tests;

public sealed class FlatFileParameterTests : ParameterTestBase<FlatFileDbFactoryFixture>
{
    public FlatFileParameterTests(FlatFileDbFactoryFixture fixture)
        : base(fixture)
    {
    }
}
