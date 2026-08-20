using Microlise.IntegrationTests.Sql.GovernanceTests.Filters;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Data.Common;

namespace Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;


public abstract class GovernanceTestBase : TransactionScopedTests
{


    public TestFilter? _testFilter;


    //public GovernanceTestBase(ITestLibraryConfiguration testLibraryConfiguration)
    //    public GovernanceTestBase()
    //    {
    //        Console.WriteLine("One time setup.");


    //        //var configuration = DependencyInjector.GetServiceProvider();

    ////var testLibraryConfiguration=        configuration.GetService(typeof(ITestLibraryConfiguration));

    //    }



    //public void RunTest()
    //{
    //    if (this._testFilter is not null)
    //    {
    //        TestContext.WriteLine($"Test filtered: {this.GetType().Name} : {_testFilter.Justification}");
    //    }

    //    RunTestExecution();
    //}

    //public abstract void RunTestExecution();

}
