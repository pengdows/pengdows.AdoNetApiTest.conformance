using System.Collections.Generic;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Type fidelity over a hand-written catalog (<see cref="ITypeFidelityFixture.Types"/>), which can encode a database's
/// own rules: a <c>char(10)</c> comes back padded and a SQL Server <c>datetime</c> keeps 3.33 ms steps.
/// </summary>
public abstract partial class TypeFidelityTestBase<TFixture> : FidelityTestBase<TFixture>
	where TFixture : class, ITypeFidelityFixture
{
	protected TypeFidelityTestBase(TFixture fixture)
		: base(fixture)
	{
		Fixture = fixture;
	}

	protected new TFixture Fixture { get; }

	protected override IReadOnlyList<TypeSpec> Specs => Fixture.Types;

	protected override string CreateTableSql(string table, string columnSql) => Fixture.CreateTableSql(table, columnSql);

	protected override string DropTableSql(string table) => Fixture.DropTableSql(table);

	protected override string ParameterMarker(string name) => Fixture.ParameterMarker(name);
}
