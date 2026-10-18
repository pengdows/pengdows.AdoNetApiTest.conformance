using System.Threading.Tasks;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Tests the documented ChangeDatabase contract for providers that explicitly
/// advertise a valid alternate database through <see cref="IChangeDatabaseFixture"/>.
/// </summary>
public abstract partial class ChangeDatabaseTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IChangeDatabaseFixture
{
	protected ChangeDatabaseTestBase(TFixture fixture)
		: base(fixture)
	{
	}

}
