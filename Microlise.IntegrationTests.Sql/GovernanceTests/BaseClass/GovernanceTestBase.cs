using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
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

    /// <summary>
    /// Creates a SQL query string, automatically applying the Test Filters.
    /// </summary>
    /// <param name="sqlQuery">Query must return the object name as ObjectName.</param>
    /// <param name="testName">Do not specify this - it is filled in by middleware.</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    protected string CreateTestSqlString(string sqlQuery, [CallerMemberName] string testName = "")
    {
        var testFilterKey = $"{GetType().Name}.{testName}";
        MethodBase? method = GetType().GetMethod(testName);

        var sql = new StringBuilder("SELECT * FROM");
        sql.AppendLine("(");
        sql.AppendLine(sqlQuery);
        sql.AppendLine(") WRAPPER");

        if (IntegrationTestConfiguration.TestFilters.ContainsKey($"{GetType().Name}.{testName}"))
        {
            sql.AppendLine("WHERE ObjectName NOT IN");

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

            sql.AppendLine("( " + string.Join(", ", testFilters.Select(e => $"'{e}'")) + " )");
        }

        return sql.ToString();
    }

    //protected bool TestHasFilters([CallerMemberName] string testName = "")
    //{
    //    return IntegrationTestConfiguration.TestFilters.ContainsKey($"{GetType().Name}.{testName}");
    //}

    //protected string TestFilterList([CallerMemberName] string testName = "")
    //{
    //    var testFilterKey = $"{GetType().Name}.{testName}";
    //    MethodBase? method = GetType().GetMethod(testName);

    //    FilterFormatAttribute attr = (FilterFormatAttribute)method!.GetCustomAttributes(typeof(FilterFormatAttribute), true)[0];
    //    string filterFormat = attr.Format;
    //    var regex = new Regex(filterFormat);

    //    var testFilters = IntegrationTestConfiguration.TestFilters[testFilterKey];

    //    testFilters.ForEach(f =>
    //    {
    //        if (!regex.Match(f).Success)
    //        {
    //            throw new Exception($"Bad filter for {testFilterKey} - '{f}'. Should match regex [{filterFormat}]. Check appsettings.json file.");
    //        }
    //    });

    //    return "( " + string.Join(", ", testFilters.Select(e => $"'{e}'")) + " )";
    //}

    protected string FormattedFailureMessage(string message, IEnumerable<string> failureCases, [CallerMemberName] string testName = "")
    {
        var testFilterKey = $"{GetType().Name}.{testName}";

        var filterHint = new StringBuilder($"{Environment.NewLine}{Environment.NewLine}");
        filterHint.AppendLine("If you have a valid reason to filter this result out of the test then add something similar to the following to the configuration in appsettings.");
        filterHint.AppendLine();
        filterHint.AppendLine("\"TestFilters\": {");
        filterHint.AppendLine($"\t\"{testFilterKey}\": [");
        filterHint.AppendLine($"\t\t\"{failureCases.FirstOrDefault()}\"");
        filterHint.AppendLine("\t],");
        filterHint.AppendLine("}");
        filterHint.AppendLine();

        return $"{message}{Environment.NewLine}{string.Join($",{Environment.NewLine}", failureCases.Select(c => $"\t'{c}'"))}{filterHint}";
    }
}
