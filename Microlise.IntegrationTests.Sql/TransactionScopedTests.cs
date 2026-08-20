using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Transactions;

namespace Microlise.IntegrationTests.Sql;

public class TransactionScopedTests
{



    private TransactionScope _transactionScope;


    protected static IIntegrationTestLibraryConfiguration IntegrationTestConfiguration
    {
        get
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));


            var root = builder.Build();

            var config = root.GetSection("IntegrationTestLibraryConfiguration").Get<IntegrationTestLibraryConfiguration>();





            //IntegrationTestConfiguration config = (IntegrationTestLibraryConfiguration)(root.GetSection("Microlise.IntegrationTests.Sql") ??
            //        throw new Exception("Expecting to find a section called 'Microlise.IntegrationTests.Sql' in an appsettings.json file in the test project."));

            return config;
        }
    }

    //protected static string ConnectionString
    //{
    //    get
    //    {
    //        var builder = new ConfigurationBuilder();
    //        builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));

    //        var root = builder.Build();

    //        return root.GetConnectionString("IntegrationTest") ??
    //            throw new Exception("Expecting to find a connection string called 'IntegrationTest' in an appsettings.json file in the test project.");
    //    }
    //}

    internal static IDbConnection IntegrationTestDatabase
    {
        get
        {
            //try
            //{
            //    Console.WriteLine($"I got the connection string! - {IntegrationTestConfiguration.ConnectionString}.");
            //}
            //catch (Exception ex)
            //{
            //    Console.Error.WriteLine(ex.ToString());
            //}


            return new SqlConnection(IntegrationTestConfiguration.ConnectionString);
        }
    }

    //[SetUp]
    //public void Setup()
    //{
    //    var options = new TransactionOptions()
    //    {
    //        IsolationLevel = IsolationLevel.ReadCommitted,
    //        Timeout = new TimeSpan(0, 2, 0),
    //    };

    //    _transactionScope = new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled);
    //    try
    //    {
    //        TestSetup();
    //    }
    //    catch
    //    {
    //        _transactionScope.Dispose();
    //        throw;
    //    }
    //}

    //[TearDown]
    //public void TearDown()
    //{
    //    try
    //    {
    //        TestTearDown();
    //    }
    //    finally
    //    {
    //        _transactionScope.Dispose();
    //    }
    //}
    //protected virtual void TestSetup()
    //{
    //}

    //protected virtual void TestTearDown()
    //{
    //}
}
