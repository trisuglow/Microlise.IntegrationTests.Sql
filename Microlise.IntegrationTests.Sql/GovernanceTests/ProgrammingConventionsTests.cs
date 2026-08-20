//using Dapper;
//using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.DependencyInjection;
//using NUnit.Framework;
//using System.Data.Common;
//using System.Text;

//namespace Microlise.IntegrationTests.Sql.GovernanceTests
//{
//    public abstract class ProgrammingConventionsTests : GovernanceTestBase
//    {
//        [Test]
//        [FilterFormat(@"\w+[.]\w+[.]\w+")]
//        public void DoNotUseSelectStarTest()
//        {

//            var sql = @"
//                SELECT
//	                [Name] = ISNULL(R.ROUTINE_SCHEMA + '.' + R.ROUTINE_NAME, ''),
//                    Definition = ISNULL(R.ROUTINE_DEFINITION, '')
//                FROM
//	                INFORMATION_SCHEMA.ROUTINES R
//                WHERE
//	                1 = 1";

//            if (_testFilter?.FilterList.Count > 0)
//            {
//                sql += @"
//                AND
//                    OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name] NOT IN 
//                        ( " + string.Join(", ", _testFilter.FilterList.Select(e => $"'{e.Key}'")) + " )";
//            }

//            var definitions = IntegrationTestDatabase.Query<(string Name, string Definition)>(sql).ToDictionary(o => o.Name, o => o.Definition);


//            StringBuilder failures = new();



//            foreach (var definition in definitions)
//            {
//                if (definition.Value.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Contains("SELECT*"))
//                {
//                    failures.AppendLine(definition.Key);
//                }
//            }


//            Assert.That(
//                failures.Length,
//                Is.EqualTo(0),
//                $"Avoid using SELECT *. Select columns specifically in {failures?.ToString()}");
//        }


//    }
//}
