using pengdows.AdoNetApiTest.conformance;

namespace FlatFile.Tests;

public sealed class FlatFileTransactionTests : TransactionTestBase<FlatFileDbFactoryFixture>
{
    public FlatFileTransactionTests(FlatFileDbFactoryFixture fixture)
        : base(fixture)
    {
    }
}
