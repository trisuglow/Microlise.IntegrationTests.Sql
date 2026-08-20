using Dapper;
using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using NUnit.Framework;

namespace Microlise.IntegrationTests.Sql.GovernanceTests;

public class NamingTests : GovernanceTestBase
{
    [Test]
    public void DatabaseName()
    {
        var dbName = IntegrationTestDatabase.QueryFirstOrDefault<string>("SELECT DB_NAME()") ?? "";

        if (_testFilter is not null && _testFilter.FilterList.ContainsKey(dbName))
        {
            return;
        }

        Assert.That(
            dbName,
            Does.Not.Contain("."),
            "The database name must not contain a full stop. See https://microliseuk.sharepoint.com/:w:/r/sites/ControlledDocuments/Shared%20Documents/EntArch-03%20(Issue%203.0)%20Microlise%20Technical%20Naming%20Standards.docx?d=wf90fc3b6ec4a4a4496b1f19bfef64312&csf=1&web=1&e=x497MW for more information on databse naming.");
    }
}
