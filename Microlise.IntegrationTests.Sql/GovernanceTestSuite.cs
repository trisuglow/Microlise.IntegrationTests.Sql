using Microlise.IntegrationTests.Sql.GovernanceTests;
using Microlise.IntegrationTests.Sql.GovernanceTests.BaseClass;
using Microlise.IntegrationTests.Sql.GovernanceTests.Filters;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace Microlise.IntegrationTests.Sql
{
    public class GovernanceTestSuite : TransactionScopedTests
    {
        private readonly Dictionary<Type, GovernanceTestBase> _governanceTests = [];

        private void AddTest(GovernanceTestBase test)
        {
            _governanceTests.Add(test.GetType(), test);
        }

        public GovernanceTestSuite()
        {
            AddTest(new DatabaseNameTest());
            AddTest(new DoNotUseSelectStarTest());
            AddTest(new UseDatetime2Test());
            AddTest(new PrimaryKeyOnAllTablesTest());
        }

        public void RunTests()
        {
            Assert.Multiple(() =>
            {
                foreach (var governanceTest in _governanceTests)
                {
                    var type = governanceTest.Value.GetType();
                    if (type.GetCustomAttributes(typeof(FilterFormatAttribute), false).FirstOrDefault() is FilterFormatAttribute filterFormatAttribute)
                    {
                        var regex = new Regex(filterFormatAttribute.Format);

                        if (governanceTest.Value._testFilter is not null)
                        {
                            foreach (string filterPattern in governanceTest.Value._testFilter.FilterList.Keys)
                            {
                                if (!regex.Match(filterPattern).Success)
                                {
                                    throw new Exception($"Badly formatted filter '{filterPattern}' for {type.Name}.");
                                }
                            }
                        }
                    }

                    governanceTest.Value.RunTest();
                }
            });
        }

        public GovernanceTestSuite FilterTest<T>(TestFilter testOverrideDetails)
        {
            if (!_governanceTests.ContainsKey(typeof(T)))
            {
                return this;
            }

            foreach (var filter in testOverrideDetails.FilterList)
            {
                if (_governanceTests[typeof(T)]._testFilter is null)
                {
                    _governanceTests[typeof(T)]._testFilter = new();
                }

                _governanceTests[typeof(T)]._testFilter!.FilterList.Add(filter.Key, filter.Value);
            }

            return this;
        }
    }
}
