using Microlise.IntegrationTests.Sql;
using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using System.Reflection;

namespace Template.Integration.Tests;

internal class GovernanceTestTests
{
    IEnumerable<MethodInfo>? _governanceTests;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var testClasses = Assembly.GetAssembly(typeof(GovernanceTestBase))?
            .GetTypes().Where(t => t.IsSubclassOf(typeof(GovernanceTestBase)) && !t.IsAbstract);

        _governanceTests = testClasses!.SelectMany(tc => tc.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            .Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(TestAttribute)));

    }

    [Test]
    public void AllTestsHaveFilterAttribute()
    {
        Assert.That(
            _governanceTests!.All(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(FilterFormatAttribute))),
            Is.True,
            $"All GovernanceTestBase tests must have a {nameof(FilterFormatAttribute)} to specify the format of the filter that can exclude them from the test run.");
    }

    [Test]
    public void AllTestsAreNamedAsTest()
    {
        Assert.That(
            _governanceTests!.All(t => t.Name.EndsWith("Test")),
            Is.True,
            $"All GovernanceTestBase tests must have a 'Test' suffix on their name.");
    }

    [Test]
    public void AllTestsGenerateLengthyFailureExplanation()
    {
    }
}
