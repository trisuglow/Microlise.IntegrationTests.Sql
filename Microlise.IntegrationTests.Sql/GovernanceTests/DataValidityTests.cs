using Dapper;
using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Text;

namespace Microlise.IntegrationTests.Sql.GovernanceTests;

public class DataValidityTests : GovernanceTestBase
{
    [Test]
    [FilterFormat(@"\w+[.]\w+")]
    public void SelectFromAllViewsTest()
    {
        var executionCount = 0;
        var views = IntegrationTestDatabase.Query<string>(@"
            SELECT
	            S.[name] + '.' + V.[name]
            FROM
	            sys.views V
            JOIN
	            sys.schemas S ON V.schema_id = S.schema_id
            WHERE
	            S.[name] <> 'tSQLt'");

        foreach (var view in views)
        {
            Console.WriteLine($"Testing: {view}");

            IntegrationTestDatabase.Execute($"SELECT * FROM {view}");

            executionCount += 1;
        }

        Assert.That(executionCount, Is.EqualTo(views.Count()));
    }

    [Test]
    [FilterFormat(@"\w+[.]\w+")]
    public void PrimaryKeyOnAllTablesTest()
    {
        StringBuilder sql = new(@"
                SELECT
                    OBJECT_SCHEMA_NAME(O.object_id) + '.' + OBJECT_NAME(O.object_id)
                FROM 
	                sys.objects O
                LEFT JOIN
	                sys.indexes I ON O.object_id = I.object_id AND I.is_primary_key = 1
                WHERE
	                O.type_desc = 'USER_TABLE'
                AND
	                O.[name] <> '__RefactorLog'
                AND
                    I.[name] IS NULL");

        if (TestHasFilters())
        {
            sql.AppendLine($@"
                AND
                    OBJECT_SCHEMA_NAME(O.object_id) + '.' + OBJECT_NAME(O.object_id) NOT IN {TestFilterList()}");
        }

        var tablesWithNoPrimaryKey = IntegrationTestDatabase.Query<string>(sql.ToString());

        Assert.That(
            tablesWithNoPrimaryKey.ToList(),
            Has.Count.EqualTo(0),
            FormattedFailureMessage(
                "Most tables should have a primary key. PK missing on tables",
                tablesWithNoPrimaryKey));
    }
}
