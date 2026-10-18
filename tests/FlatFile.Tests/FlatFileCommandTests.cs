using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace FlatFile.Tests;

public sealed class FlatFileCommandTests : CommandTestBase<FlatFileDbFactoryFixture>
{
    public FlatFileCommandTests(FlatFileDbFactoryFixture fixture)
        : base(fixture)
    {
    }
}

public sealed class FlatFileDmlTests : DmlTestBase<FlatFileDbFactoryFixture>
{
    public FlatFileDmlTests(FlatFileDbFactoryFixture fixture)
        : base(fixture)
    {
    }
}
