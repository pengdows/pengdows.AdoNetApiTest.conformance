using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class DataTypesFidelityTestBase<TFixture>
	where TFixture : class, IDbFactoryFixture
{
	/// <summary>
	/// Every type in the provider's DataTypes collection, written with a literal where one is portable and read back.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/common-schema-collections#datatypes.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/common-schema-collections", Because = "value fidelity per type is an interoperability expectation; the contract does not state it")]
	public virtual void DataTypes_are_read_back_exactly_from_literals() => RunGenerated(Paths.Read, reportUncovered: true);

	/// <summary>
	/// Every type in the provider's DataTypes collection, written with a parameter and compared on the database side with
	/// the literal.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/common-schema-collections#datatypes.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/common-schema-collections", Because = "value fidelity per type is an interoperability expectation; the contract does not state it")]
	public virtual void DataTypes_are_written_exactly_by_parameters() => RunGenerated(Paths.Write, reportUncovered: false);

	/// <summary>
	/// Every type in the provider's DataTypes collection, written with a parameter and read back as the DataType the
	/// collection advertises.
	/// See https://learn.microsoft.com/dotnet/framework/data/adonet/common-schema-collections#datatypes.
	/// </summary>
	[CommonFact(Contract = "https://learn.microsoft.com/dotnet/framework/data/adonet/common-schema-collections", Because = "value fidelity per type is an interoperability expectation; the contract does not state it")]
	public virtual void DataTypes_round_trip_exactly_through_parameters() => RunGenerated(Paths.RoundTrip, reportUncovered: false);
}
