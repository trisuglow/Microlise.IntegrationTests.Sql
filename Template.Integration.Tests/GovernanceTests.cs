using Microlise.IntegrationTests.Sql;
using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace Template.Integration.Tests
{
    public class GovernanceTests : DiBase
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<DbConnection>(_ =>
                new SqlConnection("Server=localhost;Database=IntegrationTestLibrary;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;"));
        }
    }
}
