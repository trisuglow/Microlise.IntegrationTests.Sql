using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

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
        var testFilterKey = $"{GetType().Name}.{testName}";
        MethodBase? method = GetType().GetMethod(testName);

        FilterFormatAttribute attr = (FilterFormatAttribute)method!.GetCustomAttributes(typeof(FilterFormatAttribute), true)[0];
        string filterFormat = attr.Format;
        var regex = new Regex(filterFormat);

        var testFilters = IntegrationTestConfiguration.TestFilters[testFilterKey];

        testFilters.ForEach(f =>
        {
            if (!regex.Match(f).Success)
            {
                throw new Exception($"Bad filter for {testFilterKey} - '{f}'. Should match regex [{filterFormat}]. Check appsettings.json file.");
            }
        });

        return "( " + string.Join(", ", testFilters.Select(e => $"'{e}'")) + " )";
    }

    protected static string FormattedFailureMessage(string message, IEnumerable<string> failureCases)
    {
        return $"{message}{Environment.NewLine}{string.Join($",{Environment.NewLine}", failureCases.Select(c => $"\t'{c}'"))}.";
    }
}
