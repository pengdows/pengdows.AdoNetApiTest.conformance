using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace pengdows.AdoNetApiTest.conformance;

public abstract partial class GetValueConversionTestBase<TFixture>
	where TFixture : class, ISelectValueFixture
{
}
