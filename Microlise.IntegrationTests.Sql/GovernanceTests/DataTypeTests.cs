using Dapper;
using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Microlise.IntegrationTests.Sql.GovernanceTests;

public class DataTypeTests : GovernanceTestBase
{
    [Test]
    [FilterFormat(@"\w+[.]\w+[.]\w+")]
    public void UseDatetime2Test()
    {
        var sql = @"
                SELECT
                    OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name]
                FROM
	                sys.all_columns C
                WHERE
                    OBJECT_SCHEMA_NAME(C.object_id) <> 'sys'
                AND 
                    OBJECT_SCHEMA_NAME(C.object_id) <> 'INFORMATION_SCHEMA'
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
            dateTypeUsage.ToList(),
            Has.Count.EqualTo(0),
            $"Avoid using DATETIME. Prefer to use DATETIME2 instead on columns {string.Join(", ", dateTypeUsage.Select(c => $"'{c}'"))}.");
    }

    [Test]
    [FilterFormat(@"\w+[.]\w+[.]\w+")]
    public void UseDecimalRatherThanNumeric()
    {
        var sql = @"
                SELECT
                    OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name]
                FROM
	                sys.all_columns C
                WHERE
                    OBJECT_SCHEMA_NAME(C.object_id) <> 'sys'
                AND
                    system_type_id = 106 /* NUMERIC*/";

        if (_testFilter?.FilterList.Count > 0)
        {
            sql += @"
                AND
                    OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + [name] NOT IN 
                        ( " + string.Join(", ", _testFilter.FilterList.Select(e => $"'{e.Key}'")) + " )";
        }

        var numericTypeUsage = IntegrationTestDatabase.Query<string>(sql);

        Assert.That(
            numericTypeUsage.ToList(),
            Has.Count.EqualTo(0),
            $"Avoid using NUMERIC. Prefer to use DECIMAL for consistency (as it is functionally the same) on columns {string.Join(", ", numericTypeUsage.Select(c => $"'{c}'"))}.");
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        throw new NotImplementedException();
    }
}
