using Dapper;
using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using NUnit.Framework;
using System.Text;

namespace Microlise.IntegrationTests.Sql.GovernanceTests
{
    [FilterFormat(@"\w+[.]\w+[.]\w+")]
    public class DoNotUseSelectStarTest : GovernanceTestBase
    {
        public override void RunTestExecution()
        {


            
            // DO NOT USE ? AS A SEPARATOR. EITHER FIND A BETTER WAY OF GETTING A DICTIONARY OUT, OR USE CHAR(7) (e.g.)

            var sql = @"
                SELECT
	                ISNULL(R.ROUTINE_SCHEMA + '.' + R.ROUTINE_NAME, '') + '?' + ISNULL(R.ROUTINE_DEFINITION, '')
                FROM
	                INFORMATION_SCHEMA.ROUTINES R
                WHERE
	                1 = 1";

            if (_testFilter?.FilterList.Count > 0)
            {
                sql += @"
                AND
                    OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name] NOT IN 
                        ( " + string.Join(", ", _testFilter.FilterList.Select(e => $"'{e.Key}'")) + " )";
            }

            var raw = IntegrationTestDatabase.Query<string>(sql);

            Dictionary<string, string> definitions = raw.ToDictionary(r => r.Split('?')[0], r => r.Split('?')[1]);

            StringBuilder failures = new();



            foreach (var definition in definitions)
            {
                if (definition.Value.Replace(" ", "").Replace("\t", "").Contains("SELECT*"))
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
