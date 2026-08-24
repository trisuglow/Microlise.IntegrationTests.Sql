using Dapper;
using Microlise.IntegrationTests.Sql;
using Microlise.IntegrationTests.Sql.Utilities;
using Template.Application;

namespace Template.Integration.Tests
{
    public class Tests : TransactionScopedTests
    {
        [Test]
        public void DoorStateEnumeration_MatchesDatabaseLookupTable()
        {
            EnumLookupValidation.AssertEnumMatchesDatabase<DoorState>("dbo.DoorState", "DoorStateID", "DoorStateDescription");
        }

        [Test]
        public void CanAddRowToEmptyTable()
        {
            IntegrationTestDatabase.Execute(@"
                INSERT
                    dbo.BreakTheRules
                        ( StartDate, FinishDate, RainfallInCentimetres )
                VALUES
                    ( '2022-02-02 02:02:02', '2022-02-02 14:14:14', 123.45 )");

            var rows = IntegrationTestDatabase.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.BreakTheRules");

            Assert.That(rows, Is.EqualTo(1));
        }
    }
}