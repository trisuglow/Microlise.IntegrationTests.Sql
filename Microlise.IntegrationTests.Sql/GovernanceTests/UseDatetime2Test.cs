using Dapper;
using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using NUnit.Framework;

namespace Microlise.IntegrationTests.Sql.GovernanceTests;

[FilterFormat(@"\w+[.]\w+[.]\w+")]
public class UseDatetime2Test : GovernanceTestBase
{
    public override void RunTestExecution()
    {
        var sql = @"
                SELECT
                    OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name]
                FROM
	                sys.all_columns C
                WHERE
                    OBJECT_SCHEMA_NAME(C.object_id) <> 'sys'
                AND
                    system_type_id = 61 /* DATETIME */";

        if (_testFilter?.FilterList.Count > 0)
        {
            sql += @"
                AND
                    OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name] NOT IN 
                        ( " + string.Join(", ", _testFilter.FilterList.Select(e => $"'{e.Key}'")) + " )";
        }

        var dateTypeUsage = IntegrationTestDatabase.Query<string>(sql);

        Assert.That(
            dateTypeUsage,
            Has.Count.EqualTo(0),
            $"Avoid using DATETIME. Prefer to use DATETIME2 instead on columns {string.Join(", ", dateTypeUsage.Select(c => $"'{c}'"))}.");
    }
}
