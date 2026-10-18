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

public sealed class FlatFileDataReaderTests : DataReaderTestBase<FlatFileSelectValueFixture>
{
	public FlatFileDataReaderTests(FlatFileSelectValueFixture fixture)
		: base(fixture)
	{
	}
}

public sealed class FlatFileGetValueConversionTests : GetValueConversionTestBase<FlatFileSelectValueFixture>
{
	public FlatFileGetValueConversionTests(FlatFileSelectValueFixture fixture)
		: base(fixture)
	{
	}
}
