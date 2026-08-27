# Introduction

This library is intended to be added to NUnit3 SQL integration test projects as a NuGet reference.

It provides the following features:
1. A reference to the integration test database, using the Dapper micro-ORM, wrapped in a transaction scope that rolls back changes made during individual tests.
1. A set of standard test features that can be used in the test project.
1. A suite of governance tests. These are asserted in addition to specific tests written in the test project. They are based on Data Governance Council (DGC) guidance and would usually be expected to all pass in order for code to pass a DGC review.

# How to use the libary
The library is available as a NuGet package - Microlise.IntegrationTests.Sql. For solutions that already have an integration test project, simply reference the latest version of the library.

In order for the library to find the integration test database there must be a section in the test project's appsettings.json that specifies this.
```json
{
  "IntegrationTestLibraryConfiguration": {
    "ConnectionString": "Server=localhost;Database=IntegrationTestLibrary;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
  }
}
```

Make sure that the appsettings.json file is marked as `Content` and `Copy to Output Directory if newer`.

No other setup is required - governance tests should now run alongside the other tests in the integration test project.

For some projects there will be valid reasons why some of the governance tests will not apply. In order to override tests add filters in a TestFilters section in the appsettings.json. See the appsettings.json file in Template.Integration.Tests in this repository for examples.

See the ExampleTests.cs file in Template.Integration.Tests for examples of how to use the assertions provided by the library (such as `AssertEnumMatchesDatabase`).