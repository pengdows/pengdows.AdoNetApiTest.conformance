using pengdows.AdoNetApiTest.conformance;

namespace Ydb.Tests;

public sealed class YdbConnectionStringBuilderTests : ConnectionStringTestBase<YdbDbFactoryFixture>
{
    public YdbConnectionStringBuilderTests(YdbDbFactoryFixture fixture) : base(fixture) { }
}

public sealed class YdbConnectionTests : ConnectionTestBase<YdbDbFactoryFixture>
{
    public YdbConnectionTests(YdbDbFactoryFixture fixture) : base(fixture) { }
}

public sealed class YdbCommandTests : CommandTestBase<YdbDbFactoryFixture>
{
    public YdbCommandTests(YdbDbFactoryFixture fixture) : base(fixture) { }
}

public sealed class YdbParameterTests : ParameterTestBase<YdbDbFactoryFixture>
{
    public YdbParameterTests(YdbDbFactoryFixture fixture) : base(fixture) { }
}

public sealed class YdbProviderFactoryTests : DbProviderFactoryTestBase<YdbDbFactoryFixture>
{
    public YdbProviderFactoryTests(YdbDbFactoryFixture fixture) : base(fixture) { }
}

public sealed class YdbDmlTests : DmlTestBase<YdbDbFactoryFixture>
{
    public YdbDmlTests(YdbDbFactoryFixture fixture) : base(fixture) { }
}

public sealed class YdbDataReaderTests : DataReaderTestBase<YdbSelectValueFixture>
{
    public YdbDataReaderTests(YdbSelectValueFixture fixture) : base(fixture) { }
}

public sealed class YdbGetValueConversionTests : GetValueConversionTestBase<YdbSelectValueFixture>
{
    public YdbGetValueConversionTests(YdbSelectValueFixture fixture) : base(fixture) { }
}

public sealed class YdbTransactionTests : TransactionTestBase<YdbDbFactoryFixture>
{
    public YdbTransactionTests(YdbDbFactoryFixture fixture) : base(fixture) { }
}
