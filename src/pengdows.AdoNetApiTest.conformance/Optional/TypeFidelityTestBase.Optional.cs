using System;
using System.Data;
using System.Linq;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class TypeFidelityTestBase<TFixture>
	where TFixture : class, ITypeFidelityFixture
{
	/// <summary>
	/// A provider that lists <c>DataTypes</c> among its schema collections must describe its types truthfully: each type's
	/// <c>DataType</c> is the CLR type the provider returns for it, and each type the database supports is listed.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/common-schema-collections.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/common-schema-collections", Because = "applies only to a provider that lists DataTypes in MetaDataCollections")]
	public virtual void Advertised_data_types_match_the_catalog()
	{
		using var connection = CreateOpenConnection();
		DataTable collections;
		try
		{
			collections = connection.GetSchema();
		}
		catch (NotSupportedException)
		{
			ConformanceOutcome.NotImplemented("The provider does not support GetSchema.");
			return;
		}

		if (!collections.Rows.Cast<DataRow>().Any(row => string.Equals(row[0] as string, "DataTypes", StringComparison.OrdinalIgnoreCase)))
		{
			ConformanceOutcome.NotImplemented("The provider does not list DataTypes in MetaDataCollections.");
			return;
		}

		var advertised = connection.GetSchema("DataTypes");
		var names = advertised.Rows.Cast<DataRow>().Select(row => row["TypeName"] as string).Where(name => name is not null).ToList();

		foreach (var row in advertised.Rows.Cast<DataRow>())
		{
			var name = row["TypeName"] as string;
			var claimed = row["DataType"] as string;
			if (name is null)
			{
				continue;
			}

			var specs = Fixture.Types.Where(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();
			if (specs.Count == 0)
			{
				var uncovered = Fixture.UncoveredTypes.FirstOrDefault(u => name.IndexOf(u.Name, StringComparison.OrdinalIgnoreCase) >= 0);
				ConformanceOutcome.Finding(name, "(advertised)", "metadata", FidelityStatus.NotApplicable,
					uncovered is null ? "listed by the provider but not in the catalog" : "not covered: " + uncovered.Reason);
				continue;
			}

			foreach (var spec in specs)
			{
				ConformanceOutcome.Finding(spec.Name, spec.ColumnSql, "metadata",
					string.Equals(claimed, spec.ClrType.FullName, StringComparison.Ordinal) ? FidelityStatus.Exact : FidelityStatus.WrongType,
					string.Equals(claimed, spec.ClrType.FullName, StringComparison.Ordinal) ? null : $"DataTypes says {claimed ?? "NULL"} but the type returns {spec.ClrType.FullName}");
			}
		}

		foreach (var spec in Fixture.Types.Where(s => !names.Any(n => string.Equals(n, s.Name, StringComparison.OrdinalIgnoreCase))))
		{
			ConformanceOutcome.Finding(spec.Name, spec.ColumnSql, "metadata", FidelityStatus.Lossy, "the database supports this type but DataTypes does not list it");
		}
	}
}
