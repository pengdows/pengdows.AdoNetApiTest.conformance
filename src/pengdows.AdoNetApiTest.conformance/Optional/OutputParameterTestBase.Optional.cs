using System.Data;
using System.Data.Common;
using Xunit;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class OutputParameterTestBase<TFixture>
	where TFixture : class, IOutputParameterFixture
{
	/// <summary>
	/// An output parameter must be populated after successful command execution.
	/// The fixture supplies the SQL and native value details; this shared test
	/// enforces the common DbParameter direction and execution lifecycle.
	/// See https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.direction
	/// and https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand.executenonquery.
	/// </summary>
	[OptionalFact(Contract = "https://learn.microsoft.com/dotnet/api/system.data.common.dbparameter.direction")]
	public virtual void ExecuteNonQuery_populates_output_parameter()
	{
		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		var parameter = command.CreateParameter();
		parameter.ParameterName = "output";
		parameter.Direction = ParameterDirection.Output;
		Fixture.ConfigureOutputParameter(parameter);
		command.Parameters.Add(parameter);
		command.CommandText = Fixture.CreateOutputParameterSql(MakeParameterName(connection, parameter));

		command.ExecuteNonQuery();

		Fixture.AssertOutputParameterValue(parameter);
	}
}
