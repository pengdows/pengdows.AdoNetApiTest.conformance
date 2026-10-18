using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Repository.Tests;

public class RenameTests
{
	// The library was forked from mysql-net/AdoNetApiTest and renamed. These names may only appear where the history
	// or the credit to the original project is the point.
	private static readonly string[] s_oldNames =
	{
		"AdoNet.Specification.Tests",
		"AdoNet.Databases",
	};

	private static readonly string[] s_exemptFiles = { "VersionHistory.md", "LICENSE", "README.md", "RenameTests.cs" };

	private static readonly string[] s_skippedDirectories = { ".git", "bin", "obj", "TestResults" };

	private static readonly string[] s_textExtensions =
		{ ".cs", ".csproj", ".props", ".sln", ".md", ".yml", ".yaml", ".json", ".config", ".targets", ".editorconfig" };

	[Fact]
	public void NoFileStillUsesAnOldProjectName()
	{
		var root = FindRepositoryRoot();
		var offenders = TextFiles(root)
			.Where(path => !s_exemptFiles.Contains(Path.GetFileName(path)))
			.SelectMany(path => File.ReadLines(path)
				.Select((line, index) => (path, line, number: index + 1))
				.Where(x => s_oldNames.Any(name => x.line.Contains(name, StringComparison.Ordinal))))
			.Select(x => $"{Path.GetRelativePath(root, x.path)}:{x.number}")
			.ToList();

		Assert.True(offenders.Count == 0, "Old project names remain in: " + string.Join(", ", offenders.Take(20)));
	}

	[Fact]
	public void NoOldProjectDirectoryOrFileRemains()
	{
		var root = FindRepositoryRoot();
		var offenders = Directory.EnumerateFileSystemEntries(root, "*", SearchOption.AllDirectories)
			.Where(path => !Segments(root, path).Any(s_skippedDirectories.Contains))
			.Where(path => s_oldNames.Any(name => Path.GetFileName(path).StartsWith(name, StringComparison.Ordinal)))
			.Select(path => Path.GetRelativePath(root, path))
			.ToList();

		Assert.True(offenders.Count == 0, "Old project names remain in paths: " + string.Join(", ", offenders));
	}

	private static IEnumerable<string> Segments(string root, string path) =>
		Path.GetRelativePath(root, path).Split(Path.DirectorySeparatorChar);

	private static IEnumerable<string> TextFiles(string root) =>
		Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
			.Where(path => !Segments(root, path).Any(s_skippedDirectories.Contains))
			.Where(path => s_textExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase));

	private static string FindRepositoryRoot()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);
		while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
		{
			directory = directory.Parent;
		}

		return directory?.FullName ?? throw new InvalidOperationException("global.json not found above " + AppContext.BaseDirectory);
	}
}
