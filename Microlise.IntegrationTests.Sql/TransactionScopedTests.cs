using Microsoft.Extensions.Configuration;
using PetaPoco;
using System.Configuration;
using System.Transactions;

namespace Microlise.IntegrationTests.Sql;

public abstract class TransactionScopedTests
{
    private TransactionScope _transactionScope;

    protected static string ConnectionString
    {
        get
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));

            var root = builder.Build();
            return root.GetConnectionString("IntegrationTest") ?? throw new ConfigurationErrorsException("Expecting to find a connection string called 'IntegrationTest' in an appsettings.json file in the test project.");
        }
    }

    internal static IDatabase IntegrationTestDatabase
    {
        get
        {
            var db = DatabaseConfiguration.Build()
                .UsingConnectionString(ConnectionString)
                .UsingProvider<PetaPoco.Providers.SqlServerMsDataDatabaseProvider>()
                .Create();

            return db;
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
