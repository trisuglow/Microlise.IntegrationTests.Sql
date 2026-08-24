using System.Runtime.CompilerServices;

namespace Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;

public abstract class GovernanceTestBase : TransactionScopedTests
{
    public static Dictionary<string, List<string>> TestFilters
    {
        get
        {
            return IntegrationTestConfiguration.TestFilters;
        }
    }

    protected bool TestHasFilters([CallerMemberName] string testName = "")
    {
        return IntegrationTestConfiguration.TestFilters.ContainsKey($"{GetType().Name}.{testName}");
    }

    protected string TestFilterList([CallerMemberName] string testName = "")
    {
        return "( " + string.Join(", ", IntegrationTestConfiguration.TestFilters[$"{GetType().Name}.{testName}"].Select(e => $"'{e}'")) + " )";
    }
}
