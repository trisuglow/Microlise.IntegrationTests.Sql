using Dapper;
using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using NUnit.Framework;
using System.Text;

namespace Microlise.IntegrationTests.Sql.GovernanceTests
{
    public class ProgrammingConventionsTests : GovernanceTestBase
    {
        [Test]
        [FilterFormat(@"\w+[.]\w+[.]\w+")]
        public void DoNotUseSelectStarTest()
        {
            StringBuilder sql = new(@"
                SELECT
	                [Name] = ISNULL(R.ROUTINE_SCHEMA + '.' + R.ROUTINE_NAME, ''),
                    Definition = ISNULL(R.ROUTINE_DEFINITION, '')
                FROM
	                INFORMATION_SCHEMA.ROUTINES R");

            if ( TestHasFilters())
            {
                sql.AppendLine( $@"
                WHERE
                    OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name] NOT IN {TestFilterList()}");
            }

            var definitions = IntegrationTestDatabase.Query<(string Name, string Definition)>(sql.ToString()).ToDictionary(o => o.Name, o => o.Definition);

            StringBuilder failures = new();

            foreach (var definition in definitions)
            {
                if (definition.Value.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Contains("SELECT*"))
                {
                    failures.AppendLine(definition.Key);
                }
            }

            Assert.That(
                failures.Length,
                Is.EqualTo(0),
                $"Avoid using SELECT *. Select columns specifically in {failures?.ToString()}");
        }
    }
}
