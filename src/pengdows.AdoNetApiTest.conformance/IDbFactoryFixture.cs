using System.Data.Common;

namespace pengdows.AdoNetApiTest.conformance;

public interface IDbFactoryFixture
{
	DbProviderFactory Factory { get; }
	string ConnectionString { get; }
}
