using System.Reflection;
using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using VsTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace Microlise.IntegrationTests.Sql;

[FileExtension(".dll")]
[DefaultExecutorUri(ExecutorUri)]
public sealed class ReferencedNUnitTestDiscoverer : ITestDiscoverer
{
    public const string ExecutorUri = "executor://MicroliseIntegrationTestsSql/v1";

    public void DiscoverTests(
        IEnumerable<string> sources,
        IDiscoveryContext discoveryContext,
        IMessageLogger logger,
        ITestCaseDiscoverySink discoverySink)
    {
        _ = discoveryContext;
        try
        {
            logger.SendMessage(TestMessageLevel.Informational, "Trying to discover Microlise.IntegrationTests.Sql tests.");
            var source = ChooseSource(sources);
            foreach (var test in ReferencedNUnitCatalog.CreateTestCases(source))
            {
                discoverySink.SendTestCase(test);
            }
        }
        catch (Exception ex)
        {
            logger.SendMessage(TestMessageLevel.Error, $"Failed to discover Microlise.IntegrationTests.Sql tests: {ex}");
        }
    }

    internal static string ChooseSource(IEnumerable<string>? sources)
    {
        var list = sources as IList<string> ?? sources?.ToArray() ?? [];

        var testProject = list.FirstOrDefault(path =>
        {
            var name = Path.GetFileName(path);
            Console.WriteLine($"Found {name}.");
            return name.Contains(".Tests.", StringComparison.OrdinalIgnoreCase)
                   && name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                   && !name.EndsWith(".TestAdapter.dll", StringComparison.OrdinalIgnoreCase);
        });

        return testProject
               ?? list.FirstOrDefault(path =>
                   path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                   && !path.EndsWith(".TestAdapter.dll", StringComparison.OrdinalIgnoreCase))
               ?? typeof(GovernanceTestBase).Assembly.Location;
    }
}

[ExtensionUri(ReferencedNUnitTestDiscoverer.ExecutorUri)]
public sealed class ReferencedNUnitTestExecutor : ITestExecutor
{
    private volatile bool _cancelled;

    public void Cancel() => _cancelled = true;

