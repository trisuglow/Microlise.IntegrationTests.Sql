using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using NUnit.Framework.Internal.Commands;
using System.Reflection;
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
            var result = invocable.Invoke();
            Record(frameworkHandle, test, result.Outcome, result.ErrorMessage, result.ErrorStackTrace, DateTime.UtcNow - started);
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

internal readonly record struct TestInvocationResult(
    TestOutcome Outcome,
    string? ErrorMessage,
    string? ErrorStackTrace)
{
    public static TestInvocationResult Passed { get; } = new(TestOutcome.Passed, null, null);
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


        public TestInvocationResult Invoke()
        {
            var instance = Activator.CreateInstance(fixtureType)
                           ?? throw new InvalidOperationException($"Could not create fixture '{fixtureType.FullName}'.");

            var setupSucceeded = false;
            TestExecutionContext? executionContext = null;

            try
            {
                InvokeAttributed(instance, typeof(OneTimeSetUpAttribute));

                using (new TestExecutionContext.IsolatedContext())
                {
                    IMethodInfo nUnitMethod = new MethodWrapper(fixtureType, method);
                    TestCaseParameters? parms = arguments.Length > 0 ? new TestCaseParameters(arguments) : null;
                    var test = new NUnitTestCaseBuilder().BuildTestMethod(nUnitMethod, parentSuite: null, parms);

                    var context = TestExecutionContext.CurrentContext;
                    executionContext = context;
                    context.CurrentTest = test;
                    context.CurrentResult = test.MakeTestResult();
                    context.TestObject = instance;
                    context.EstablishExecutionEnvironment();

                    try
                    {
                        InvokeAttributed(instance, typeof(SetUpAttribute));
                        setupSucceeded = true;

                        new TestMethodCommand(test).Execute(context);
                        return TestInvocationResult.Passed;
                    }
                    finally
                    {
                        if (setupSucceeded)
                        {
                            InvokeAttributed(instance, typeof(TearDownAttribute));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return DescribeFailure(ex, executionContext);
            }
            finally
            {
                InvokeAttributed(instance, typeof(OneTimeTearDownAttribute));
                (instance as IDisposable)?.Dispose();
            }
        }

        private static TestInvocationResult DescribeFailure(Exception ex, TestExecutionContext? context)
        {
            if (context?.CurrentResult is { } result)
            {
                if (result.AssertionResults.Count > 0)
                {
                    result.RecordTestCompletion();
                }

                if (result.ResultState.Status != TestStatus.Passed && !string.IsNullOrWhiteSpace(result.Message))
                {
                    return new TestInvocationResult(
                        MapOutcome(result.ResultState),
                        result.Message,
                        result.StackTrace);
                }
            }

            var failure = UnwrapException(ex);
            return new TestInvocationResult(
                MapOutcome(failure),
                ExceptionHelper.BuildMessage(failure, excludeExceptionNames: true),
                ExceptionHelper.BuildStackTrace(failure));
        }

        private static Exception UnwrapException(Exception ex)
        {
            for (;;)
            {
                var unwrapped = ex switch
                {
                    TargetInvocationException { InnerException: { } inner } => inner,
                    NUnitException { InnerException: { } inner } nunit when nunit.Message == "Rethrown" => inner,
                    _ => null
                };

                if (unwrapped is null)
                {
                    return ex;
                }

                ex = unwrapped;
            }
        }

        private static TestOutcome MapOutcome(ResultState resultState) => resultState.Status switch
        {
            TestStatus.Passed => TestOutcome.Passed,
            TestStatus.Skipped => TestOutcome.Skipped,
            TestStatus.Inconclusive => TestOutcome.Skipped,
            _ => TestOutcome.Failed
        };

        private static TestOutcome MapOutcome(Exception ex) => ex switch
        {
            IgnoreException or InconclusiveException => TestOutcome.Skipped,
            SuccessException => TestOutcome.Passed,
            _ => TestOutcome.Failed
        };

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
