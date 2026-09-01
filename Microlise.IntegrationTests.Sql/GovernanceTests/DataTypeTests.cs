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
        var sqlString = CreateTestSqlString(@"
                SELECT
                    ObjectName = OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name]
                FROM
	                sys.all_columns C
                WHERE
                    OBJECT_SCHEMA_NAME(C.object_id) <> 'sys'
                AND 
                    OBJECT_SCHEMA_NAME(C.object_id) <> 'INFORMATION_SCHEMA'
                AND
                    system_type_id = 61 /* DATETIME */");

        var dateTypeUsage = IntegrationTestDatabase.Query<string>(sqlString);

        Assert.That(
            dateTypeUsage.ToList(),
            Has.Count.EqualTo(0),
            FormattedFailureMessage(
                "Avoid using DATETIME. Prefer to use DATETIME2 instead on columns",
                dateTypeUsage));
    }

    [Test]
    [FilterFormat(@"\w+[.]\w+[.]\w+")]
    public void UseDecimalRatherThanNumericTest()
    {
        var sqlString = CreateTestSqlString(@"
            SELECT
                ObjectName = OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name]
            FROM
	            sys.all_columns C
            WHERE
                OBJECT_SCHEMA_NAME(C.object_id) <> 'sys'
            AND
                system_type_id = 106 /* NUMERIC*/");

        var numericTypeUsage = IntegrationTestDatabase.Query<string>(sqlString);

        Assert.That(
            numericTypeUsage.ToList(),
            Has.Count.EqualTo(0),
            FormattedFailureMessage(
                "Avoid using NUMERIC. Prefer to use DECIMAL for consistency (as it is functionally the same) on columns",
                numericTypeUsage));
    }
}
