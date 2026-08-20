using Microlise.IntegrationTests.Sql.GovernanceTests.Filters;

namespace Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;


public abstract class GovernanceTestBase : TransactionScopedTests
{
    public TestFilter? _testFilter;
}
