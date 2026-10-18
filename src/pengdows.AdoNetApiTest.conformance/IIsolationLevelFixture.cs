using System.Data;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Identifies an isolation level that the configured provider supports for an
/// explicit transaction request.
/// </summary>
public interface IIsolationLevelFixture : IDbFactoryFixture
{
	IsolationLevel SupportedIsolationLevel { get; }
}
