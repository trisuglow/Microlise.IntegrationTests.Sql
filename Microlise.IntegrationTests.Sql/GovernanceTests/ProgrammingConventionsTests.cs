using Dapper;
using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using NUnit.Framework;
using System.Text;

namespace Microlise.IntegrationTests.Sql.GovernanceTests
{
    public class ProgrammingConventionsTests : GovernanceTestBase
    {


        ///  Add a check that stored procedures using TRANSACTION / ROLLBACK also have TRY CATCH



        [Test]
        [FilterFormat(@"\w+[.]\w+")]
        public void DoNotUseSelectStarTest()
        {
            var sqlString = CreateTestSqlString(@"
                SELECT
                    ObjectName,
                    ObjectDefinition
                FROM
                (
                    SELECT
	                    ObjectName = ISNULL(R.ROUTINE_SCHEMA + '.' + R.ROUTINE_NAME, ''),
                        ObjectDefinition = ISNULL(R.ROUTINE_DEFINITION, '')
                    FROM
	                    INFORMATION_SCHEMA.ROUTINES R

                    UNION ALL

                    SELECT
	                    V.TABLE_SCHEMA + '.' + V.TABLE_NAME,	
                        V.VIEW_DEFINITION
                    FROM
	                    INFORMATION_SCHEMA.VIEWS V
                ) D");

            var definitions = IntegrationTestDatabase.Query<(string ObjectName, string ObjectDefinition)>(sqlString.ToString())
                .ToDictionary(o => o.ObjectName, o => o.ObjectDefinition);

            List<string> failures = [];

            foreach (var definition in definitions)
            {
                if (definition.Value.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Contains("SELECT*"))
                {
                    failures.Add(definition.Key);
                }
            }

            Assert.That(
                failures,
                Has.Count.EqualTo(0),
                FormattedFailureMessage(
                    "Avoid using SELECT *. Select columns specifically in",
                    failures));
        }

        [Test]
        [FilterFormat(@"\w+[.]\w+")]
        public void DoNotUseTop1000Test()
        {
            var sqlString = CreateTestSqlString(
                @"
                SELECT
                    D.ObjectName,
                    D.ObjectDefinition
                FROM
                (
                    SELECT
	                    ObjectName = ISNULL(R.ROUTINE_SCHEMA + '.' + R.ROUTINE_NAME, ''),
                        ObjectDefinition = ISNULL(R.ROUTINE_DEFINITION, '')
                    FROM
	                    INFORMATION_SCHEMA.ROUTINES R

                    UNION ALL

                    SELECT
	                    V.TABLE_SCHEMA + '.' + V.TABLE_NAME,	
                        V.VIEW_DEFINITION
                    FROM
	                    INFORMATION_SCHEMA.VIEWS V
                ) D");

            var definitions = IntegrationTestDatabase.Query<(string ObjectName, string ObjectDefinition)>(sqlString)
                .ToDictionary(o => o.ObjectName, o => o.ObjectDefinition);

            List<string> failures = [];

            foreach (var definition in definitions)
            {
                if (definition.Value.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("(", "").Contains("TOP1000"))
                {
                    failures.Add(definition.Key);
                }
            }

            Assert.That(
                failures,
                Has.Count.EqualTo(0),
                FormattedFailureMessage(
                    "Avoid using TOP 1000. This is likely a cut-and-paste error. If this is by design, then exclude this test for",
                    failures));
        }
    }
}