    public void RunTests(IEnumerable<string>? sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
    {
        var source = ReferencedNUnitTestDiscoverer.ChooseSource(sources);
        RunTests(ReferencedNUnitCatalog.CreateTestCases(source), runContext, frameworkHandle);
    }

    public void RunTests(IEnumerable<VsTestCase>? tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
    {
        _ = runContext;
        _cancelled = false;
        if (tests is null)
        {
            return;
        }

        var catalog = ReferencedNUnitCatalog.CreateInvocables().ToDictionary(item => item.FullyQualifiedName, StringComparer.Ordinal);

        foreach (var test in tests)
        {
            if (_cancelled)
            {
                break;
            }

            if (!catalog.TryGetValue(test.FullyQualifiedName, out var invocable))
            {
                Record(frameworkHandle, test, TestOutcome.NotFound, $"Unknown test '{test.FullyQualifiedName}'.", null);
                continue;
            }

            var started = DateTime.UtcNow;
            frameworkHandle.RecordStart(test);
            try
            {
                invocable.Invoke();
                Record(frameworkHandle, test, TestOutcome.Passed, null, null, DateTime.UtcNow - started);
            }
            catch (Exception ex)
            {
                var inner = ex is TargetInvocationException { InnerException: { } target } ? target : ex;
                var outcome = inner switch
                {
                    IgnoreException or InconclusiveException => TestOutcome.Skipped,
                    SuccessException => TestOutcome.Passed,
                    ResultStateException => TestOutcome.Failed,
                    _ => TestOutcome.Failed
                };
                Record(frameworkHandle, test, outcome, inner.Message, inner.StackTrace, DateTime.UtcNow - started);
            }
        }
    }

    private static void Record(
        IFrameworkHandle frameworkHandle,
        VsTestCase test,
        TestOutcome outcome,
        string? message,
        string? stackTrace,
        TimeSpan? duration = null)
    {
        var result = new VsTestResult(test)
        {
            Outcome = outcome,
            ErrorMessage = message,
            ErrorStackTrace = stackTrace,
            Duration = duration ?? TimeSpan.Zero,
            DisplayName = test.DisplayName
        };
        frameworkHandle.RecordEnd(test, outcome);
        frameworkHandle.RecordResult(result);
    }
}

internal static class ReferencedNUnitCatalog
{
    private static readonly Uri Executor = new(ReferencedNUnitTestDiscoverer.ExecutorUri);

    public static IReadOnlyList<VsTestCase> CreateTestCases(string source)
        => CreateInvocables().Select(item => item.ToTestCase(source)).ToArray();

    public static IReadOnlyList<InvocableTest> CreateInvocables()
    {
        var assembly = typeof(GovernanceTestBase).Assembly;
        var invocables = new List<InvocableTest>();

        foreach (var type in assembly.GetExportedTypes().Where(type => type.IsClass && !type.IsAbstract))
        {
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                var testCases = method.GetCustomAttributes<TestCaseAttribute>(inherit: true).ToArray();
                if (testCases.Length > 0)
                {
                    for (var index = 0; index < testCases.Length; index++)
                    {
                        invocables.Add(new InvocableTest(type, method, testCases[index].Arguments ?? [], index));
                    }

                    continue;
                }

                if (method.GetCustomAttribute<TestAttribute>(inherit: true) is not null)
                {
                    invocables.Add(new InvocableTest(type, method, [], index: 0));
                }
            }
        }

        return invocables;
    }

    internal sealed class InvocableTest(Type fixtureType, MethodInfo method, object?[] arguments, int index)
    {
        public string FullyQualifiedName { get; } = arguments.Length == 0
            ? $"{fixtureType.FullName}.{method.Name}"
            : $"{fixtureType.FullName}.{method.Name}({string.Join(", ", arguments.Select(FormatArg))})#{index}";

        public VsTestCase ToTestCase(string source)
        {
            return new VsTestCase(FullyQualifiedName, Executor, source)
            {
                DisplayName = arguments.Length == 0
                    ? method.Name
                    : $"{method.Name}({string.Join(", ", arguments.Select(FormatArg))})",
                CodeFilePath = null
            };
        }


        

        public void Invoke()
        {
            var instance = Activator.CreateInstance(fixtureType)
                           ?? throw new InvalidOperationException($"Could not create fixture '{fixtureType.FullName}'.");

            try
            {
                InvokeAttributed(instance, typeof(OneTimeSetUpAttribute));
                InvokeAttributed(instance, typeof(SetUpAttribute));

                /*
                 
                 The following code replaces method.Invoke(... and is intended to put each test in its own context,
                to prevent test failures accumulating as multiples for every new test that fails.
                  
                IMethodInfo nUnitMethod = new MethodWrapper(method.DeclaringType, method);                

                var test = new TestMethod(nUnitMethod);
                var context = new TestExecutionContext()
                {
                    CurrentTest = test,
                    TestObject = instance,
                };

                TestCommand command = new TestMethodCommand(test);

                command.Execute(context);

                */
                method.Invoke(instance, arguments.Length == 0 ? null : arguments);
            }
            finally
            {
                InvokeAttributed(instance, typeof(TearDownAttribute));
                InvokeAttributed(instance, typeof(OneTimeTearDownAttribute));
                (instance as IDisposable)?.Dispose();
            }
        }

        private static void InvokeAttributed(object instance, Type attributeType)
        {
            foreach (var setup in instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                         .Where(candidate => candidate.GetCustomAttributes(attributeType, inherit: true).Length > 0))
            {
                setup.Invoke(instance, null);
            }
        }

        private static string FormatArg(object? value) => value switch
        {
            null => "null",
            string text => $"\"{text}\"",
            char character => $"'{character}'",
            _ => Convert.ToString(value) ?? "null"
        };
    }
}
