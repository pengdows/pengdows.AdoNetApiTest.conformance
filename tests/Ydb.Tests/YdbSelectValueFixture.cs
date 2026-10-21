using System.Collections.Generic;
using System.Data;
using pengdows.AdoNetApiTest.conformance.Databases;

namespace Ydb.Tests;

/// <summary>YQL names its types after the CLR ones (<c>Utf8</c> is text, <c>String</c> bytes) and declares the primary key apart.</summary>
public sealed class YdbSelectValueFixture : SelectValueFixture<YdbDbFactoryFixture>
{
	protected override IReadOnlyDictionary<DbType, string> ColumnTypes { get; } = new Dictionary<DbType, string>
	{
		[DbType.Binary] = "String",
		[DbType.Double] = "Double",
		[DbType.Int16] = "Int16",
		[DbType.Int32] = "Int32",
		[DbType.Int64] = "Int64",
		[DbType.String] = "Utf8",
	};

	/// <summary>YQL has no <c>X'..'</c> literal; <c>String::HexDecode</c> gives the bytes.</summary>
	protected override string BinaryLiteral => "Unwrap(String::HexDecode('{0}'))";

	protected override string CreateTableSql(string table, string column) => $"CREATE TABLE {table} (Id Int32, V {column}, PRIMARY KEY (Id))";
}
