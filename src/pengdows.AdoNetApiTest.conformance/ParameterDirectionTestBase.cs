using System.Data;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

/// <summary>
/// Tests non-input parameter directions for providers that explicitly advertise
/// support through <see cref="IParameterDirectionFixture"/>.
/// </summary>
public abstract partial class ParameterDirectionTestBase<TFixture> : DbFactoryTestBase<TFixture>
	where TFixture : class, IParameterDirectionFixture
{
	protected ParameterDirectionTestBase(TFixture fixture)
		: base(fixture)
	{
	}

	private void ExecuteAndAssert(ParameterDirection direction)
	{
		if (!Fixture.SupportsParameterDirection(direction))
			throw SkipException.ForSkip($"The provider does not support {direction} parameters.");

		using var connection = CreateOpenConnection();
		using var command = connection.CreateCommand();
		var parameter = command.CreateParameter();
		parameter.ParameterName = "value";
		parameter.Direction = direction;
		Fixture.ConfigureParameter(parameter, direction);
		command.Parameters.Add(parameter);
		command.CommandText = Fixture.CreateParameterDirectionSql(MakeParameterName(connection, parameter), direction);

		command.ExecuteNonQuery();

		Fixture.AssertParameterValue(parameter, direction);
	}
}
