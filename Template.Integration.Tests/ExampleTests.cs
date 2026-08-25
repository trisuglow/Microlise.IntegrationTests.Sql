using Dapper;
using Microlise.IntegrationTests.Sql;
using Microlise.IntegrationTests.Sql.Utilities;
using Template.Application;

namespace Template.Integration.Tests
{
    /// <summary>
    /// Use the library's TransactionScopedTests as a base class. This gives access to IntegrationTestDatabase for
    /// data manipulation. All data changes made within the lifecycle of an individual test will be rolled back on
    /// completion of the test, leaving the data in the database unchanged.
    /// </summary>
    public class ExampleTests : TransactionScopedTests
    {
        /// <summary>
        /// Use the library's AssertEnumMatchesDatabase assertion to validate a lookup table against an enumeration.
        /// </summary>
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