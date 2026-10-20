using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.IO;
using pengdows.AdoNetApiTest.conformance;
using pengdows.flatfile;

namespace FlatFile.Tests;

/// <summary>
/// A typed <c>select_value</c> table with a row per <see cref="ValueKind"/>, as the other providers have, so the data reader and
/// conversion suites run against pengdows.flatfile. flatfile has no binary literal, so the two byte arrays the reader tests ask
/// for are rows of the table (<c>Id</c> 100 and 101) and <see cref="CreateSelectSql(byte[])"/> looks them up.
/// </summary>
public sealed class FlatFileSelectValueFixture : IDbFactoryFixture, ISingleWriterFixture, ISelectValueFixture, IDeleteFixture, IDisposable
{
	private static readonly Dictionary<string, int> s_byteRows = new()
	{
		["01020304"] = 100,
		["7E57"] = 101,
	};

	private readonly string _root = Path.Combine(Path.GetTempPath(), "adonet-flatfile-values-" + Guid.NewGuid().ToString("N"));

	public FlatFileSelectValueFixture()
	{
		Directory.CreateDirectory(_root);
		File.WriteAllText(Path.Combine(_root, "select_value.table.json"), """
			{
			  "name": "select_value",
			  "path": "select_value.jsonl",
			  "format": "Json",
			  "json": {},
			  "columns": [
			    { "name": "Id", "clrType": "int", "allowNull": false, "isPrimaryKey": true },
			    { "name": "Binary", "clrType": "byte[]" },
			    { "name": "Boolean", "clrType": "bool" },
			    { "name": "Byte", "clrType": "byte" },
			    { "name": "DateTime", "clrType": "DateTime" },
			    { "name": "DateTimeOffset", "clrType": "DateTimeOffset" },
			    { "name": "Decimal", "clrType": "decimal" },
			    { "name": "Double", "clrType": "double" },
			    { "name": "Guid", "clrType": "Guid" },
			    { "name": "Int16", "clrType": "short" },
			    { "name": "Int32", "clrType": "int" },
			    { "name": "Int64", "clrType": "long" },
			    { "name": "Single", "clrType": "float" },
			    { "name": "String", "clrType": "string" }
			  ]
			}
			""");
		File.WriteAllText(Path.Combine(_root, "select_value.jsonl"), string.Join("\n", Rows) + "\n");
	}

	// Id = (int) ValueKind: Null 0, Empty 1, Zero 2, One 3, Minimum 4, Maximum 5.
	private static readonly string[] Rows =
	{
		"""{"Id":0}""",
		"""{"Id":1,"Binary":"","String":""}""",
		"""{"Id":2,"Binary":"AA==","Boolean":"false","Byte":"0","Decimal":"0","Double":"0","Guid":"00000000-0000-0000-0000-000000000000","Int16":"0","Int32":"0","Int64":"0","Single":"0","String":"0"}""",
		"""{"Id":3,"Binary":"AQ==","Boolean":"true","Byte":"1","DateTime":"1111-11-11T11:11:11.111","DateTimeOffset":"1111-11-11T11:11:11.111+11:11","Decimal":"1","Double":"1","Guid":"11111111-1111-1111-1111-111111111111","Int16":"1","Int32":"1","Int64":"1","Single":"1","String":"1"}""",
		"""{"Id":4,"Boolean":"false","Byte":"0","DateTime":"0001-01-01T00:00:00","DateTimeOffset":"0001-01-01T00:00:00+00:00","Decimal":"0.000000000000001","Double":"2.23e-308","Guid":"33221100-5544-7766-9988-aabbccddeeff","Int16":"-32768","Int32":"-2147483648","Int64":"-9223372036854775808","Single":"1.18e-38"}""",
		"""{"Id":5,"Boolean":"true","Byte":"255","DateTime":"9999-12-31T23:59:59.999","DateTimeOffset":"9999-12-31T23:59:59.999+14:00","Decimal":"99999999999999999999.999999999999999","Double":"1.79e308","Guid":"ccddeeff-aabb-8899-7766-554433221100","Int16":"32767","Int32":"2147483647","Int64":"9223372036854775807","Single":"3.40e38"}""",
		"""{"Id":100,"Binary":"AQIDBA=="}""",
		"""{"Id":101,"Binary":"flc="}""",
	};

	public DbProviderFactory Factory => FlatFileProviderFactory.Instance;

	public string ConnectionString => $"path={_root}";

	public string CreateSelectSql(DbType dbType, ValueKind kind) => $"SELECT \"{dbType}\" FROM select_value WHERE Id = {(int) kind}";

	public string CreateSelectSql(byte[] value)
	{
		var key = BitConverter.ToString(value).Replace("-", string.Empty);
		return s_byteRows.TryGetValue(key, out var id)
			? $"SELECT \"Binary\" FROM select_value WHERE Id = {id}"
			: throw new NotSupportedException($"select_value has no row for the byte array 0x{key}; flatfile has no binary literal.");
	}

	public string SelectNoRows => "SELECT Id FROM select_value WHERE 1 = 0";

	public string DeleteNoRows => "DELETE FROM select_value WHERE 1 = 0";

	public IReadOnlyCollection<DbType> SupportedDbTypes { get; } = new ReadOnlyCollection<DbType>(new[]
	{
		DbType.Binary,
		DbType.Boolean,
		DbType.Byte,
		DbType.DateTime,
		DbType.DateTimeOffset,
		DbType.Decimal,
		DbType.Double,
		DbType.Guid,
		DbType.Int16,
		DbType.Int32,
		DbType.Int64,
		DbType.Single,
		DbType.String,
	});

	public Type NullValueExceptionType => typeof(InvalidCastException);

	public void Dispose()
	{
		if (Directory.Exists(_root))
		{
			Directory.Delete(_root, recursive: true);
		}
	}
}
