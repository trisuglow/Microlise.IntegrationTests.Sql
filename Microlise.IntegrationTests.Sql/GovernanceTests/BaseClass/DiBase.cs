using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Data;
using System.Data.Common;

namespace Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;

[TestFixture]
public abstract class DiBase //: TransactionScopedTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", "NUnit1032:An IDisposable field/property should be Disposed in a TearDown method", Justification = "<Pending>")]
    public IServiceProvider Services { get; set; }
    public DbConnection Connection { get; set; }






    [OneTimeSetUp]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();
        Connection = Services.GetRequiredService<DbConnection>();
        Connection.Open();
    }
    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        Connection?.Dispose();
    }


    protected abstract void ConfigureServices(IServiceCollection services);

    public static IDbConnection IntegrationTestDatabase
    {
        get
        {
            Console.WriteLine("Getting IntegrationTestDatabase");


            try
            {

                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsetting.json").Build();

                Console.WriteLine(config.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }




            //var connection = Services.GetRequiredService<DbConnection>();
            //return connection;

            //return new SqlConnection("Server=localhost;Database=IntegrationTestLibrary;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;");
            return new SqlConnection("Server=localhost;Database=DeviceConfiguration.PreferenceSets;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}
