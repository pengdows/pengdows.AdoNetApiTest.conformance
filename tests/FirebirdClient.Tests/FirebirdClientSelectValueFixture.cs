using pengdows.AdoNetApiTest.conformance.Databases;

namespace FirebirdClient.Tests;

/// <summary>Firebird 4 and later take the SQL-92 column types, <c>varbinary</c> and the <c>X'..'</c> literal as they are.</summary>
public sealed class FirebirdClientSelectValueFixture : SelectValueFixture<FirebirdClientDbFactoryFixture>
{
}
