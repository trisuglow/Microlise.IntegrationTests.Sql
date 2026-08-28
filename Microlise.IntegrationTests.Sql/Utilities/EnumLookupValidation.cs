using Dapper;
using NUnit.Framework;

namespace Microlise.IntegrationTests.Sql.Utilities
{
    public class EnumLookupValidation : TransactionScopedTests
    {
        /// <summary>
        /// Assert that there is a 1:1 match between a C# enumeration and the records in a lookup table.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration, e.g. "StateValues".</typeparam>
        /// <param name="databaseTable">The lookup table in the database, e.g. "dbo.StateValues".</param>
        /// <param name="enumIdColumn">The column name of the numeric ID column in the lookup table.</param>
        /// <param name="enumNameColumn">The column name of the description/name column in the lookup table.</param>
        public static void AssertEnumMatchesDatabase<T>(string databaseTable, string enumIdColumn, string enumNameColumn) where T : Enum
        {
            var databaseValues = IntegrationTestDatabase.Query<EnumFromDatabase>($@"
                SELECT
                    EnumId = {enumIdColumn},
                    EnumName = {enumNameColumn}
                FROM
                    {databaseTable}");
            var dv = databaseValues.Select(v => new EnumFromDatabase()
            {
                EnumId = v.EnumId,
                EnumName = v.EnumName.Replace(" ", "").Replace("-", "").Replace("_", "").Replace("\\", "").Replace("/", "") });

            var enumValues = (IEnumerable<T>)Enum.GetValues(typeof(T));
            var ev = enumValues.Select(v => new EnumFromDatabase() { EnumId = Convert.ToInt32(v), EnumName = v.ToString() });

            Assert.Multiple(() =>
            {
                Assert.That(databaseValues, Is.Not.Null, "Expecting records to be present in database.");
                Assert.That(enumValues, Is.Not.Null, "Expecting values to be present in enumeration.");
                Assert.That(
                    ev, 
                    Is.EquivalentTo(dv).Using<EnumFromDatabase>((x, y) => { return x.EnumId == y.EnumId && x.EnumName == y.EnumName; }),
                    "Expecting Int32 enumeration values to match exactly, one-to-one, with database records. Also expecting enum value names to match database records, ignoring underscores, hyphens, and slashes.");
            });
        }

        private class EnumFromDatabase
        {
            public int EnumId { get; set; }
            public string EnumName { get; set; } = "";
        }
    }
}

