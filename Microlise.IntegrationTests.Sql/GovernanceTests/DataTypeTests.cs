using Dapper;
using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using NUnit.Framework;
using System.Text;

namespace Microlise.IntegrationTests.Sql.GovernanceTests;

public class DataTypeTests : GovernanceTestBase
{
    [Test]
    [FilterFormat(@"\w+[.]\w+[.]\w+")]
    public void UseDatetime2Test()
    {
        StringBuilder sql = new(@"
                SELECT
                    OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name]
                FROM
	                sys.all_columns C
                WHERE
                    OBJECT_SCHEMA_NAME(C.object_id) <> 'sys'
                AND 
                    OBJECT_SCHEMA_NAME(C.object_id) <> 'INFORMATION_SCHEMA'
                AND
                    system_type_id = 61 /* DATETIME */");

        if (TestHasFilters())
        {
            sql.AppendLine($@"
                AND
                    OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name] NOT IN {TestFilterList()}");
        }

        var dateTypeUsage = IntegrationTestDatabase.Query<string>(sql.ToString());

        Assert.That(
            dateTypeUsage.ToList(),
            Has.Count.EqualTo(0),
            $"Avoid using DATETIME. Prefer to use DATETIME2 instead on columns {string.Join(", ", dateTypeUsage.Select(c => $"'{c}'"))}.");
    }

    [Test]
    [FilterFormat(@"\w+[.]\w+[.]\w+")]
    public void UseDecimalRatherThanNumericTest()
    {
        StringBuilder sql = new( @"
                SELECT
                    OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name]
                FROM
	                sys.all_columns C
                WHERE
                    OBJECT_SCHEMA_NAME(C.object_id) <> 'sys'
                AND
                    system_type_id = 106 /* NUMERIC*/");

        if (TestHasFilters())
        {
            sql.AppendLine($@"
                AND
                    OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name] NOT IN {TestFilterList()}");
        }

        var numericTypeUsage = IntegrationTestDatabase.Query<string>(sql.ToString());

        Assert.That(
            numericTypeUsage.ToList(),
            Has.Count.EqualTo(0),
            $"Avoid using NUMERIC. Prefer to use DECIMAL for consistency (as it is functionally the same) on columns {string.Join(", ", numericTypeUsage.Select(c => $"'{c}'"))}.");
    }
}
