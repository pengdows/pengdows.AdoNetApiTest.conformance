using System.Collections.Generic;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>A type to test that the provider's <c>DataTypes</c> collection may not list.</summary>
public sealed class ExtraType
{
	public ExtraType(string name, string clrType, string columnSql)
	{
		Name = name;
		ClrType = clrType;
		ColumnSql = columnSql;
	}

	/// <summary>The type's name as the database lists it, such as <c>BOOLEAN</c>.</summary>
	public string Name { get; }

	/// <summary>The CLR type a faithful provider returns for it, by name, such as <c>System.Boolean</c>.</summary>
	public string ClrType { get; }

	/// <summary>The type as written in a column definition; the name when <c>null</c>.</summary>
	public string ColumnSql { get; }
}

/// <summary>
/// Optional on a fixture that knows more about the server's types than the provider's <c>DataTypes</c> collection says.
/// Most providers ship a fixed list that describes the server they were written for (ODP.NET lists no BOOLEAN or JSON
/// for Oracle 23ai, FirebirdClient none of Firebird 4's types), so the server's own list shows what it leaves out.
/// </summary>
public interface IServerTypesFixture
{
	/// <summary>
	/// A query returning each type the server supports, or <c>null</c>: its name first and, optionally, a column
	/// definition second (<c>decimal(28,10)</c>) for a type that needs parameters. A type the provider's <c>DataTypes</c>
	/// does not list is tested anyway, as the CLR type the driver itself reports for a column of it (<c>GetFieldType</c>).
	/// </summary>
	string ServerTypesSql { get; }

	/// <summary>Types to test as well as those <c>DataTypes</c> lists.</summary>
	IReadOnlyList<ExtraType> ExtraTypes { get; }

	/// <summary>Types not to test, by name, each with the reason.</summary>
	IReadOnlyDictionary<string, string> ExcludedTypes { get; }
}
