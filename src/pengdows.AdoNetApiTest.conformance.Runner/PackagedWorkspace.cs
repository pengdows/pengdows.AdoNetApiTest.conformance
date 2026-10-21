namespace pengdows.AdoNetApiTest.conformance.Runner;

/// <summary>Owns the temporary source tree used by the packaged runner.</summary>
internal sealed class PackagedWorkspace : IDisposable
{
	private PackagedWorkspace(string root)
	{
		Root = root;
		TestsDirectory = Path.Combine(root, "tests");
		Directory.CreateDirectory(TestsDirectory);
	}

	public string Root { get; }

	public string TestsDirectory { get; }

	public static PackagedWorkspace Create()
	{
		var root = Path.Combine(Path.GetTempPath(), "pengdows-conformance", Guid.NewGuid().ToString("N"));
		return new PackagedWorkspace(root);
	}

	public void Dispose()
	{
		Directory.Delete(Root, recursive: true);
	}
}
