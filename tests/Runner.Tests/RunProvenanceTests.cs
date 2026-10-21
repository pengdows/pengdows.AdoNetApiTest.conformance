using pengdows.AdoNetApiTest.conformance.Runner;
using Xunit;

namespace Runner.Tests;

public sealed class RunProvenanceTests : IDisposable
{
	private readonly string m_directory = Path.Combine(Path.GetTempPath(), "conformance-provenance-" + Guid.NewGuid().ToString("N"));

	public RunProvenanceTests() => Directory.CreateDirectory(m_directory);

	public void Dispose() => Directory.Delete(m_directory, recursive: true);

	[Fact]
	public void Malformed_provenance_fails_closed()
	{
		var result = Path.Combine(m_directory, "Example.xml");
		File.WriteAllText(RunProvenance.FileFor(result), "{ not json");

		var exception = Assert.Throws<InvalidDataException>(() => RunProvenance.Read(result, null));

		Assert.Contains("is not valid JSON", exception.Message);
	}

	[Fact]
	public void Non_object_provenance_fails_closed()
	{
		var result = Path.Combine(m_directory, "Example.xml");
		File.WriteAllText(RunProvenance.FileFor(result), "[]");

		var exception = Assert.Throws<InvalidDataException>(() => RunProvenance.Read(result, null));

		Assert.Contains("must contain a JSON object", exception.Message);
	}
}
