using System;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class TypeFidelityTestBase<TFixture>
	where TFixture : class, ITypeFidelityFixture
{
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getvalue", Because = "value fidelity per type is an interoperability expectation; the contract does not state it")]
	public virtual void Types_are_read_back_exactly_from_literals() => Run(Paths.Read);

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.value", Because = "value fidelity per type is an interoperability expectation; the contract does not state it")]
	public virtual void Types_are_written_exactly_by_parameters() => Run(Paths.Write);

	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbdatareader.getfieldvalue", Because = "value fidelity per type is an interoperability expectation; the contract does not state it")]
	public virtual void Types_round_trip_exactly_through_parameters() => Run(Paths.RoundTrip);
}
