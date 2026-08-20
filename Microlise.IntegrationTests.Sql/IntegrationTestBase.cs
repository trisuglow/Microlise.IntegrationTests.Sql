using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microlise.IntegrationTests.Sql
{
    public abstract class IntegrationTestBase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", "NUnit1032:An IDisposable field/property should be Disposed in a TearDown method", Justification = "<Pending>")]
        protected IServiceProvider Services { get; private set; }
        protected DbConnection Connection { get; private set; }
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
    }
}
