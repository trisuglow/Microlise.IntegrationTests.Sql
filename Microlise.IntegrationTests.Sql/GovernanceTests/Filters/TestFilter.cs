using System.Text;

namespace Microlise.IntegrationTests.Sql.GovernanceTests.Filters;

public class TestFilter
{
    public Dictionary<string, string> FilterList { get; private set; } = [];

    public TestFilter AddFilter(string filter, string justification)
    {
        if (justification.Length < 20)
        {
            throw new ArgumentException("A justification must be provided when filtering a test. We consider you'll need at least twenty characters to do this.");
        }

        FilterList.Add(filter, justification);

        return this;
    }

    public string Justification
    {
        get
        {
            var justification = new StringBuilder(Environment.NewLine);
            foreach (var filter in FilterList)
            {
                justification.AppendLine($"\tFiltering '{filter.Key}' from test because {filter.Value}");
            }

            return justification.ToString();
        }
    }
}