using System;
using System.Collections.Generic;
using System.Data;

namespace pengdows.AdoNetApiTest.conformance;

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

	/// <summary>A SQL expression that produces the value on the database side, with no parameter involved.</summary>
	public string Literal { get; }

	/// <summary>The CLR value a correct provider returns for it. <c>null</c> means SQL NULL.</summary>
	public object Expected { get; }

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

	/// <summary>The CLR type a provider is expected to return for the column.</summary>
	public Type ClrType { get; }

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
public interface ITypeFidelityFixture : IDbFactoryFixture
{
	/// <summary>The types to exercise.</summary>
	IReadOnlyList<TypeSpec> Types { get; }

	/// <summary>Types the database offers that <see cref="Types"/> does not cover, each with the reason.</summary>
	IReadOnlyList<UncoveredType> UncoveredTypes { get; }

	/// <summary>A parameter name as it appears in SQL, such as <c>@p</c>.</summary>
	string ParameterMarker(string name);

	/// <summary>A statement that creates <paramref name="table"/> with an <c>Id</c> primary key and a nullable column <c>V</c> of the given type.</summary>
	string CreateTableSql(string table, string columnSql);

	/// <summary>A statement that drops <paramref name="table"/> if it exists.</summary>
	string DropTableSql(string table);
}
