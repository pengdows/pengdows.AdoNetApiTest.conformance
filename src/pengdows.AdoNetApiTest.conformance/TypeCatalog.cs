using System;
using System.Collections.Generic;
using System.Data;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// An expected value known only by its text, for a type whose faithful CLR form lives in the provider's own assembly (an
/// <c>inet</c> with its netmask is Npgsql's <c>NpgsqlInet</c>). The value read back matches when its invariant
/// <c>ToString()</c> is <see cref="Text"/>.
/// </summary>
public sealed class ClrText
{
	public ClrText(string text) => Text = text;

	public string Text { get; }

	public override string ToString() => Text;
}

/// <summary>One value to push through a provider for a type, and what must come back.</summary>
public sealed class TypeValue
{
	public TypeValue(string label, string literal, object expected)
	{
		Label = label;
		Literal = literal;
		Expected = expected;
	}

	/// <summary>A short name for the value, such as <c>max</c>, <c>empty</c> or <c>non-BMP</c>.</summary>
	public string Label { get; }

	/// <summary>
	/// A SQL expression that produces the value on the database side, with no parameter involved; <c>null</c> when there
	/// is no reliable literal, and the value then takes only the <c>roundtrip</c> path.
	/// </summary>
	public string Literal { get; }

	/// <summary>The CLR value <c>GetValue</c> returns for it. <c>null</c> means SQL NULL.</summary>
	public object Expected { get; }

	/// <summary>The value <c>GetFieldValue&lt;T&gt;</c> returns, for a provider whose <c>GetValue</c> hands back a storage form.</summary>
	public object ExpectedTyped => HasTyped ? _typed : Expected;

	private object _typed;

	/// <summary>True when <see cref="ExpectedTyped"/> differs from <see cref="Expected"/>.</summary>
	public bool HasTyped { get; private set; }

	/// <summary>
	/// For a provider whose <c>GetValue</c> returns the storage form (Microsoft.Data.Sqlite returns the text of a date),
	/// the CLR value the typed read must produce.
	/// </summary>
	public TypeValue TypedAs(object typed)
	{
		_typed = typed;
		HasTyped = true;
		return this;
	}

	/// <summary>
	/// The value to bind as a parameter when it differs from <see cref="Expected"/>: <c>"abc"</c> bound to a <c>char(10)</c>
	/// comes back padded, and a date with finer ticks than the type keeps comes back rounded.
	/// </summary>
	public object Bound { get; private set; }

	/// <summary>True when <see cref="Bound"/> is set, even to <c>null</c>.</summary>
	public bool HasBound { get; private set; }

	/// <summary>Binds <paramref name="bound"/> instead of <see cref="Expected"/>.</summary>
	public TypeValue Binding(object bound)
	{
		Bound = bound;
		HasBound = true;
		return this;
	}

	/// <summary>The value a parameter is given.</summary>
	public object ParameterValue => HasBound ? Bound : Expected;
}

/// <summary>A native database type and the values that show whether a provider handles it faithfully.</summary>
public sealed class TypeSpec
{
	public TypeSpec(string name, string columnSql, Type clrType, DbType dbType, params TypeValue[] values)
	{
		Name = name;
		ColumnSql = columnSql;
		ClrType = clrType;
		DbType = dbType;
		Values = values;
	}

	/// <summary>The native type name as the database lists it (the <c>DataTypes</c> collection's <c>TypeName</c>), such as <c>decimal</c>.</summary>
	public string Name { get; }

	/// <summary>The type as written in a column definition, such as <c>decimal(18,4)</c>.</summary>
	public string ColumnSql { get; }

	/// <summary>The CLR type <c>GetFieldType</c> and <c>GetValue</c> are expected to return for the column.</summary>
	public Type ClrType { get; }

	private Type _typedClrType;

	/// <summary>The CLR type the typed read (<c>GetFieldValue&lt;T&gt;</c>) is expected to produce; <see cref="ClrType"/> unless set.</summary>
	public Type TypedClrType
	{
		get => _typedClrType ?? ClrType;
		set => _typedClrType = value;
	}

	private string _display;

	/// <summary>The key a finding is reported under; the column definition unless set (one column type can be bound as several CLR types).</summary>
	public string Display
	{
		get => _display ?? ColumnSql;
		set => _display = value;
	}

	/// <summary>
	/// A column definition to use when the database rejects <see cref="ColumnSql"/>, as when a provider's
	/// <c>DataTypes</c> collection advertises a <c>CreateFormat</c> its database refuses. The rejection is a finding.
	/// </summary>
	public string FallbackColumnSql { get; set; }

	/// <summary>False when the database defines no text for the stored value, so the <c>write</c> path has nothing to compare against.</summary>
	public bool WriteOracle { get; set; } = true;

	/// <summary>True when the storage form of the value is the provider's own choice, so only the typed read is judged.</summary>
	public bool RawFormatIsProviderDefined { get; set; }

	/// <summary>True when the case of text is not significant (a GUID's hex digits).</summary>
	public bool IgnoreCase { get; set; }

	/// <summary>True to leave <see cref="DbType"/> unset when binding a value, so the provider infers it; a NULL still gets <see cref="DbType"/>.</summary>
	public bool InferDbType { get; set; }

	/// <summary>
	/// True for a spec generated from the provider's <c>DataTypes</c> collection, whose values are chosen without knowing
	/// the database's range: a value the database rejects even as a literal is then not applicable, not a finding.
	/// </summary>
	public bool Generated { get; set; }

	/// <summary>The <see cref="System.Data.DbType"/> to bind a parameter of this type with.</summary>
	public DbType DbType { get; }

	public int? Size { get; set; }

	public byte? Precision { get; set; }

	public byte? Scale { get; set; }

	/// <summary>False for a type the database cannot compare with <c>=</c> in a WHERE clause.</summary>
	public bool Comparable { get; set; } = true;

	/// <summary>SQL that renders the column as text on the database side, with <c>{0}</c> for the column name.</summary>
	public string RenderAsText { get; set; } = "CAST({0} AS nvarchar(max))";

	public IReadOnlyList<TypeValue> Values { get; }
}

/// <summary>A type the database offers that the catalog does not exercise, and why.</summary>
public sealed class UncoveredType
{
	public UncoveredType(string name, string reason)
	{
		Name = name;
		Reason = reason;
	}

	public string Name { get; }

	public string Reason { get; }
}

/// <summary>What a provider's fixture supplies to check type fidelity.</summary>
public interface ITypeFidelityFixture : IDbFactoryFixture, IParameterMarkerFixture
{
	/// <summary>The types to exercise.</summary>
	IReadOnlyList<TypeSpec> Types { get; }

	/// <summary>Types the database offers that <see cref="Types"/> does not cover, each with the reason.</summary>
	IReadOnlyList<UncoveredType> UncoveredTypes { get; }

	/// <summary>A statement that creates <paramref name="table"/> with an <c>Id</c> primary key and a nullable column <c>V</c> of the given type.</summary>
	string CreateTableSql(string table, string columnSql);

	/// <summary>A statement that drops <paramref name="table"/> if it exists.</summary>
	string DropTableSql(string table);
}
