using System.Configuration;

using Microlise.IntegrationTests.Sql;
using Microlise.IntegrationTests.Sql.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Application;

namespace Template.Integration.Tests
{
    public class Tests
    {
        [Test]
        public void DoorStateEnumeration_MatchesDatabaseLookupTable()
        {
            EnumLookupValidation.AssertEnumMatchesDatabase<DoorState>("dbo.DoorState", "DoorStateID", "DoorStateDescription");
        }
    }
}