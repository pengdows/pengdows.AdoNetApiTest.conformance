using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.v3;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Base for the three groups every shared test belongs to. The group is exposed as the <c>Category</c> trait, so a run
/// selects it with <c>--filter "Category=Required"</c>.
/// </summary>
public abstract class ConformanceFactAttribute : FactAttribute, ITraitAttribute
{
	private readonly string _group;

	private protected ConformanceFactAttribute(string group, string sourceFilePath, int sourceLineNumber)
		: base(sourceFilePath, sourceLineNumber)
	{
		_group = group;
	}

	/// <summary>The documentation page that states the behavior this test checks.</summary>
	public string Contract { get; set; }

	/// <summary>Why the test belongs to its group, when the group is not obvious from <see cref="Contract"/>.</summary>
	public string Because { get; set; }

	/// <inheritdoc />
	public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
		new[] { new KeyValuePair<string, string>("Category", _group) };
}

/// <summary>
/// The documented <c>System.Data.Common</c> contract requires this behavior from every provider that exposes the API.
/// A failure is a hard conformance failure.
/// </summary>
public sealed class RequiredFactAttribute : ConformanceFactAttribute
{
	public RequiredFactAttribute([CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
		: base("Required", sourceFilePath, sourceLineNumber)
	{
	}
}

/// <summary>
/// The behavior belongs to a capability a provider may decline. The test skips when the fixture reports the capability
/// as unsupported; when it runs, a failure is a hard conformance failure.
/// </summary>
public sealed class OptionalFactAttribute : ConformanceFactAttribute
{
	public OptionalFactAttribute([CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
		: base("Optional", sourceFilePath, sourceLineNumber)
	{
	}
}

/// <summary>
/// Interoperability behavior most providers share but the contract does not require. A difference is advisory: it is
/// reported (often as a <c>[SOFT WARNING]</c>) and never counts toward the hard pass or failure.
/// </summary>
public sealed class CommonFactAttribute : ConformanceFactAttribute
{
	public CommonFactAttribute([CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
		: base("Common", sourceFilePath, sourceLineNumber)
	{
	}
}
