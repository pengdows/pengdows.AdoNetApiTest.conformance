using System.Data;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class ParameterDirectionTestBase<TFixture>
	where TFixture : class, IParameterDirectionFixture
{
	/// <summary>
	/// An InputOutput parameter must be populated after successful execution when
	/// the provider advertises support for that direction.
	/// See https://learn.microsoft.com/dotnet/api/system.data.parameterdirection
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.direction.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.direction")]
	public virtual void ExecuteNonQuery_populates_input_output_parameter()
		=> ExecuteAndAssert(ParameterDirection.InputOutput);

	/// <summary>
	/// A ReturnValue parameter must be populated after successful execution when
	/// the provider advertises support for that direction.
	/// See https://learn.microsoft.com/dotnet/api/system.data.parameterdirection
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.direction.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.direction")]
	public virtual void ExecuteNonQuery_populates_return_value_parameter()
		=> ExecuteAndAssert(ParameterDirection.ReturnValue);
}
