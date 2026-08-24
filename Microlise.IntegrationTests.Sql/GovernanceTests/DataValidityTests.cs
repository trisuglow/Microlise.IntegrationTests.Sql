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
            $"Most tables should have a primary key. PK missing on tables {string.Join(", ", tablesWithNoPrimaryKey.Select(c => $"'{c}'"))}.");
    }

}
