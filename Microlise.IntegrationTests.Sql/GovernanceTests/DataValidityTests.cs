//using Dapper;
//using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.DependencyInjection;
//using NUnit.Framework;
//using System.Data.Common;

//namespace Microlise.IntegrationTests.Sql.GovernanceTests;


//public abstract class DataValidityTests : GovernanceTestBase
//{

//    [Test]
//    [FilterFormat(@"\w+[.]\w+")]
//    public void PrimaryKeyOnAllTablesTest()
//    {
//        var sql = @"
//                SELECT
//                    OBJECT_SCHEMA_NAME(O.object_id) + '.' + OBJECT_NAME(O.object_id)
//                FROM 
//	                sys.objects O
//                LEFT JOIN
//	                sys.indexes I ON O.object_id = I.object_id AND I.is_primary_key = 1
//                WHERE
//	                O.type_desc = 'USER_TABLE'
//                AND
//	                O.[name] <> '__RefactorLog'
//                AND
//                    I.[name] IS NULL";

//        if (_testFilter?.FilterList.Count > 0)
//        {
//            sql += @"
//                AND
//                    OBJECT_SCHEMA_NAME(O.object_id) + '.' + OBJECT_NAME(O.object_id) NOT IN
//                        ( " + string.Join(", ", _testFilter.FilterList.Select(e => $"'{e.Key}'")) + " )";
//        }

//        var tablesWithNoPrimaryKey = IntegrationTestDatabase.Query<string>(sql);

//        Assert.That(
//            tablesWithNoPrimaryKey.ToList(),
//            Has.Count.EqualTo(0),
//            $"Most tables should have a primary key. PK missing on tables {string.Join(", ", tablesWithNoPrimaryKey.Select(c => $"'{c}'"))}.");
//    }

//}
