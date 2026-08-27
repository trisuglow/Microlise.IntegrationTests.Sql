using Dapper;
using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using NUnit.Framework;
using System.Text;

namespace Microlise.IntegrationTests.Sql.GovernanceTests;

public class NamingTests : GovernanceTestBase
{
    [Test]
    [FilterFormat(@"\w+")]
    public void DatabaseNameTest()
    {
        var dbName = IntegrationTestDatabase.QueryFirstOrDefault<string>("SELECT DB_NAME()") ?? "";

        if (TestHasFilters() && TestFilterList().Contains($"'{dbName}'"))
        {
            return;
        }

        Assert.That(
            dbName,
            Does.Not.Contain("."),
            "The database name must not contain a full stop. See https://microliseuk.sharepoint.com/:w:/r/sites/ControlledDocuments/Shared%20Documents/EntArch-03%20(Issue%203.0)%20Microlise%20Technical%20Naming%20Standards.docx?d=wf90fc3b6ec4a4a4496b1f19bfef64312&csf=1&web=1&e=x497MW for more information on database naming.");
    }

    [Test]
    [FilterFormat(@"\w+[.]\w+[.]\w+")]
    public void DateColumnsSuffixedWithLocalOrUtcTest()
    {
        StringBuilder sql = new(@"
            SELECT
                OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + C.[name]
            FROM
	            sys.all_columns C
            JOIN
                sys.systypes S ON C.user_type_id = S.xusertype
            WHERE
                OBJECT_SCHEMA_NAME(C.object_id) <> 'sys'
            AND 
                OBJECT_SCHEMA_NAME(C.object_id) <> 'INFORMATION_SCHEMA'
            AND
                S.[name] IN ('date', 'datetime', 'datetime2', 'datetimeoffset', 'smalldatetime' )
            AND
                C.[name] NOT LIKE '%utc'
            AND
                C.[name] NOT LIKE '%local'");

        if (TestHasFilters())
        {
            sql.AppendLine($@"
                AND
                    OBJECT_SCHEMA_NAME(C.object_id) + '.' + OBJECT_NAME(C.object_id) + '.' + C.[name] NOT IN {TestFilterList()}");
        }

        var badlyNamedDateTypeColumns = IntegrationTestDatabase.Query<string>(sql.ToString());

        Assert.That(
            badlyNamedDateTypeColumns.ToList(),
            Has.Count.EqualTo(0),
            FormattedFailureMessage(
                "Date columns should be suffixed with 'UTC' or 'Local'. This is not the case on:",
                badlyNamedDateTypeColumns));
    }
}
