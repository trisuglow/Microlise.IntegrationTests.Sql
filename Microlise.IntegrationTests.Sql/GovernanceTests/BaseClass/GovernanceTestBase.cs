using Microlise.IntegrationTests.Sql.GovernanceTests.Filters;
using NUnit.Framework;

namespace Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;

public abstract class GovernanceTestBase : TransactionScopedTests
{
    public TestFilter? _testFilter;
    /*
    public GovernanceTestBase(ITestFilter)
    {

    }
    */
    public void RunTest()
    {
        if (this._testFilter is not null)
        {
            TestContext.WriteLine($"Test filtered: {this.GetType().Name} : {_testFilter.Justification}");
        }

        RunTestExecution();
    }

    public abstract void RunTestExecution();
}
