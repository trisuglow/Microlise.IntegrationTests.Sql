using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microlise.IntegrationTests.Sql
{
    public interface ITestLibraryConfiguration
    {
        public string ConnectionString { get; set; }
    }
}
