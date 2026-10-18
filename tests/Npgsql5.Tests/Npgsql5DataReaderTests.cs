using pengdows.AdoNetApiTest.conformance;
using Xunit;

namespace Npgsql5.Tests;

public sealed class Npgsql5DataReaderTests : DataReaderTestBase<Npgsql5SelectValueFixture>
{
	public Npgsql5DataReaderTests(Npgsql5SelectValueFixture fixture)
		: base(fixture)
	{
	}
}
